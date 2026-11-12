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
/// and read back through FileBoundary (import roots, .json only).
/// Nothing here modifies any drawing.
/// </summary>
public static class CompareCommands
{
  private const int DefaultMaxDetails = 200;
  private const int MaxRowsPerSummaryArray = 2000;
  private const long MaxSnapshotBytes = 256L * 1024 * 1024;

  public static async Task<object?> CompareDrawingsAsync(JsonObject? parameters)
  {
    var rawPath = PluginRuntime.GetRequiredString(parameters, "otherPath");
    var otherPath = FileBoundary.ResolveImportPath(rawPath, ".dwg");
    var maxDetails = PluginRuntime.GetOptionalInt(parameters, "maxDetails") ?? DefaultMaxDetails;
    var includeCivil = PluginRuntime.GetOptionalBool(parameters, "includeCivilSummaries") ?? true;

    return await CivilExecution.ReadAsync<object?>((doc, civilDoc, database, transaction) =>
    {
      var current = Capture(database, transaction, civilDoc, database.Filename, "active_drawing", includeCivil);
      var baseline = CaptureSideDatabase(otherPath, includeCivil);
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
      Capture(database, transaction, civilDoc, database.Filename, "active_drawing", includeCivil));

    // Written outside the host gate: file IO never holds up Civil 3D.
    var json = fingerprint.ToJson();
    var written = FileBoundary.WriteAllTextAtomic(outputPath, json, new UTF8Encoding(false), overwrite, ".json");

    return new Dictionary<string, object?>
    {
      ["outputPath"] = written,
      ["schema"] = fingerprint.Schema,
      ["capturedAtUtc"] = fingerprint.CapturedAtUtc,
      ["sourcePath"] = fingerprint.SourcePath,
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
    var snapshotPath = FileBoundary.ResolveImportPath(rawPath, ".json");
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
      var current = Capture(database, transaction, civilDoc, database.Filename, "active_drawing", includeCivil);
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

  private static DrawingFingerprint CaptureSideDatabase(string path, bool includeCivil)
  {
    using var side = new Database(false, true);
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

    CivilDocument? sideCivil = null;
    var warnings = new List<string>();
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

    using var transaction = side.TransactionManager.StartOpenCloseTransaction();
    var fingerprint = Capture(side, transaction, sideCivil, path, "side_database", includeCivil && sideCivil != null);
    fingerprint.Warnings.InsertRange(0, warnings);
    return fingerprint;
  }

  internal static DrawingFingerprint Capture(
    Database database,
    Transaction transaction,
    CivilDocument? civilDoc,
    string? sourcePath,
    string sourceKind,
    bool includeCivil)
  {
    var fingerprint = new DrawingFingerprint
    {
      CapturedAtUtc = DateTime.UtcNow.ToString("O"),
      SourcePath = sourcePath,
      SourceKind = sourceKind,
    };

    CaptureEntities(database, transaction, fingerprint);

    if (includeCivil && civilDoc != null)
    {
      CaptureCivil(civilDoc, transaction, fingerprint);
    }

    return fingerprint;
  }

  private static void CaptureEntities(Database database, Transaction transaction, DrawingFingerprint fingerprint)
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
            Hash = DrawingFingerprint.Hash(EntitySignature(entity, type, transaction)),
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

  private static string EntitySignature(Autodesk.AutoCAD.DatabaseServices.Entity entity, string type, Transaction transaction)
  {
    var builder = new StringBuilder(160);
    builder.Append(type).Append('|').Append(entity.Layer)
      .Append('|').Append(entity.Color.ColorIndex)
      .Append('|').Append(entity.Linetype)
      .Append('|').Append((int)entity.LineWeight)
      .Append('|').Append(entity.Visible ? '1' : '0');

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
    // those cheaply; a full vertex hash would cost O(points) under the host
    // lock on every comparison.
    try
    {
      switch (surface)
      {
        case TinSurface tin:
          summary["triangleCount"] = tin.GetTinProperties().NumberOfTriangles;
          AddAreas(summary, tin.GetTerrainProperties());
          break;
        case GridSurface grid:
          AddAreas(summary, grid.GetTerrainProperties());
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
