using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json.Nodes;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Windows;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Civil3DMcpPlugin;

/// <summary>
/// Creates a paper-space sheet with a live model-space viewport using the command runner
/// (-LAYOUT New / MVIEW), then plots it, following a LISP chain that worked in Civil 3D. The .NET
/// Layout/Viewport object model does not configure a paper-space viewport reliably in a command
/// context (creating one on the current layout has crashed Civil 3D), so AutoCAD itself creates the
/// viewport with MVIEW. The sheet is then plotted with the detailed -PLOT chain through
/// CommandLineRunner, with FILEDIA, CMDECHO and BACKGROUNDPLOT set as civil3d_plot sets them.
/// Experimental: see the CHANGELOG entry for what is not verified yet.
/// </summary>
public static class SheetCommands
{
  public static Task<object?> CreateSheetAndPlotAsync(JsonObject? parameters)
  {
    var layoutName = PluginRuntime.GetOptionalString(parameters, "layoutName") ?? "MCP-SHEET";
    var outputPath = PluginRuntime.GetRequiredString(parameters, "outputPath");
    var cx = PluginRuntime.GetOptionalDouble(parameters, "modelCenterX") ?? 0.0;
    var cy = PluginRuntime.GetOptionalDouble(parameters, "modelCenterY") ?? 0.0;
    var w = PluginRuntime.GetOptionalDouble(parameters, "modelWidth") ?? 1000.0;
    var modelHeight = PluginRuntime.GetOptionalDouble(parameters, "modelHeight");
    var scaleDen = PluginRuntime.GetOptionalDouble(parameters, "scaleDenominator") ?? 100.0; // 1:scaleDen
    var paperSize = PluginRuntime.GetOptionalString(parameters, "paperSize") ?? "ARCH D (24.00 x 36.00 Inches)";
    var orientation = PluginRuntime.GetOptionalString(parameters, "orientation") ?? "landscape";
    var overwrite = PluginRuntime.GetOptionalBool(parameters, "overwrite") ?? false;
    if (orientation != "portrait" && orientation != "landscape")
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "Parameter 'orientation' must be 'portrait' or 'landscape'.");

    return CivilExecution.ExecuteCommandSequenceAsync<object?>(async (doc, cancellationToken) =>
    {
      // Resolve + boundary-check the output path first (same rule as plot_layouts_to_pdf).
      var resolved = FileBoundary.ResolveExportPath(outputPath, overwrite, ".pdf");

      // Everything this sets is restored afterwards: the current layout, the command-line plot
      // settings, the text quality and the drawing's current text style.
      using var sysvars = new PlotCommands.SystemVariableScope();
      sysvars.Remember("CTAB");
      sysvars.Set("FILEDIA", 0);
      sysvars.Set("CMDECHO", 0);
      sysvars.Set("BACKGROUNDPLOT", 0);
      var originalTextStyle = doc.Database.Textstyle;
      try
      {
        // 1. Ensure the layout exists and is current.
        await CommandLineRunner.RunAsync(doc, "LAYOUT", "_.-LAYOUT", "_N", layoutName);
        App.SetSystemVariable("CTAB", layoutName);

        // 1b. Activate the layout (and force the paper metrics to refresh by bouncing through Model).
        App.SetSystemVariable("CTAB", "Model");
        App.SetSystemVariable("CTAB", layoutName);
        await CommandLineRunner.RunAsync(doc, "REGEN", "");
        double paperW = 17.0, paperH = 11.0;
        try
        {
          var lmMax = App.GetSystemVariable("LIMMAX");
          var lmMin = App.GetSystemVariable("LIMMIN");
          if (lmMax is double[] a && lmMin is double[] b) { paperW = a[0] + b[0]; paperH = a[1] + b[1]; }
        }
        catch { /* keep the ANSI B default */ }
        // Model units per paper unit. Paper is mm when the sheet numbers are large.
        double u = paperW > 50.0 ? 25.4 : 1.0;                 // mm-per-paper-unit (25.4) or 1 (inches)
        double mpi = 12.0;                                     // model units per inch (feet model, imperial paper -> 12)
        double mpu = mpi / u;                                  // model units per paper unit

        // 1c. Commit the correct paper config into the CURRENT layout. A fresh MCP layout defaults
        //     to Letter/NorthAmericaLetter on DWG To PDF.pc3, so -PLOT plots on Letter paper (not the
        //     ARCH D passed to it), and the paper-space furniture sized for ARCH D crams into the
        //     Letter region while the rest of the page is blank. Set the layout's own PlotSettings
        //     (device + canonical media + paper units + 1:1 + rotation) and read back the REAL paper
        //     size in model units so the sheet frame, margins and -PLOT window all share it.
        var (rw, rh) = ForcePaperConfig(doc, paperSize, orientation, mpi);
        if (rw > 1.0 && rh > 1.0)
        {
          paperW = rw; paperH = rh;
          u = 1.0; mpu = mpi / u;   // paper space is in model units (feet); paper units are inches
        }

        // 2. Sheet margins and the viewport rectangle. The viewport occupies the drawing area
        //    ABOVE the title strip (bottom reserved for the title block), so geometry and its
        //    Civil 3D labels never fall over the title block.
        double mg = 0.5 * u;           // outer margin to the sheet edge
        double tbReserve = 3.2 * u;    // bottom strip kept clear for the title block
        double vx1 = mg + 0.08 * u, vx2 = paperW - mg - 0.08 * u;
        double vy1 = mg + tbReserve, vy2 = paperH - mg - 0.08 * u;
        string F(double v) => v.ToString("0.0000", CultureInfo.InvariantCulture);
        string Corner(double x, double y) => F(x) + "," + F(y);

        // Grid/text helpers (all draw in paper space at 1:1). One string per cell, vertically centred,
        // so nothing can collide the way the previous equal-spaced text lines did.
        // Sheet text uses a TrueType style ("Arial"), created once, so it plots as real font glyphs
        // (crisp and readable) instead of the default SHX font, which DWG To PDF emits as line-art.
        // Driven through the database (not the STYLE dialog command, which cannot run headless).
        bool textStyleDone = false;
        void EnsureTextStyle()
        {
          if (textStyleDone) return;
          using var tr = doc.Database.TransactionManager.StartTransaction();
          var tst = (TextStyleTable)tr.GetObject(doc.Database.TextStyleTableId, OpenMode.ForWrite);
          ObjectId styleId;
          if (tst.Has("Arial")) styleId = tst["Arial"];
          else
          {
            var rec = new TextStyleTableRecord { FileName = "arial.ttf" };
            styleId = tst.Add(rec);
            tr.AddNewlyCreatedDBObject(rec, true);
          }
          doc.Database.Textstyle = styleId;   // new TEXT entities use the current style
          tr.Commit();
          textStyleDone = true;
        }
        async Task GridH(double x0, double x1, double y)
          => await CommandLineRunner.RunAsync(doc, "LINE", "_.LINE", Corner(x0, y), Corner(x1, y), "");
        async Task GridV(double x, double y0, double y1)
          => await CommandLineRunner.RunAsync(doc, "LINE", "_.LINE", Corner(x, y0), Corner(x, y1), "");
        async Task CellText(double x0, double x1, double y0, double y1, string text, double h)
        {
          if (string.IsNullOrEmpty(text)) return;
          EnsureTextStyle();
          double cxm = (x0 + x1) / 2.0, cym = (y0 + y1) / 2.0 - h * 0.35;
          await CommandLineRunner.RunAsync(doc, "TEXT",
            "_.TEXT", "J", "MC", Corner(cxm, cym), F(h), "0", text);
        }
        async Task Rect(double x0, double y0, double x1, double y1)
          => await CommandLineRunner.RunAsync(doc, "PLINE", "_.PLINE",
            Corner(x0, y0), Corner(x1, y0), Corner(x1, y1), Corner(x0, y1), "_C");
        async Task Circle(double x, double y, double r)
          => await CommandLineRunner.RunAsync(doc, "CIRCLE", "_.CIRCLE",
            Corner(x, y), F(r));
        async Task NorthArrow(double cx, double cy, double s)
        {
          // Filled arrowhead pointing up + stem + ring + bold "N", big enough to read at 24x36.
          await Circle(cx, cy, s);
          await CommandLineRunner.RunAsync(doc, "PLINE", "_.PLINE",
            Corner(cx, cy + s * 0.92), Corner(cx - s * 0.30, cy - s * 0.40),
            Corner(cx, cy - s * 0.10), "_C");                        // left blade (closed)
          await CommandLineRunner.RunAsync(doc, "PLINE", "_.PLINE",
            Corner(cx, cy + s * 0.92), Corner(cx + s * 0.30, cy - s * 0.40),
            Corner(cx, cy - s * 0.10), "_C");                        // right blade (closed)
          EnsureTextStyle();
          await CommandLineRunner.RunAsync(doc, "TEXT",
            "_.TEXT", "J", "MC", Corner(cx, cy + s * 0.18), F(s * 0.95), "0", "N");
        }

        // 3. MVIEW: AutoCAD owns the viewport object. Then zoom its model view via MSPACE.
        await CommandLineRunner.RunAsync(doc, "MVIEW",
          "_.MVIEW", Corner(vx1, vy1), Corner(vx2, vy2));

        // Model view: centre at (cx,cy); the requested model height (feet) or the viewport's own height.
        double viewH = modelHeight > 0.0 ? modelHeight.Value : (vy2 - vy1) * mpu;
        await CommandLineRunner.RunAsync(doc, "MSPACE", "_.MSPACE");
        await CommandLineRunner.RunAsync(doc, "ZOOM", "_.ZOOM", "_C", Corner(cx,cy), F(viewH));
        await CommandLineRunner.RunAsync(doc, "PSPACE", "_.PSPACE");

        // ---- sheet furniture, sized to the paper so it is legible on 24x36 ----
        // Inner drawing border, and a reserved title strip at the bottom. The viewport is built
        // inside the drawing area only, so model geometry/labels never land on top of the title block.
        double tbH = 2.25 * u;         // title block height (in paper units)
        // (viewport corner variables vx1..vy2 are defined above inside the drawing area)
        string titleText = PluginRuntime.GetOptionalString(parameters, "title")    ?? layoutName;
        string subtitle  = PluginRuntime.GetOptionalString(parameters, "subtitle") ?? "";
        string idLine    = PluginRuntime.GetOptionalString(parameters, "id")       ?? "";
        string dwgNo     = PluginRuntime.GetOptionalString(parameters, "drawingNumber") ?? layoutName;
        string scaletxt  = "1\" = " + scaleDen.ToString("0.##") + "'";
        string dateStr   = PluginRuntime.GetOptionalString(parameters, "date") ?? DateTime.Now.ToString("MM/dd/yyyy");

        // Double outer border.
        await Rect(mg, mg, paperW - mg, paperH - mg);
        await Rect(mg + 0.08 * u, mg + 0.08 * u, paperW - mg - 0.08 * u, paperH - mg - 0.08 * u);

        // Title block bottom-left as a real grid (one value per cell). Width ~ 12", rows from bottom.
        // Sized so the text is comfortably readable at full size on 24x36.
        double tbL = mg + 0.08 * u, tbB = mg + 0.08 * u;
        double tbR = tbL + 12.0 * u, tbT = tbB + 2.6 * u;
        double r1 = tbB + 0.65 * u, r2 = tbB + 1.30 * u, r3 = tbB + 1.95 * u;
        double c1 = tbL + 2.4 * u;                           // label col | value col
        // outer box + internal grid
        await Rect(tbL, tbB, tbR, tbT);
        await GridH(tbL, tbR, r1); await GridH(tbL, tbR, r2); await GridH(tbL, tbR, r3);
        await GridV(c1, tbB, r3);                            // label/value split, only above the top row
        // Left label cells (rows bottom->top): DATE, SCALE, DWG NO
        await CellText(tbL + 0.05 * u, c1, tbB, r1, "DATE", 0.22 * u);
        await CellText(tbL + 0.05 * u, c1, r1, r2, "SCALE", 0.22 * u);
        await CellText(tbL + 0.05 * u, c1, r2, r3, "DWG. NO.", 0.22 * u);
        // Right value cells
        await CellText(c1 + 0.05 * u, tbR, tbB, r1, dateStr, 0.22 * u);
        await CellText(c1 + 0.05 * u, tbR, r1, r2, scaletxt, 0.22 * u);
        await CellText(c1 + 0.05 * u, tbR, r2, r3, dwgNo, 0.22 * u);
        // Top row spans full width: TITLE + subtitle
        await CellText(tbL + 0.05 * u, tbR, r3, tbT, titleText, 0.32 * u);
        // subtitle sits just under the title inside the same top row (smaller, so no clash)
        if (!string.IsNullOrEmpty(subtitle))
        {
          EnsureTextStyle();
          await CommandLineRunner.RunAsync(doc, "TEXT",
            "_.TEXT", "J", "MC", Corner((tbL + tbR) / 2.0, tbT - 0.50 * u), F(0.18 * u), "0", subtitle);
        }

        // North arrow, top-right, inside a bordered box (so it reads as a proper north arrow,
        // and never gets confused with the sheet border).
        double naBox = 1.6 * u;
        double naCx = paperW - mg - 0.15 * u - naBox / 2.0;
        double naCy = paperH - mg - 0.15 * u - naBox / 2.0;
        await Rect(naCx - naBox / 2.0, naCy - naBox / 2.0, naCx + naBox / 2.0, naCy + naBox / 2.0);
        await NorthArrow(naCx, naCy, 0.62 * u);

        // ID line sits in its own bordered cell, bottom-right, clear of the border.
        double idL = tbR + 0.2 * u;
        if (idL < paperW - mg - 4.5 * u)
        {
          double idR = paperW - mg - 0.08 * u;
          await Rect(idL, tbB, idR, r2);
          await GridH(idL, idR, r1);
          await CellText(idL, idR, r1, r2, "PREPARED BY", 0.20 * u);
          await CellText(idL + 0.06 * u, idR - 0.06 * u, tbB, r1, idLine, 0.24 * u);
        }

        // 5. Plot via the detailed -PLOT chain. TrueType text at full quality, so the title block
        // plots as font text rather than coarse strokes.
        sysvars.Set("TEXTQLTY", 100);
        var orientToken = orientation == "landscape" ? "_L" : "_P";
        // Window plot area: plot exactly the full-sheet rectangle 0,0..paperW,paperH in paper space.
        // (Plot area "Layout" plots the paper-space background, whose origin/axes are not at the
        //  sheet corner on a new MCP layout, which threw all the title-block text into the top-left.)
        var tokens = new List<object>
        {
          "_.-PLOT", "_Y", layoutName, PlotCommands.DefaultPdfDevice, paperSize,
          "_I", orientToken, "_N",
          "_Window", Corner(0.0, 0.0), Corner(paperW, paperH),
          "1:1",                     // viewport carries the scale, paper at 1:1
          "0.00,0.00", "_Y", ".", "_Y", "_N", "_N", "_N",
          resolved, "_N", "_Y"
        };
        await CommandLineRunner.RunAsync(doc, "PLOT", tokens.ToArray());
      }
      finally
      {
        if (doc.Database.Textstyle != originalTextStyle)
        {
          try { doc.Database.Textstyle = originalTextStyle; }
          catch (Exception ex) { PluginLog.Warn("SheetCommands", $"Could not restore the current text style: {ex.Message}"); }
        }
      }

      var fi = new FileInfo(resolved);
      if (!fi.Exists || fi.Length == 0)
        throw new JsonRpcDispatchException("CIVIL3D.API_ERROR", $"Sheet plot did not write '{resolved}'.");
      return new Dictionary<string, object?>
      {
        ["layoutName"] = layoutName,
        ["outputPath"] = resolved,
        ["bytes"] = fi.Length,
        ["modelWindow"] = new Dictionary<string, object?> { ["center"] = new[] { cx, cy }, ["width"] = w, ["height"] = modelHeight },
        ["plotMethod"] = "-MVIEW + -PLOT via command runner",
        ["created"] = true,
      };
    });
  }

  // Resolve the requested (display) paper size to this device's CANONICAL media name, and set the
  // CURRENT LAYOUT's plot config (device + media + orientation + paper units + 1:1) through the
  // PlotSettingsValidator, directly on the live PlotSettings so -PLOT plots on real ARCH D paper.
  // A fresh MCP layout otherwise defaults to Letter: all paper-space furniture, sized to ARCH D,
  // would cram into the small Letter region while the PDF page is ARCH D.
  // Returns the committed paper size in PAPER units (model units per inch is applied by the caller).
  private static (double paperW, double paperH) ForcePaperConfig(Document doc, string paperSize, string orientation, double mpi)
  {
    double pw = 0, ph = 0;
    var db = doc.Database;
    using var tr = db.TransactionManager.StartTransaction();
    var validator = PlotSettingsValidator.Current;

    using var probe = new PlotSettings(false);
    validator.SetPlotConfigurationName(probe, PlotCommands.DefaultPdfDevice, null);
    validator.RefreshLists(probe);
    string? canonical = null;
    foreach (string? c in validator.GetCanonicalMediaNameList(probe))
    {
      if (c == null) continue;
      string? loc = null; try { loc = validator.GetLocaleMediaName(probe, c); } catch { }
      if (string.Equals(c, paperSize, StringComparison.OrdinalIgnoreCase) ||
          string.Equals(loc, paperSize, StringComparison.OrdinalIgnoreCase))
      { canonical = c; break; }
    }

    // The current (live) layout's own PlotSettings, which -PLOT actually uses.
    var cid = LayoutManager.Current.CurrentLayout;
    var layDict = (DBDictionary)tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);
    Layout? cfg = null;
    foreach (DBDictionaryEntry e in layDict)
    {
      if (tr.GetObject(e.Value, OpenMode.ForRead) is Layout lo &&
          string.Equals(lo.LayoutName, cid, StringComparison.OrdinalIgnoreCase))
      { cfg = (Layout)tr.GetObject(e.Value, OpenMode.ForWrite); break; }
    }
    if (cfg == null) { tr.Commit(); return (0, 0); }

    if (canonical != null)
    {
      validator.SetPlotConfigurationName(cfg, PlotCommands.DefaultPdfDevice, canonical);
      validator.SetPlotPaperUnits(cfg, PlotPaperUnit.Inches);
      validator.SetUseStandardScale(cfg, true);
      validator.SetStdScaleType(cfg, StdScaleType.StdScale1To1);
      validator.SetPlotRotation(cfg,
        orientation == "landscape" ? PlotRotation.Degrees090 : PlotRotation.Degrees000);
      validator.RefreshLists(cfg);
      // Read the real committed paper size. PlotPaperSize is in plot-paper units (inches here).
      // Return it as model-units-so-the-furniture-scales-correctly = inches * modelUnitsPerInch.
      pw = cfg.PlotPaperSize.X * mpi; ph = cfg.PlotPaperSize.Y * mpi;
    }
    tr.Commit();
    return (pw, ph);
  }
}
