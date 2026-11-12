using System.Text;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using AcRuntimeException = Autodesk.AutoCAD.Runtime.Exception;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Civil3DMcpPlugin;

/// <summary>
/// Read-only drawing comparison. The other drawing is opened as a side
/// database with Database.ReadDwgFile (never as a document), fingerprinted,
/// and released immediately. Snapshots are fingerprints written through
/// FileBoundary (export roots, .json only, atomic, overwrite=false default)
/// and read back through FileBoundary (import or export roots, .json only).
/// Nothing here modifies any drawing.
/// </summary>
public static class CompareCommands
{
  private const int DefaultMaxDetails = 200;
  private const int MaxRowsPerSummaryArray = 2000;
  private const long MaxSnapshotBytes = 256L * 1024 * 1024;

  // Above this many layout entities a capture switches to fast detail (see
  // DrawingFingerprint.EntityDetail), so a very large drawing does not hold
  // Civil 3D for an extents computation per curve.
  internal const int FastEntityThreshold = 100_000;

  // Surfaces up to this many vertices get a full vertex hash; larger ones
  // keep the statistics, triangle count and areas only.
  internal const int MaxHashedSurfaceVertices = 2_000_000;

  public static async Task<object?> CompareDrawingsAsync(JsonObject? parameters)
  {
    var rawPath = PluginRuntime.GetRequiredString(parameters, "otherPath");
    var otherPath = FileBoundary.ResolveImportPath(rawPath, ".dwg");
    var maxDetails = PluginRuntime.GetOptionalInt(parameters, "maxDetails") ?? DefaultMaxDetails;
    var includeCivil = PluginRuntime.GetOptionalBool(parameters, "includeCivilSummaries") ?? true;

    return await CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      using var side = OpenSideDatabase(otherPath, includeCivil, out var sideCivil, out var sideWarnings);
      using var sideTransaction = side.TransactionManager.StartOpenCloseTransaction();
      // One detail level for both drawings, chosen by the larger of the two.
      var fast = Math.Max(CountLayoutEntities(database, transaction), CountLayoutEntities(side, sideTransaction)) > FastEntityThreshold;
      var current = Capture(database, transaction, civilDoc, database.Filename, "active_drawing", includeCivil, fast);
      var baseline = Capture(side, sideTransaction, sideCivil, otherPath, "side_database", includeCivil && sideCivil != null, fast);
      baseline.Warnings.InsertRange(0, sideWarnings);
      var result = DrawingFingerprintDiff.Compare(baseline, current, maxDetails);
      result["mode"] = "drawing";
      return result;
    });
  }

  public static async Task<object?> WriteDrawingSnapshotAsync(JsonObject? parameters)
  {
    var outputPath = PluginRuntime.GetRequiredString(parameters, "outputPath");
    var overwrite = PluginRuntime.GetOptionalBool(parameters, "overwrite") ?? false;
    var includeCivil = PluginRuntime.GetOptionalBool(parameters, "includeCivilSummaries") ?? true;

    // Validate the destination before touching the host so a bad path fails fast.
    FileBoundary.ResolveExportPath(outputPath, overwrite, ".json");

    var fingerprint = await CivilExecution.ReadAsync((doc, civilDoc, database, transaction) =>
      Capture(database, transaction, civilDoc, database.Filename, "active_drawing", includeCivil,
        CountLayoutEntities(database, transaction) > FastEntityThreshold));

    // Written outside the host gate: file IO never holds up Civil 3D.
    var json = fingerprint.ToJson();
    var written = FileBoundary.WriteAllTextAtomic(outputPath, json, new UTF8Encoding(false), overwrite, ".json");

    return new Dictionary<string, object?>
    {
      ["outputPath"] = written,
      ["schema"] = fingerprint.Schema,
      ["capturedAtUtc"] = fingerprint.CapturedAtUtc,
      ["sourcePath"] = fingerprint.SourcePath,
      ["entityDetail"] = fingerprint.EntityDetail,
      ["entityCount"] = fingerprint.Entities.Count,
      ["civilObjectCount"] = fingerprint.CivilObjects.Count,
      ["bytes"] = Encoding.UTF8.GetByteCount(json),
      ["contentHash"] = DrawingFingerprint.Hash(json),
      ["warnings"] = fingerprint.Warnings,
    };
  }

  public static async Task<object?> CompareDrawingSnapshotAsync(JsonObject? parameters)
  {
    var rawPath = PluginRuntime.GetRequiredString(parameters, "snapshotPath");
    // Snapshots are written through the export roots: read them back from
    // either root set.
    var snapshotPath = FileBoundary.ResolveReadBackPath(rawPath, ".json");
    var maxDetails = PluginRuntime.GetOptionalInt(parameters, "maxDetails") ?? DefaultMaxDetails;
    var includeCivil = PluginRuntime.GetOptionalBool(parameters, "includeCivilSummaries") ?? true;

    DrawingFingerprint baseline;
    try
    {
      var length = new FileInfo(snapshotPath).Length;
      if (length > MaxSnapshotBytes)
      {
        throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", $"Snapshot is {length} bytes; the limit is {MaxSnapshotBytes}.");
      }

      baseline = DrawingFingerprint.FromJson(await File.ReadAllTextAsync(snapshotPath, Encoding.UTF8));
    }
    catch (IOException exception)
    {
      throw new JsonRpcDispatchException("CIVIL3D.FILE_IO_ERROR", $"Unable to read snapshot '{snapshotPath}': {exception.Message}");
    }

    return await CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      // Capture at the snapshot's own detail level so unchanged entities match.
      var current = Capture(database, transaction, civilDoc, database.Filename, "active_drawing", includeCivil, baseline.IsFast);
      if (!includeCivil)
      {
        baseline.CivilObjects.Clear();
      }

      var result = DrawingFingerprintDiff.Compare(baseline, current, maxDetails);
      result["mode"] = "snapshot";
      result["snapshotPath"] = snapshotPath;
      return result;
    });
  }

  // ---------------------------------------------------------------------------
  // Capture
  // ---------------------------------------------------------------------------

  // The caller disposes the returned database.
  private static Database OpenSideDatabase(string path, bool includeCivil, out CivilDocument? sideCivil, out List<string> warnings)
  {
    var side = new Database(false, true);
    try
    {
      ReadSideDatabase(side, path);
    }
    catch
    {
      side.Dispose();
      throw;
    }

    sideCivil = null;
    warnings = new List<string>();
    if (includeCivil)
    {
      try
      {
        sideCivil = CivilDocument.GetCivilDocument(side);
      }
      catch (Exception exception)
      {
        warnings.Add($"Civil 3D objects in the other drawing could not be read: {exception.Message}");
      }
    }

    return side;
  }

  private static void ReadSideDatabase(Database side, string path)
  {
    try
    {
      side.ReadDwgFile(path, FileOpenMode.OpenForReadAndAllShare, true, string.Empty);
    }
    catch (AcRuntimeException)
    {
      // A drawing open in this session is write-locked; fall back to the
      // most permissive read share AutoCAD offers.
      try
      {
        side.ReadDwgFile(path, FileOpenMode.OpenTryForReadShare, true, string.Empty);
      }
      catch (AcRuntimeException exception)
      {
        throw new JsonRpcDispatchException("CIVIL3D.FILE_IO_ERROR", $"Unable to read '{path}' as a side database: {exception.Message}");
      }
    }

    // Load everything into memory and release the file handle at once.
    side.CloseInput(true);
  }

  // Entities in all layouts (model and paper space), counted by id only.
  private static int CountLayoutEntities(Database database, Transaction transaction)
  {
    var count = 0;
    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
    foreach (ObjectId btrId in blockTable)
    {
      var btr = (BlockTableRecord)transaction.GetObject(btrId, OpenMode.ForRead);
      if (!btr.IsLayout)
      {
        continue;
      }

      foreach (ObjectId _ in btr)
      {
        count++;
      }
    }

    return count;
  }

  internal static DrawingFingerprint Capture(
    Database database,
    Transaction transaction,
    CivilDocument? civilDoc,
    string? sourcePath,
    string sourceKind,
    bool includeCivil,
    bool fast)
  {
    var fingerprint = new DrawingFingerprint
    {
      CapturedAtUtc = DateTime.UtcNow.ToString("O"),
      SourcePath = sourcePath,
      SourceKind = sourceKind,
      EntityDetail = fast ? DrawingFingerprint.FastDetail : DrawingFingerprint.FullDetail,
    };

    if (fast)
    {
      fingerprint.Warnings.Add(
        $"More than {FastEntityThreshold} layout entities: lines were fingerprinted without their extents (their start and end points still are), to keep Civil 3D responsive.");
    }

    CaptureEntities(database, transaction, fingerprint, fast);

    if (includeCivil && civilDoc != null)
    {
      CaptureCivil(civilDoc, transaction, fingerprint);
    }

    return fingerprint;
  }

  private static void CaptureEntities(Database database, Transaction transaction, DrawingFingerprint fingerprint, bool fast)
  {
    var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
    var skipped = 0;
    foreach (ObjectId btrId in blockTable)
    {
      var btr = (BlockTableRecord)transaction.GetObject(btrId, OpenMode.ForRead);
      if (!btr.IsLayout)
      {
        continue;
      }

      var space = btr.Name;
      try
      {
        if (!btr.LayoutId.IsNull && transaction.GetObject(btr.LayoutId, OpenMode.ForRead) is Layout layout)
        {
          space = layout.LayoutName;
        }
      }
      catch (Exception exception) when (exception is not JsonRpcDispatchException)
      {
      }

      foreach (ObjectId entityId in btr)
      {
        if (entityId.IsErased)
        {
          continue;
        }

        try
        {
          if (transaction.GetObject(entityId, OpenMode.ForRead) is not Autodesk.AutoCAD.DatabaseServices.Entity entity)
          {
            continue;
          }

          var type = entityId.ObjectClass?.DxfName;
          if (string.IsNullOrWhiteSpace(type))
          {
            type = entityId.ObjectClass?.Name ?? entity.GetType().Name;
          }

          fingerprint.Entities.Add(new EntityPrint
          {
            Handle = entity.Handle.ToString(),
            Type = type,
            Layer = entity.Layer,
            Space = space,
            Hash = DrawingFingerprint.Hash(EntitySignature(entity, type, transaction, fast)),
          });
        }
        catch (Exception exception) when (exception is not JsonRpcDispatchException)
        {
          skipped++;
        }
      }
    }

    if (skipped > 0)
    {
      fingerprint.Warnings.Add($"{skipped} entities could not be opened and were skipped.");
    }
  }

  private static string EntitySignature(Autodesk.AutoCAD.DatabaseServices.Entity entity, string type, Transaction transaction, bool fast)
  {
    var builder = new StringBuilder(160);
    builder.Append(type).Append('|').Append(entity.Layer)
      .Append('|').Append(entity.Color.ColorIndex)
      .Append('|').Append(entity.Linetype)
      .Append('|').Append((int)entity.LineWeight)
      .Append('|').Append(entity.Visible ? '1' : '0');

    // Fast detail skips the extents of lines only: a line's start and end
    // (below) define it completely. Other curves keep their extents (a
    // polyline can move an interior vertex and keep its start, end and
    // length), and so do block references (extents show a redefined block).
    if (!(fast && entity is Line))
    {
      try
      {
        var extents = entity.GeometricExtents;
        builder.Append("|ext:")
          .Append(DrawingFingerprint.Num(extents.MinPoint.X)).Append(',')
          .Append(DrawingFingerprint.Num(extents.MinPoint.Y)).Append(',')
          .Append(DrawingFingerprint.Num(extents.MinPoint.Z)).Append(',')
          .Append(DrawingFingerprint.Num(extents.MaxPoint.X)).Append(',')
          .Append(DrawingFingerprint.Num(extents.MaxPoint.Y)).Append(',')
          .Append(DrawingFingerprint.Num(extents.MaxPoint.Z));
      }
      catch (Exception exception) when (exception is not JsonRpcDispatchException)
      {
        builder.Append("|ext:none");
      }
    }

    try
    {
      switch (entity)
      {
        case DBText text:
          builder.Append("|text:").Append(text.TextString);
          break;
        case MText mtext:
          builder.Append("|mtext:").Append(mtext.Contents);
          break;
        case BlockReference reference:
          builder.Append("|block:").Append(reference.Name)
            .Append('@').Append(DrawingFingerprint.Num(reference.Position.X))
            .Append(',').Append(DrawingFingerprint.Num(reference.Position.Y))
            .Append(',').Append(DrawingFingerprint.Num(reference.Position.Z))
            .Append("|rot:").Append(DrawingFingerprint.Num(reference.Rotation))
            .Append("|scl:").Append(DrawingFingerprint.Num(reference.ScaleFactors.X))
            .Append(',').Append(DrawingFingerprint.Num(reference.ScaleFactors.Y));
          // Attribute references are owned by the block reference, not the
          // layout, so they are never fingerprinted on their own: fold their
          // tags and values in here so title-block edits show as modified.
          foreach (ObjectId attributeId in reference.AttributeCollection)
          {
            if (!attributeId.IsErased &&
                transaction.GetObject(attributeId, OpenMode.ForRead) is AttributeReference attribute)
            {
              builder.Append("|att:").Append(attribute.Tag).Append('=').Append(attribute.TextString);
            }
          }

          break;
        case Dimension dimension:
          builder.Append("|dim:").Append(DrawingFingerprint.Num(dimension.Measurement)).Append(':').Append(dimension.DimensionText);
          break;
        case Curve curve:
          builder.Append("|len:").Append(DrawingFingerprint.Num(curve.GetDistanceAtParameter(curve.EndParam)))
            .Append("|start:").Append(DrawingFingerprint.Num(curve.StartPoint.X)).Append(',').Append(DrawingFingerprint.Num(curve.StartPoint.Y)).Append(',').Append(DrawingFingerprint.Num(curve.StartPoint.Z))
            .Append("|end:").Append(DrawingFingerprint.Num(curve.EndPoint.X)).Append(',').Append(DrawingFingerprint.Num(curve.EndPoint.Y)).Append(',').Append(DrawingFingerprint.Num(curve.EndPoint.Z));
          break;
      }
    }
    catch (Exception exception) when (exception is not JsonRpcDispatchException)
    {
      builder.Append("|detail:unavailable");
    }

    return builder.ToString();
  }

  private static void CaptureCivil(CivilDocument civilDoc, Transaction transaction, DrawingFingerprint fingerprint)
  {
    void Guard(string what, Action action)
    {
      try
      {
        action();
      }
      catch (Exception exception) when (exception is not JsonRpcDispatchException)
      {
        fingerprint.Warnings.Add($"{what}: {exception.Message}");
      }
    }

    Guard("alignments", () =>
    {
      foreach (ObjectId id in civilDoc.GetAlignmentIds())
      {
        Guard($"alignment {id.Handle}", () =>
        {
          var alignment = (Alignment)transaction.GetObject(id, OpenMode.ForRead);
          fingerprint.CivilObjects.Add(DescribeAlignment(alignment));
          foreach (ObjectId profileId in alignment.GetProfileIds())
          {
            Guard($"profile {profileId.Handle}", () =>
            {
              var profile = (Profile)transaction.GetObject(profileId, OpenMode.ForRead);
              fingerprint.CivilObjects.Add(DescribeProfile(profile, alignment.Name));
            });
          }
        });
      }
    });

    Guard("surfaces", () =>
    {
      foreach (ObjectId id in civilDoc.GetSurfaceIds())
      {
        Guard($"surface {id.Handle}", () =>
        {
          var surface = (CivilSurface)transaction.GetObject(id, OpenMode.ForRead);
          fingerprint.CivilObjects.Add(DescribeSurface(surface));
        });
      }
    });

    Guard("pipe networks", () =>
    {
      foreach (ObjectId id in civilDoc.GetPipeNetworkIds())
      {
        Guard($"pipe network {id.Handle}", () =>
        {
          var network = (Network)transaction.GetObject(id, OpenMode.ForRead);
          fingerprint.CivilObjects.Add(DescribePipeNetwork(network, transaction));
        });
      }
    });

    Guard("corridors", () =>
    {
      foreach (ObjectId id in civilDoc.CorridorCollection)
      {
        Guard($"corridor {id.Handle}", () =>
        {
          var corridor = (Corridor)transaction.GetObject(id, OpenMode.ForRead);
          var summary = new JsonObject
          {
            ["baselineCount"] = corridor.Baselines.Count,
            ["style"] = corridor.StyleName,
          };
          fingerprint.CivilObjects.Add(Print("corridor", corridor.Name, corridor, summary));
        });
      }
    });
  }

  private static CivilObjectPrint Print(string kind, string name, Autodesk.AutoCAD.DatabaseServices.Entity entity, JsonObject summary)
  {
    summary["layer"] = entity.Layer;
    return new CivilObjectPrint
    {
      Kind = kind,
      Name = name,
      Handle = entity.Handle.ToString(),
      Layer = entity.Layer,
      Hash = DrawingFingerprint.Hash(summary.ToJsonString()),
      Summary = summary,
    };
  }

  private static CivilObjectPrint DescribeAlignment(Alignment alignment)
  {
    var entityTypes = new List<string>();
    for (var index = 0; index < alignment.Entities.Count; index++)
    {
      entityTypes.Add(alignment.Entities.GetEntityByOrder(index).EntityType.ToString());
    }

    // Geometry hash: entity sequence plus 65 evenly spaced centerline points.
    var geometry = new StringBuilder(string.Join(",", entityTypes));
    const int intervals = 64;
    var span = alignment.EndingStation - alignment.StartingStation;
    for (var step = 0; step <= intervals; step++)
    {
      var station = alignment.StartingStation + span * step / intervals;
      double easting = 0, northing = 0;
      try
      {
        alignment.PointLocation(station, 0, ref easting, ref northing);
        geometry.Append('|').Append(DrawingFingerprint.Num(Math.Round(easting, 4))).Append(',').Append(DrawingFingerprint.Num(Math.Round(northing, 4)));
      }
      catch (Exception exception) when (exception is not JsonRpcDispatchException)
      {
        geometry.Append("|x");
      }
    }

    var summary = new JsonObject
    {
      ["length"] = Round(alignment.Length),
      ["startStation"] = Round(alignment.StartingStation),
      ["endStation"] = Round(alignment.EndingStation),
      ["entityCount"] = entityTypes.Count,
      ["site"] = SafeText(() => alignment.SiteName),
      ["style"] = SafeText(() => alignment.StyleName),
      ["geometryHash"] = DrawingFingerprint.Hash(geometry.ToString()),
    };
    return Print("alignment", alignment.Name, alignment, summary);
  }

  private static CivilObjectPrint DescribeProfile(Profile profile, string alignmentName)
  {
    var pvis = new JsonArray();
    var pviText = new StringBuilder();
    var count = 0;
    foreach (ProfilePVI pvi in profile.PVIs)
    {
      var station = Round(pvi.RawStation);
      var elevation = Round(pvi.Elevation);
      pviText.Append(DrawingFingerprint.Num(station)).Append(':').Append(DrawingFingerprint.Num(elevation)).Append(';');
      if (count++ < MaxRowsPerSummaryArray)
      {
        pvis.Add(new JsonObject
        {
          ["key"] = DrawingFingerprint.Num(Math.Round(pvi.RawStation, 3)),
          ["station"] = station,
          ["elevation"] = elevation,
        });
      }
    }

    var summary = new JsonObject
    {
      ["alignment"] = alignmentName,
      ["profileType"] = profile.ProfileType.ToString(),
      ["startStation"] = Round(profile.StartingStation),
      ["endStation"] = Round(profile.EndingStation),
      ["pviCount"] = count,
      ["pviHash"] = DrawingFingerprint.Hash(pviText.ToString()),
      ["pvis"] = pvis,
    };
    if (count > MaxRowsPerSummaryArray)
    {
      summary["pvisTruncated"] = true;
    }

    // Profile names are unique per alignment, not per drawing.
    return Print("profile", $"{alignmentName}/{profile.Name}", profile, summary);
  }

  private static CivilObjectPrint DescribeSurface(CivilSurface surface)
  {
    var summary = new JsonObject
    {
      ["surfaceType"] = surface.GetType().Name,
      ["style"] = SafeText(() => surface.StyleName),
    };

    try
    {
      var properties = surface.GetGeneralProperties();
      summary["numberOfPoints"] = properties.NumberOfPoints;
      summary["minimumElevation"] = Round(properties.MinimumElevation);
      summary["maximumElevation"] = Round(properties.MaximumElevation);
      summary["meanElevation"] = Round(properties.MeanElevation);
    }
    catch (Exception exception) when (exception is not JsonRpcDispatchException)
    {
      summary["statistics"] = $"unavailable: {exception.Message}";
    }

    // Count and min/max/mean miss edits that keep them unchanged (moving an
    // interior point, flipping an edge). Triangle count and 2D/3D area catch
    // most of those; vertexHash (an order-independent hash of every vertex)
    // catches any vertex moved or re-elevated. It is one pass over the
    // vertices, skipped above MaxHashedSurfaceVertices.
    try
    {
      switch (surface)
      {
        case TinSurface tin:
          summary["triangleCount"] = tin.GetTinProperties().NumberOfTriangles;
          AddAreas(summary, tin.GetTerrainProperties());
          AddVertexHash(summary, () => tin.Vertices.Count, add =>
          {
            foreach (TinSurfaceVertex vertex in tin.Vertices)
            {
              add(vertex.Location);
            }
          });
          break;
        case GridSurface grid:
          AddAreas(summary, grid.GetTerrainProperties());
          AddVertexHash(summary, () => grid.Vertices.Count, add =>
          {
            foreach (GridSurfaceVertex vertex in grid.Vertices)
            {
              add(vertex.Location);
            }
          });
          break;
      }
    }
    catch (Exception exception) when (exception is not JsonRpcDispatchException)
    {
      summary["terrainStatistics"] = $"unavailable: {exception.Message}";
    }

    try
    {
      summary["isReference"] = surface.IsReferenceObject;
    }
    catch (Exception exception) when (exception is not JsonRpcDispatchException)
    {
    }

    return Print("surface", surface.Name, surface, summary);
  }

  private static void AddVertexHash(JsonObject summary, Func<int> count, Action<Action<Autodesk.AutoCAD.Geometry.Point3d>> forEachVertex)
  {
    try
    {
      var vertexCount = count();
      if (vertexCount > MaxHashedSurfaceVertices)
      {
        summary["vertexHash"] = $"skipped: {vertexCount} vertices (over {MaxHashedSurfaceVertices})";
        return;
      }

      var hash = new PointSetHash();
      forEachVertex(point => hash.Add(point.X, point.Y, point.Z));
      summary["vertexHash"] = hash.Value;
    }
    catch (Exception exception) when (exception is not JsonRpcDispatchException)
    {
      summary["vertexHash"] = $"unavailable: {exception.Message}";
    }
  }

  private static void AddAreas(JsonObject summary, TerrainSurfaceProperties properties)
  {
    summary["area2d"] = Round(properties.SurfaceArea2D);
    summary["area3d"] = Round(properties.SurfaceArea3D);
  }

  private static CivilObjectPrint DescribePipeNetwork(Network network, Transaction transaction)
  {
    var pipes = new JsonArray();
    var structures = new JsonArray();
    var totalLength = 0.0;
    var pipeCount = 0;
    var structureCount = 0;

    foreach (ObjectId pipeId in network.GetPipeIds())
    {
      if (transaction.GetObject(pipeId, OpenMode.ForRead) is not Pipe pipe)
      {
        continue;
      }

      pipeCount++;
      totalLength += pipe.Length2D;
      // InnerHeight is read defensively: Civil 3D throws "Retrieve attribute
      // failed" for parts that do not define it, which dropped the whole
      // network from the fingerprint. InnerDiameterOrWidth stands in only for
      // a circular section; otherwise the inverts are reported as null.
      var innerHeight = SafeValue(() => pipe.InnerHeight) is > 0 and var h
        ? h
        : string.Equals(SafeText(() => pipe.CrossSectionalShape.ToString()), "Circular", StringComparison.Ordinal)
          ? pipe.InnerDiameterOrWidth
          : (double?)null;
      var halfHeight = innerHeight / 2.0;
      if (pipes.Count < MaxRowsPerSummaryArray)
      {
        pipes.Add(new JsonObject
        {
          ["key"] = pipe.Handle.ToString(),
          ["name"] = pipe.Name,
          ["size"] = SafeText(() => pipe.PartSizeName),
          ["startInvert"] = halfHeight is double startHalf ? Round(pipe.StartPoint.Z - startHalf) : null,
          ["endInvert"] = halfHeight is double endHalf ? Round(pipe.EndPoint.Z - endHalf) : null,
          ["length2d"] = Round(pipe.Length2D),
          ["slope"] = Round(pipe.Slope),
        });
      }
    }

    foreach (ObjectId structureId in network.GetStructureIds())
    {
      if (transaction.GetObject(structureId, OpenMode.ForRead) is not Structure structure)
      {
        continue;
      }

      structureCount++;
      if (structures.Count < MaxRowsPerSummaryArray)
      {
        structures.Add(new JsonObject
        {
          ["key"] = structure.Handle.ToString(),
          ["name"] = structure.Name,
          ["rim"] = Round(structure.RimElevation),
          ["sump"] = Round(structure.SumpElevation),
          ["x"] = Round(structure.Location.X),
          ["y"] = Round(structure.Location.Y),
        });
      }
    }

    var summary = new JsonObject
    {
      ["pipeCount"] = pipeCount,
      ["structureCount"] = structureCount,
      ["totalPipeLength2d"] = Round(totalLength),
      ["pipes"] = pipes,
      ["structures"] = structures,
    };
    if (pipeCount > MaxRowsPerSummaryArray || structureCount > MaxRowsPerSummaryArray)
    {
      summary["rowsTruncated"] = true;
    }

    return Print("pipe_network", network.Name, network, summary);
  }

  private static double Round(double value) => double.IsFinite(value) ? Math.Round(value, 4) : 0.0;

  private static double? SafeValue(Func<double> read)
  {
    try
    {
      return read();
    }
    catch (Exception exception) when (exception is not JsonRpcDispatchException)
    {
      return null;
    }
  }

  private static string? SafeText(Func<string?> read)
  {
    try
    {
      return read();
    }
    catch (Exception exception) when (exception is not JsonRpcDispatchException)
    {
      return null;
    }
  }
}
