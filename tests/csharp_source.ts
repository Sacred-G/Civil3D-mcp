// Small helpers for tests that check C# plugin source without compiling it.
// They work on "masked" code: comments and the contents of string and char
// literals are replaced with spaces (newlines kept), so every index still
// points at the same place in the original, while searches and brace
// matching see only code. Interpolation holes ({...} in $"...") stay code.

export function maskCSharp(code: string): string {
  return maskCSharpParts(code).code;
}

// Both masks of the same source, index for index: `code` (comments and
// literal contents blanked) for finding things, and `withoutComments`
// (comments blanked, literals kept) for reading a literal at a place found in
// `code`.
export function maskCSharpParts(code: string): { code: string; withoutComments: string } {
  const out = code.split("");
  const noComments = code.split("");
  const blank = (from: number, to: number, comment = false) => {
    for (let k = from; k < to && k < code.length; k++) {
      if (out[k] !== "\n" && out[k] !== "\r") {
        out[k] = " ";
        if (comment) noComments[k] = " ";
      }
    }
  };

  // Code from i; inside an interpolation hole it stops at the hole's '}'.
  function codeFrom(i: number, inHole: boolean): number {
    let depth = 0;
    while (i < code.length) {
      const c = code[i];
      const next = code[i + 1];
      if (c === "/" && next === "/") {
        const end = code.indexOf("\n", i);
        const stop = end < 0 ? code.length : end;
        blank(i, stop, true);
        i = stop;
        continue;
      }
      if (c === "/" && next === "*") {
        const end = code.indexOf("*/", i + 2);
        const stop = end < 0 ? code.length : end + 2;
        blank(i, stop, true);
        i = stop;
        continue;
      }
      if (c === "'") {
        let j = i + 1;
        while (j < code.length && code[j] !== "'" && code[j] !== "\n") j += code[j] === "\\" ? 2 : 1;
        blank(i + 1, j);
        i = j + 1;
        continue;
      }
      const prefix = /^(?:\$@|@\$|\$|@)?"/.exec(code.slice(i, i + 3));
      if (prefix && (c === '"' || c === "$" || c === "@")) {
        i = stringFrom(i, prefix[0]);
        continue;
      }
      if (inHole) {
        if (c === "{") depth++;
        else if (c === "}") {
          if (depth === 0) return i;
          depth--;
        }
      }
      i++;
    }
    return i;
  }

  // A string literal starting at i with the given prefix ($, @, $@, @$ and
  // the opening quote); returns the index after its closing quote.
  function stringFrom(i: number, prefix: string): number {
    const interpolated = prefix.includes("$");
    const verbatim = prefix.includes("@");
    let j = i + prefix.length;
    // Not for verbatim strings: @"""" is a verbatim string holding one quote.
    if (!verbatim && code.startsWith('"""', j - 1)) {
      // Raw string literal ("""..."""): treated as one literal.
      const end = code.indexOf('"""', j + 2);
      const stop = end < 0 ? code.length : end + 3;
      blank(j, stop - 3);
      return stop;
    }
    let start = j;
    while (j < code.length) {
      const c = code[j];
      if (verbatim && c === '"' && code[j + 1] === '"') {
        j += 2;
        continue;
      }
      if (!verbatim && c === "\\") {
        j += 2;
        continue;
      }
      if (c === '"') break;
      if (!verbatim && c === "\n") break;
      if (interpolated && c === "{") {
        if (code[j + 1] === "{") {
          j += 2;
          continue;
        }
        blank(start, j);
        const close = codeFrom(j + 1, true);
        j = close + 1;
        start = j;
        continue;
      }
      j++;
    }
    blank(start, j);
    return j + 1;
  }

  codeFrom(0, false);
  return { code: out.join(""), withoutComments: noComments.join("") };
}

const pairs: Record<string, string> = { "(": ")", "{": "}", "[": "]" };

// Index of the bracket that closes the one at `open`, in masked code.
export function matchingClose(masked: string, open: number): number {
  const opener = masked[open];
  const closer = pairs[opener];
  if (!closer) throw new Error(`No bracket at ${open}: '${opener}'`);
  let depth = 0;
  for (let j = open; j < masked.length; j++) {
    if (masked[j] === opener) depth++;
    else if (masked[j] === closer && --depth === 0) return j;
  }
  throw new Error(`Unclosed '${opener}' at ${open}`);
}

// Top-level comma-separated parts of masked[from, to), with their start
// indexes. Angle brackets count when `angles` is set (parameter lists).
export function splitTopLevel(masked: string, from: number, to: number, angles = false): { text: string; start: number }[] {
  const parts: { text: string; start: number }[] = [];
  let depth = 0;
  let start = from;
  for (let j = from; j < to; j++) {
    const c = masked[j];
    if ("({[".includes(c) || (angles && c === "<")) depth++;
    else if (")}]".includes(c) || (angles && c === ">")) depth--;
    else if (c === "," && depth === 0) {
      parts.push({ text: masked.slice(start, j), start });
      start = j + 1;
    }
  }
  if (masked.slice(start, to).trim() !== "") parts.push({ text: masked.slice(start, to), start });
  return parts;
}

// The argument list of the call whose opening parenthesis is at `open`.
export function callArguments(masked: string, open: number) {
  return splitTopLevel(masked, open + 1, matchingClose(masked, open));
}

// Body of a method found by a regex for its declaration (up to its name), as
// masked code between its braces.
export function methodBody(masked: string, declaration: RegExp): string {
  const match = declaration.exec(masked);
  if (!match) throw new Error(`Declaration not found: ${declaration}`);
  const open = masked.indexOf("{", match.index + match[0].length);
  return masked.slice(open + 1, matchingClose(masked, open));
}

const notMethods = new Set(["if", "for", "foreach", "while", "switch", "catch", "using", "lock", "fixed", "else", "try", "finally", "do", "get", "set", "return", "new"]);

// Innermost blocks around `index`, from the inside out: the open brace's
// index and the text before it (its header, back to the previous ; { or }).
export function enclosingBlocks(masked: string, index: number): { open: number; header: string }[] {
  const blocks: { open: number; header: string }[] = [];
  let depth = 0;
  for (let j = index - 1; j >= 0; j--) {
    const c = masked[j];
    if (c === "}") depth++;
    else if (c === "{") {
      if (depth === 0) {
        let k = j - 1;
        let parens = 0;
        while (k >= 0) {
          const h = masked[k];
          if (h === ")") parens++;
          else if (h === "(") parens--;
          else if (parens === 0 && (h === ";" || h === "{" || h === "}")) break;
          k--;
        }
        blocks.push({ open: j, header: masked.slice(k + 1, j).trim() });
      } else {
        depth--;
      }
    }
  }
  return blocks;
}

// The method around `index`: its name and parameter names.
export function enclosingMethod(masked: string, index: number): { name: string; parameters: string[] } | undefined {
  for (const block of enclosingBlocks(masked, index)) {
    // Drop what may follow the parameter list: generic constraints
    // (where T : class) and a constructor initializer (: base(...) or
    // : this(...)), so the last parenthesised group is the parameters.
    const header = block.header
      .replace(/\)\s*where\s+\w+\s*:[\s\S]*$/, ")")
      .replace(/\)\s*:\s*(?:base|this)\s*\([\s\S]*\)\s*$/, ")");
    const match = /(\w+)\s*(?:<[^()]*>)?\s*\(([\s\S]*)\)\s*$/.exec(header);
    if (!match || notMethods.has(match[1])) continue;
    if (/=>\s*$/.test(header)) continue;
    const parametersText = match[2];
    const parameters = splitTopLevel(parametersText, 0, parametersText.length, true)
      .map((part) => part.text.trim().split(/\s+/).pop() ?? "")
      .filter(Boolean);
    return { name: match[1], parameters };
  }
  return undefined;
}

// Whitespace runs folded to one space, for comparing code regardless of
// formatting.
export const squash = (code: string) => code.replace(/\s+/g, " ").trim();
