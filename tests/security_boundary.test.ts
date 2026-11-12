import { readFileSync, readdirSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { describe, expect, it } from "vitest";

const pluginDirectory = fileURLToPath(new URL("../Civil3D-MCP-Plugin/", import.meta.url));

describe("P2 security boundaries", () => {
  it("keeps generated artifact writes behind FileBoundary", () => {
    const directWritePattern = /File\.(?:WriteAll(?:Text|Bytes|Lines)|AppendAll(?:Text|Lines)|Create|OpenWrite|Copy|Replace|Move)\b|FileInfo\([^)]*\)\.(?:Create|OpenWrite)|\.(?:CopyTo|MoveTo)\(|new (?:System\.IO\.)?StreamWriter|new (?:System\.IO\.)?FileStream/;
    const violations: string[] = [];

    for (const fileName of readdirSync(pluginDirectory).filter((name) => name.endsWith(".cs"))) {
      // PluginLog writes only the plugin's own log; RunCodeLog writes only inside the
      // run-code folder next to plugin.log (checked below); RpcAuth writes only its own
      // %LOCALAPPDATA%\Civil3DMcpPlugin\rpc-token file.
      if (fileName === "FileBoundary.cs" || fileName === "PluginLog.cs" || fileName === "RunCodeLog.cs" || fileName === "RpcAuth.cs") continue;
      const source = readFileSync(`${pluginDirectory}/${fileName}`, "utf8");
      source.split(/\r?\n/).forEach((line, index) => {
        if (directWritePattern.test(line)) violations.push(`${fileName}:${index + 1}`);
      });
    }

    expect(violations).toEqual([]);
  });

  it("confines runCode's files to its run-code folder next to plugin.log", () => {
    const source = readFileSync(`${pluginDirectory}/RunCodeLog.cs`, "utf8");
    expect(source).toContain("Path.GetFullPath(path)");
    expect(source).toContain("runCode writes only inside");
    expect(source).toContain("Expected a plain file name");
    expect(source).toContain("WriteFile cannot use");
    expect(source).toContain("Path.GetFileName(fileName).TrimEnd('.', ' ')");

    // Every direct write in RunCodeLog goes to a path that FilePath() just
    // confined to the run folder.
    const lines = source.split(/\r?\n/);
    const writes = lines
      .map((line, index) => ({ line, index }))
      .filter(({ line }) => /File\.(?:WriteAll(?:Text|Bytes|Lines)|AppendAll(?:Text|Lines)|Create|OpenWrite|Copy|Replace|Move)\b|new (?:System\.IO\.)?(?:StreamWriter|FileStream)/.test(line));
    expect(writes.length).toBeGreaterThan(0);
    for (const { line, index } of writes) {
      expect(line, `RunCodeLog.cs:${index + 1}`).toMatch(/File\.WriteAll(?:Text|Bytes|Lines)\(path,/);
      const before = lines.slice(Math.max(0, index - 8), index).join("\n");
      expect(before, `RunCodeLog.cs:${index + 1}`).toMatch(/var path = FilePath\(runFolder, fileName\);/);
    }
  });

  it("canonicalizes paths, enforces roots and extensions, and writes by atomic move", () => {
    const source = readFileSync(`${pluginDirectory}/FileBoundary.cs`, "utf8");
    expect(source).toContain("Path.GetFullPath");
    expect(source).toContain("Path.GetRelativePath");
    expect(source).toContain("CIVIL3D_IMPORT_ROOTS");
    expect(source).toContain("CIVIL3D_EXPORT_ROOTS");
    expect(source).toContain("CIVIL3D.PATH_NOT_ALLOWED");
    expect(source).toContain("CIVIL3D.FILE_TYPE_NOT_ALLOWED");
    expect(source).toContain("FileAttributes.ReparsePoint");
    expect(source).toContain("File.Move(tempPath, path, overwrite)");
    expect(source).toContain("Set overwrite=true");
  });

  it("uses JSON-RPC numeric errors and retains domain identifiers in error.data.code", () => {
    const source = readFileSync(`${pluginDirectory}/JsonRpcProtocol.cs`, "utf8");
    expect(source).toContain('["code"] = numericCode');
    expect(source).toContain('["data"] = new JsonObject');
    expect(source).toContain('["code"] = domainCode');
    expect(source).toContain('"CIVIL3D.METHOD_NOT_FOUND" => -32601');
    expect(source).toContain('"CIVIL3D.INVALID_INPUT" => -32602');
  });
});
