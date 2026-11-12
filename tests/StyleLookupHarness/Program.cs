using System.Reflection;
using Civil3DMcpPlugin;

// Offline checks for the style lookups behind civil3d_profile create_layout /
// create_from_surface / view_create and the other LookupUtils style lookups.
// Run with: npm run test:style-lookups

// --- Reproduce the Civil 3D 2027 shape: a set-only `new Name` hides a readable Name.
// Autodesk.Civil.DatabaseServices.DBObject declares get_Name/set_Name;
// Styles.StyleBase declares only set_Name. Plain Type.GetProperty then gives a
// property that cannot be read (or throws AmbiguousMatchException), so every
// style name read as null and a named lookup never matched.
var flags = BindingFlags.Public | BindingFlags.Instance;
PropertyInfo? plain = null;
var plainThrew = false;
try
{
  plain = typeof(FakeProfileStyle).GetProperty("Name", flags);
}
catch (AmbiguousMatchException)
{
  plainThrew = true;
}
Assert(plainThrew || plain?.CanRead == false, "plain GetProperty cannot read the hidden Name (the bug)");

var readable = Civil3DCompatibility.FindProperty(typeof(FakeProfileStyle), "Name", flags, Civil3DCompatibility.PropertyAccess.Read);
Assert(readable?.CanRead == true, "FindProperty(Read) finds the readable base Name");
Assert(readable?.DeclaringType == typeof(FakeDbObject), "readable Name is declared on the base object");

var style = new FakeProfileStyle("Design Profile");
Assert(Equals(Civil3DCompatibility.GetPropertyValue(style, "Name"), "Design Profile"), "GetPropertyValue reads a style name");
Assert(Civil3DCompatibility.GetPropertyValue<string>(style, "Name") == "Design Profile", "typed GetPropertyValue reads a style name");

var writable = Civil3DCompatibility.FindProperty(typeof(FakeProfileStyle), "Name", flags, Civil3DCompatibility.PropertyAccess.Write);
Assert(writable?.DeclaringType == typeof(FakeStyleBase), "FindProperty(Write) keeps the most derived setter");
Assert(Civil3DCompatibility.TrySetProperty(style, "Name", "Renamed"), "TrySetProperty sets a style name");
Assert(style.SetThroughStyleBase, "the set goes through the derived (StyleBase) setter");
Assert(Equals(Civil3DCompatibility.GetPropertyValue(style, "Name"), "Renamed"), "renamed style reads back");

// Ordinary properties are unaffected.
var plainObject = new PlainObject { Name = "Alignment - (1)", Count = 3 };
Assert(Equals(Civil3DCompatibility.GetPropertyValue(plainObject, "Name"), "Alignment - (1)"), "ordinary Name still reads");
Assert(Civil3DCompatibility.GetPropertyValue<int>(plainObject, "Count") == 3, "ordinary int property still reads");
Assert(Civil3DCompatibility.GetPropertyValue(plainObject, "Missing") == null, "missing property reads as null");
Assert(Civil3DCompatibility.TrySetProperty(plainObject, "Count", 5) && plainObject.Count == 5, "ordinary set still works");
Assert(!Civil3DCompatibility.TrySetProperty(plainObject, "ReadOnly", "x"), "read-only property is not settable");

// --- Name matching -----------------------------------------------------------
// The NCS Imperial template's profile view styles, in collection order.
string?[] viewStyles =
{
  "Intermediate View", "Profile View", "Last View", "Top Stacked View", "Major Grids and HGP",
  "Middle Stacked View", "Land Desktop Profile View", "Bottom Stacked View", "Major Grids", "Standard",
  "Full Grid", "First View",
};
Assert(StyleNameMatch.FindIndex(viewStyles, "Profile View") == 1, "'Profile View' is found (live bug: reported missing)");
Assert(StyleNameMatch.FindIndex(viewStyles, "profile view") == 1, "match is case-insensitive");
Assert(StyleNameMatch.FindIndex(viewStyles, "  Profile View ") == 1, "surrounding whitespace is ignored");
Assert(StyleNameMatch.FindIndex(viewStyles, null) == 0, "no name uses the first style");
Assert(StyleNameMatch.FindIndex(viewStyles, "   ") == 0, "blank name uses the first style");
Assert(StyleNameMatch.FindIndex(viewStyles, "No Such View") == -1, "unknown name does not fall back");
Assert(StyleNameMatch.FindIndex(Array.Empty<string?>(), null) == -1, "empty collection has no first style");
Assert(StyleNameMatch.FindIndex(Array.Empty<string?>(), "Profile View") == -1, "empty collection has no match");

string?[] profileStyles = { "Existing Ground Profile", "Design Profile", "Standard" };
Assert(StyleNameMatch.FindIndex(profileStyles, "Design Profile") == 1, "'Design Profile' is found (live bug: got 'Existing Ground Profile')");

string?[] cased = { "standard", "Standard" };
Assert(StyleNameMatch.FindIndex(cased, "Standard") == 1, "an exact-case match wins over a case-insensitive one");
Assert(StyleNameMatch.FindIndex(new string?[] { null, "Standard" }, "Standard") == 1, "unreadable names are skipped");

var missing = StyleNameMatch.DescribeMissing("Profile style", " Desgin Profile ", profileStyles);
Assert(missing == "Profile style 'Desgin Profile' was not found. Available: 'Existing Ground Profile', 'Design Profile', 'Standard'.", "missing-name message lists the available names: " + missing);
var none = StyleNameMatch.DescribeMissing("Profile view band set", "Major Grids", Array.Empty<string?>());
Assert(none == "Profile view band set 'Major Grids' was not found: the drawing has no profile view band sets.", "empty-collection message: " + none);
var many = StyleNameMatch.DescribeMissing("Surface style", "X", Enumerable.Range(1, 35).Select(i => (string?)$"S{i}").ToArray());
Assert(many.EndsWith("'S30' (and 5 more).", StringComparison.Ordinal), "long lists are truncated: " + many);

Console.WriteLine($"StyleLookupHarness: {passed} checks passed.");
return failed == 0 ? 0 : 1;

partial class Program
{
  private static int passed;
  private static int failed;

  private static void Assert(bool condition, string message)
  {
    if (condition)
    {
      passed++;
      return;
    }

    failed++;
    Console.Error.WriteLine($"FAIL: {message}");
  }
}

public class FakeDbObject
{
  private string name = string.Empty;

  public string Name
  {
    get => name;
    set => name = value;
  }
}

public class FakeStyleBase : FakeDbObject
{
  public bool SetThroughStyleBase { get; private set; }

  public new string Name
  {
    set
    {
      SetThroughStyleBase = true;
      base.Name = value;
    }
  }
}

public sealed class FakeProfileStyle : FakeStyleBase
{
  public FakeProfileStyle(string name)
  {
    ((FakeDbObject)this).Name = name;
  }
}

public sealed class PlainObject
{
  public string Name { get; set; } = string.Empty;

  public int Count { get; set; }

  public string ReadOnly => "fixed";
}
