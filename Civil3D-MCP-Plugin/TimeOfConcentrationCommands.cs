using System.Text.Json.Nodes;

namespace Civil3DMcpPlugin;

/// <summary>
/// Backend commands for Time of Concentration (Tc) calculations and
/// SCS unit hydrograph generation. These are pure computational methods
/// that use standard hydrology formulas — they do not require Civil 3D API
/// objects but can consume data from CatchmentCommands output.
/// </summary>
public static class TimeOfConcentrationCommands
{
  // ──────────────────────────────────────────────
  //  Calculate Time of Concentration
  // ──────────────────────────────────────────────
  public static Task<object?> CalculateTimeOfConcentrationAsync(JsonObject? parameters)
  {
    var method = PluginRuntime.GetRequiredString(parameters, "method");

    return method.ToLowerInvariant() switch
    {
      "kirpich" => Task.FromResult<object?>(CalculateKirpich(parameters)),
      "tr55_sheet" => Task.FromResult<object?>(CalculateTr55Sheet(parameters)),
      "tr55_shallow" => Task.FromResult<object?>(CalculateTr55Shallow(parameters)),
      "tr55_channel" => Task.FromResult<object?>(CalculateTr55Channel(parameters)),
      "faa" => Task.FromResult<object?>(CalculateFaa(parameters)),
      "nrcs_lag" => Task.FromResult<object?>(CalculateNrcsLag(parameters)),
      _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
        $"Unknown Tc method '{method}'. Supported: kirpich, tr55_sheet, tr55_shallow, tr55_channel, faa, nrcs_lag"),
    };
  }

  // ──────────────────────────────────────────────
  //  Generate SCS Unit Hydrograph
  // ──────────────────────────────────────────────
  public static Task<object?> GenerateHydrographAsync(JsonObject? parameters)
  {
    var method = PluginRuntime.GetOptionalString(parameters, "method") ?? "scs_triangular";

    return method.ToLowerInvariant() switch
    {
      "scs_triangular" => Task.FromResult<object?>(GenerateScsTriangularHydrograph(parameters)),
      "scs_curvilinear" => Task.FromResult<object?>(GenerateScsCurvilinearHydrograph(parameters)),
      _ => throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT",
        $"Unknown hydrograph method '{method}'. Supported: scs_triangular, scs_curvilinear"),
    };
  }

  // ──────────────────────────────────────────────
  //  List Supported Tc Methods
  // ──────────────────────────────────────────────
  public static Task<object?> ListTcMethodsAsync()
  {
    return Task.FromResult<object?>(new Dictionary<string, object?>
    {
      ["methods"] = new List<Dictionary<string, object?>>
      {
        new()
        {
          ["name"] = "kirpich",
          ["displayName"] = "Kirpich Formula",
          ["description"] = "Tc = 0.0078 * L^0.77 * S^(-0.385). Suitable for small rural watersheds.",
          ["requiredParameters"] = new List<string> { "flowLength_ft", "elevationDifference_ft" },
        },
        new()
        {
          ["name"] = "tr55_sheet",
          ["displayName"] = "TR-55 Sheet Flow",
          ["description"] = "Tt = 0.007 * (nL)^0.8 / (P2^0.5 * S^0.4). For overland sheet flow ≤ 300 ft.",
          ["requiredParameters"] = new List<string> { "manningsN", "flowLength_ft", "rainfall2yr24hr_in", "slope_ftPerFt" },
        },
        new()
        {
          ["name"] = "tr55_shallow",
          ["displayName"] = "TR-55 Shallow Concentrated Flow",
          ["description"] = "Uses average velocity from slope and surface type to compute travel time.",
          ["requiredParameters"] = new List<string> { "flowLength_ft", "slope_ftPerFt", "surfaceType" },
        },
        new()
        {
          ["name"] = "tr55_channel",
          ["displayName"] = "TR-55 Channel Flow",
          ["description"] = "Tt = L / (60 * V). Uses Manning's equation for channel velocity.",
          ["requiredParameters"] = new List<string> { "flowLength_ft", "slope_ftPerFt", "manningsN", "hydraulicRadius_ft" },
        },
        new()
        {
          ["name"] = "faa",
          ["displayName"] = "FAA (Federal Aviation Administration)",
          ["description"] = "Tc = 1.8 * (1.1 - C) * L^0.5 / S^(1/3). For airport drainage.",
          ["requiredParameters"] = new List<string> { "runoffCoefficient", "flowLength_ft", "slope_percent" },
        },
        new()
        {
          ["name"] = "nrcs_lag",
          ["displayName"] = "NRCS Lag Method",
          ["description"] = "Lag = L^0.8 * (S_retention+1)^0.7 / (1900 * Y^0.5). Tc = Lag / 0.6.",
          ["requiredParameters"] = new List<string> { "flowLength_ft", "curveNumber", "slope_percent" },
        },
      },
    });
  }

  // ══════════════════════════════════════════════
  //  Tc Calculation Methods
  // ══════════════════════════════════════════════

  /// <summary>
  /// Kirpich Formula: Tc = 0.0078 * L^0.77 * S^(-0.385)
  /// L = channel length in feet, S = average slope in ft/ft
  /// Result in minutes.
  /// </summary>
  private static Dictionary<string, object?> CalculateKirpich(JsonObject? parameters)
  {
    var L = PluginRuntime.GetRequiredDouble(parameters, "flowLength_ft");
    var dH = PluginRuntime.GetRequiredDouble(parameters, "elevationDifference_ft");

    if (L <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "flowLength_ft must be positive.");
    if (dH <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "elevationDifference_ft must be positive.");

    var S = dH / L; // slope in ft/ft
    var Tc = 0.0078 * Math.Pow(L, 0.77) * Math.Pow(S, -0.385);

    return new Dictionary<string, object?>
    {
      ["method"] = "kirpich",
      ["timeOfConcentration_min"] = Math.Round(Tc, 2),
      ["timeOfConcentration_hr"] = Math.Round(Tc / 60.0, 4),
      ["flowLength_ft"] = L,
      ["elevationDifference_ft"] = dH,
      ["averageSlope_ftPerFt"] = Math.Round(S, 6),
    };
  }

  /// <summary>
  /// TR-55 Sheet Flow: Tt = 0.007 * (n*L)^0.8 / (P2^0.5 * S^0.4)
  /// n = Manning's roughness, L = flow length (ft, max 300),
  /// P2 = 2-year 24-hour rainfall (in), S = slope (ft/ft)
  /// Result in minutes.
  /// </summary>
  private static Dictionary<string, object?> CalculateTr55Sheet(JsonObject? parameters)
  {
    var n = PluginRuntime.GetRequiredDouble(parameters, "manningsN");
    var L = PluginRuntime.GetRequiredDouble(parameters, "flowLength_ft");
    var P2 = PluginRuntime.GetRequiredDouble(parameters, "rainfall2yr24hr_in");
    var S = PluginRuntime.GetRequiredDouble(parameters, "slope_ftPerFt");

    if (L <= 0 || L > 300)
      throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "flowLength_ft must be between 0 and 300 for sheet flow.");
    if (n <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "manningsN must be positive.");
    if (P2 <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "rainfall2yr24hr_in must be positive.");
    if (S <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "slope_ftPerFt must be positive.");

    var Tt = 0.007 * Math.Pow(n * L, 0.8) / (Math.Pow(P2, 0.5) * Math.Pow(S, 0.4));

    return new Dictionary<string, object?>
    {
      ["method"] = "tr55_sheet",
      ["travelTime_min"] = Math.Round(Tt, 2),
      ["travelTime_hr"] = Math.Round(Tt / 60.0, 4),
      ["manningsN"] = n,
      ["flowLength_ft"] = L,
      ["rainfall2yr24hr_in"] = P2,
      ["slope_ftPerFt"] = S,
    };
  }

  /// <summary>
  /// TR-55 Shallow Concentrated Flow.
  /// Unpaved: V = 16.1345 * S^0.5
  /// Paved: V = 20.3282 * S^0.5
  /// Tt = L / (60 * V)
  /// </summary>
  private static Dictionary<string, object?> CalculateTr55Shallow(JsonObject? parameters)
  {
    var L = PluginRuntime.GetRequiredDouble(parameters, "flowLength_ft");
    var S = PluginRuntime.GetRequiredDouble(parameters, "slope_ftPerFt");
    var surfaceType = PluginRuntime.GetOptionalString(parameters, "surfaceType") ?? "unpaved";

    if (L <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "flowLength_ft must be positive.");
    if (S <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "slope_ftPerFt must be positive.");

    double V;
    if (surfaceType.Equals("paved", StringComparison.OrdinalIgnoreCase))
    {
      V = 20.3282 * Math.Sqrt(S);
    }
    else
    {
      V = 16.1345 * Math.Sqrt(S);
    }

    var Tt = L / (60.0 * V);

    return new Dictionary<string, object?>
    {
      ["method"] = "tr55_shallow",
      ["travelTime_min"] = Math.Round(Tt, 2),
      ["travelTime_hr"] = Math.Round(Tt / 60.0, 4),
      ["velocity_ftPerSec"] = Math.Round(V, 2),
      ["flowLength_ft"] = L,
      ["slope_ftPerFt"] = S,
      ["surfaceType"] = surfaceType,
    };
  }

  /// <summary>
  /// TR-55 Channel Flow: V = (1.49/n) * R^(2/3) * S^(1/2)
  /// Tt = L / (60 * V)
  /// </summary>
  private static Dictionary<string, object?> CalculateTr55Channel(JsonObject? parameters)
  {
    var L = PluginRuntime.GetRequiredDouble(parameters, "flowLength_ft");
    var S = PluginRuntime.GetRequiredDouble(parameters, "slope_ftPerFt");
    var n = PluginRuntime.GetRequiredDouble(parameters, "manningsN");
    var R = PluginRuntime.GetRequiredDouble(parameters, "hydraulicRadius_ft");

    if (L <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "flowLength_ft must be positive.");
    if (S <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "slope_ftPerFt must be positive.");
    if (n <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "manningsN must be positive.");
    if (R <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "hydraulicRadius_ft must be positive.");

    var V = (1.49 / n) * Math.Pow(R, 2.0 / 3.0) * Math.Pow(S, 0.5);
    var Tt = L / (60.0 * V);

    return new Dictionary<string, object?>
    {
      ["method"] = "tr55_channel",
      ["travelTime_min"] = Math.Round(Tt, 2),
      ["travelTime_hr"] = Math.Round(Tt / 60.0, 4),
      ["velocity_ftPerSec"] = Math.Round(V, 2),
      ["flowLength_ft"] = L,
      ["slope_ftPerFt"] = S,
      ["manningsN"] = n,
      ["hydraulicRadius_ft"] = R,
    };
  }

  /// <summary>
  /// FAA Formula: Tc = 1.8 * (1.1 - C) * L^0.5 / S^(1/3)
  /// C = runoff coefficient, L = overland flow length (ft), S = slope (%)
  /// Result in minutes.
  /// </summary>
  private static Dictionary<string, object?> CalculateFaa(JsonObject? parameters)
  {
    var C = PluginRuntime.GetRequiredDouble(parameters, "runoffCoefficient");
    var L = PluginRuntime.GetRequiredDouble(parameters, "flowLength_ft");
    var S = PluginRuntime.GetRequiredDouble(parameters, "slope_percent");

    if (C < 0 || C > 1) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "runoffCoefficient must be between 0 and 1.");
    if (L <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "flowLength_ft must be positive.");
    if (S <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "slope_percent must be positive.");

    var Tc = 1.8 * (1.1 - C) * Math.Pow(L, 0.5) / Math.Pow(S, 1.0 / 3.0);

    return new Dictionary<string, object?>
    {
      ["method"] = "faa",
      ["timeOfConcentration_min"] = Math.Round(Tc, 2),
      ["timeOfConcentration_hr"] = Math.Round(Tc / 60.0, 4),
      ["runoffCoefficient"] = C,
      ["flowLength_ft"] = L,
      ["slope_percent"] = S,
    };
  }

  /// <summary>
  /// NRCS Lag Equation: Lag = L^0.8 * (S_retention + 1)^0.7 / (1900 * Y^0.5)
  /// S_retention = (1000 / CN) - 10, Y = average watershed slope (%)
  /// Tc = Lag / 0.6
  /// Result in minutes.
  /// </summary>
  private static Dictionary<string, object?> CalculateNrcsLag(JsonObject? parameters)
  {
    var L = PluginRuntime.GetRequiredDouble(parameters, "flowLength_ft");
    var CN = PluginRuntime.GetRequiredDouble(parameters, "curveNumber");
    var Y = PluginRuntime.GetRequiredDouble(parameters, "slope_percent");

    if (L <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "flowLength_ft must be positive.");
    if (CN <= 0 || CN > 100) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "curveNumber must be between 0 and 100.");
    if (Y <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "slope_percent must be positive.");

    var S_retention = (1000.0 / CN) - 10.0;
    var Lag = Math.Pow(L, 0.8) * Math.Pow(S_retention + 1.0, 0.7) / (1900.0 * Math.Pow(Y, 0.5));
    var Tc = Lag / 0.6;

    return new Dictionary<string, object?>
    {
      ["method"] = "nrcs_lag",
      ["lag_min"] = Math.Round(Lag, 2),
      ["timeOfConcentration_min"] = Math.Round(Tc, 2),
      ["timeOfConcentration_hr"] = Math.Round(Tc / 60.0, 4),
      ["flowLength_ft"] = L,
      ["curveNumber"] = CN,
      ["slope_percent"] = Y,
      ["potentialRetention_in"] = Math.Round(S_retention, 3),
    };
  }

  // ══════════════════════════════════════════════
  //  Hydrograph Generation Methods
  // ══════════════════════════════════════════════

  /// <summary>
  /// SCS Triangular Unit Hydrograph.
  /// Qp = (484 * A * Q) / Tp
  /// Tp = D/2 + 0.6*Tc
  /// Tb = 2.67 * Tp
  /// A = drainage area (mi²), Q = runoff depth (in), D = storm duration (hr)
  /// </summary>
  private static Dictionary<string, object?> GenerateScsTriangularHydrograph(JsonObject? parameters)
  {
    var drainageArea_mi2 = PluginRuntime.GetRequiredDouble(parameters, "drainageArea_mi2");
    var runoffDepth_in = PluginRuntime.GetRequiredDouble(parameters, "runoffDepth_in");
    var Tc_hr = PluginRuntime.GetRequiredDouble(parameters, "timeOfConcentration_hr");
    var stormDuration_hr = PluginRuntime.GetOptionalDouble(parameters, "stormDuration_hr") ?? (0.133 * Tc_hr);

    if (drainageArea_mi2 <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "drainageArea_mi2 must be positive.");
    if (runoffDepth_in <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "runoffDepth_in must be positive.");
    if (Tc_hr <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "timeOfConcentration_hr must be positive.");

    var D = stormDuration_hr;
    var Tp = D / 2.0 + 0.6 * Tc_hr;
    var Tb = 2.67 * Tp;
    var Qp = 484.0 * drainageArea_mi2 * runoffDepth_in / Tp;

    // Generate hydrograph ordinates (time, flow)
    var ordinates = new List<Dictionary<string, double>>();
    var timeStep = Tb / 20.0;
    for (int i = 0; i <= 20; i++)
    {
      var t = i * timeStep;
      double q;
      if (t <= Tp)
      {
        q = Qp * (t / Tp);
      }
      else if (t <= Tb)
      {
        q = Qp * (Tb - t) / (Tb - Tp);
      }
      else
      {
        q = 0;
      }

      ordinates.Add(new Dictionary<string, double>
      {
        ["time_hr"] = Math.Round(t, 4),
        ["flow_cfs"] = Math.Round(q, 2),
      });
    }

    return new Dictionary<string, object?>
    {
      ["method"] = "scs_triangular",
      ["peakFlow_cfs"] = Math.Round(Qp, 2),
      ["timeToPeak_hr"] = Math.Round(Tp, 4),
      ["timeBase_hr"] = Math.Round(Tb, 4),
      ["stormDuration_hr"] = Math.Round(D, 4),
      ["drainageArea_mi2"] = drainageArea_mi2,
      ["runoffDepth_in"] = runoffDepth_in,
      ["timeOfConcentration_hr"] = Tc_hr,
      ["ordinates"] = ordinates,
    };
  }

  /// <summary>
  /// SCS Curvilinear (Dimensionless) Unit Hydrograph.
  /// Uses the standard SCS dimensionless ratios to produce a more
  /// realistic hydrograph shape than the triangular approximation.
  /// </summary>
  private static Dictionary<string, object?> GenerateScsCurvilinearHydrograph(JsonObject? parameters)
  {
    var drainageArea_mi2 = PluginRuntime.GetRequiredDouble(parameters, "drainageArea_mi2");
    var runoffDepth_in = PluginRuntime.GetRequiredDouble(parameters, "runoffDepth_in");
    var Tc_hr = PluginRuntime.GetRequiredDouble(parameters, "timeOfConcentration_hr");
    var stormDuration_hr = PluginRuntime.GetOptionalDouble(parameters, "stormDuration_hr") ?? (0.133 * Tc_hr);

    if (drainageArea_mi2 <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "drainageArea_mi2 must be positive.");
    if (runoffDepth_in <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "runoffDepth_in must be positive.");
    if (Tc_hr <= 0) throw new JsonRpcDispatchException("CIVIL3D.INVALID_INPUT", "timeOfConcentration_hr must be positive.");

    var D = stormDuration_hr;
    var Tp = D / 2.0 + 0.6 * Tc_hr;
    var Qp = 484.0 * drainageArea_mi2 * runoffDepth_in / Tp;

    // Standard SCS dimensionless ratios (t/Tp, q/Qp)
    double[][] dimensionlessRatios = new[]
    {
      new[] { 0.0, 0.000 },
      new[] { 0.1, 0.030 },
      new[] { 0.2, 0.100 },
      new[] { 0.3, 0.190 },
      new[] { 0.4, 0.310 },
      new[] { 0.5, 0.470 },
      new[] { 0.6, 0.660 },
      new[] { 0.7, 0.820 },
      new[] { 0.8, 0.930 },
      new[] { 0.9, 0.990 },
      new[] { 1.0, 1.000 },
      new[] { 1.1, 0.990 },
      new[] { 1.2, 0.930 },
      new[] { 1.3, 0.860 },
      new[] { 1.4, 0.780 },
      new[] { 1.5, 0.680 },
      new[] { 1.6, 0.560 },
      new[] { 1.7, 0.460 },
      new[] { 1.8, 0.390 },
      new[] { 1.9, 0.330 },
      new[] { 2.0, 0.280 },
      new[] { 2.2, 0.207 },
      new[] { 2.4, 0.147 },
      new[] { 2.6, 0.107 },
      new[] { 2.8, 0.077 },
      new[] { 3.0, 0.055 },
      new[] { 3.2, 0.040 },
      new[] { 3.4, 0.029 },
      new[] { 3.6, 0.021 },
      new[] { 3.8, 0.015 },
      new[] { 4.0, 0.011 },
      new[] { 4.5, 0.005 },
      new[] { 5.0, 0.000 },
    };

    var ordinates = new List<Dictionary<string, double>>();
    foreach (var ratio in dimensionlessRatios)
    {
      var t = ratio[0] * Tp;
      var q = ratio[1] * Qp;
      ordinates.Add(new Dictionary<string, double>
      {
        ["time_hr"] = Math.Round(t, 4),
        ["flow_cfs"] = Math.Round(q, 2),
        ["t_over_Tp"] = ratio[0],
        ["q_over_Qp"] = ratio[1],
      });
    }

    return new Dictionary<string, object?>
    {
      ["method"] = "scs_curvilinear",
      ["peakFlow_cfs"] = Math.Round(Qp, 2),
      ["timeToPeak_hr"] = Math.Round(Tp, 4),
      ["stormDuration_hr"] = Math.Round(D, 4),
      ["drainageArea_mi2"] = drainageArea_mi2,
      ["runoffDepth_in"] = runoffDepth_in,
      ["timeOfConcentration_hr"] = Tc_hr,
      ["ordinateCount"] = ordinates.Count,
      ["ordinates"] = ordinates,
    };
  }
}
