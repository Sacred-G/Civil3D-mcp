using System.Text;

namespace Civil3DMcpPlugin;

/// <summary>
/// runCode's Log buffer (RunCodeScript.Log). AutoCAD-free, so
/// tests\RunCodeHarness checks it offline.
/// </summary>
internal static class RunCodeOutput
{
  // Appends text and a newline without the buffer ever passing maxChars: a
  // line that does not fit is cut to the room left (one huge Log call must not
  // copy all of it), never between the two halves of a surrogate pair.
  // Returns true when anything was cut or refused.
  internal static bool AppendLine(StringBuilder output, string? text, int maxChars)
  {
    var room = maxChars - output.Length;
    if (room <= 0)
    {
      return true;
    }

    text ??= string.Empty;
    if (text.Length < room)
    {
      output.Append(text).Append('\n');
      return false;
    }

    var cut = room - 1;
    if (cut > 0 && char.IsHighSurrogate(text[cut - 1]))
    {
      cut--;
    }

    output.Append(text, 0, cut).Append('\n');
    return true;
  }
}
