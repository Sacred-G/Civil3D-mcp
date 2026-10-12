// Live smoke test of label_add (civil3d_standards / civil3d_label) through the real
// MCP server (build/index.js) and the plugin in a running Civil 3D. Run it against a
// SCRATCH drawing: it adds an alignment (line, arc, line, spiral-curve-spiral, line),
// two profile views, and a pipe network with a pipe and two structures shown in both
// views plus a pipe shown in none, all named LBLSMOKE-<pid>-..., and labels them. It
// does not remove them.
//
//   npm run build
//   node scripts/live-label-smoke.mjs --expect-drawing <name>
//
// --expect-drawing (required): runs only when the active drawing's name contains this
//   text, so it never writes into a real job by accident.
import { parseArgs } from "node:util";
import { Client } from "@modelcontextprotocol/sdk/client/index.js";
import { StdioClientTransport } from "@modelcontextprotocol/sdk/client/stdio.js";

const { values: options } = parseArgs({ options: { "expect-drawing": { type: "string" } }, strict: true });
const expectDrawing = options["expect-drawing"];
if (!expectDrawing) {
  console.error("usage: node scripts/live-label-smoke.mjs --expect-drawing <scratch drawing name>");
  process.exit(2);
}

const prefix = `LBLSMOKE-${process.pid}`;
const transport = new StdioClientTransport({
  command: process.execPath,
  args: ["./build/index.js"],
  cwd: process.cwd(),
  env: { ...process.env, MCP_HTTP_HOST: "127.0.0.1", MCP_HTTP_PORT: "0" },
  stderr: "pipe",
});
const client = new Client({ name: "civil3d-label-smoke", version: "1.0.0" });
const results = [];
let failures = 0;

function record(name, passed, detail) {
  results.push({ name, passed, ...(detail === undefined ? {} : { detail }) });
  if (!passed) failures++;
  console.error(`${passed ? "ok  " : "FAIL"} ${name}${detail === undefined ? "" : ` :: ${JSON.stringify(detail).slice(0, 400)}`}`);
}

const text = (result) => (result.content ?? []).map((item) => item.text ?? "").join("\n");
const call = (name, args) => client.callTool({ name, arguments: args }, undefined, { timeout: 180_000, maxTotalTimeout: 180_000 });

// Calls a tool, getting an approval first when the server asks for one.
async function gated(toolName, args) {
  let response = await call(toolName, args);
  if (response.isError && /approval required/i.test(text(response))) {
    const approval = await call("civil3d_request_approval", { toolName, action: args.action, parameters: args });
    if (approval.isError) throw new Error(`approval failed: ${text(approval)}`);
    response = await call(toolName, { ...args, approvalToken: approval.structuredContent.approvalToken });
  }
  return response;
}

// civil3d_run_code has no action parameter; its approval is for action 'run'.
async function runCode(code, args = {}, mode = "read") {
  const parameters = { code, args, mode, timeoutSeconds: 60 };
  const approval = await call("civil3d_request_approval", { toolName: "civil3d_run_code", action: "run", parameters });
  if (approval.isError) throw new Error(`approval failed: ${text(approval)}`);
  const response = await call("civil3d_run_code", { ...parameters, approvalToken: approval.structuredContent.approvalToken });
  if (response.isError) throw new Error(`civil3d_run_code failed: ${text(response)}`);
  const result = response.structuredContent.result;
  if (result.status !== "ok") throw new Error(`run_code ${result.status}: ${JSON.stringify(result.errors ?? result.error).slice(0, 400)}`);
  return result.result;
}

const label = (objectType, objectName, labelType, labelStyle) =>
  gated("civil3d_standards", { action: "label_add", objectType, objectName, labelType, ...(labelStyle ? { labelStyle } : {}) });
const payload = (response) => response.structuredContent?.result ?? response.structuredContent ?? {};

const setupCode = String.raw`
using Autodesk.Civil.DatabaseServices.Styles;
var p = (string)Args["prefix"];
var x0 = (double)Args["x0"];
ObjectId partsList = ObjectId.Null;
foreach (ObjectId id in Civil.Styles.PartsListSet)
{
  var candidate = Open<PartsList>(id);
  if (candidate.GetPartFamilyIdsByDomain(DomainType.Pipe).Count > 0 && candidate.GetPartFamilyIdsByDomain(DomainType.Structure).Count > 0) { partsList = id; break; }
}
if (partsList.IsNull) throw new InvalidOperationException("No parts list with both pipe and structure families.");
var alignmentId = Autodesk.Civil.DatabaseServices.Alignment.Create(Civil, p + "-AL", ObjectId.Null, Db.Clayer,
  Civil.Styles.AlignmentStyles[0], Civil.Styles.LabelSetStyles.AlignmentLabelSetStyles[0]);
var alignment = Open<Autodesk.Civil.DatabaseServices.Alignment>(alignmentId, forWrite: true);
var l1 = alignment.Entities.AddFixedLine(new Point3d(x0, 0, 0), new Point3d(x0 + 100, 0, 0));
var l2 = alignment.Entities.AddFixedLine(new Point3d(x0 + 200, 50, 0), new Point3d(x0 + 300, 150, 0));
var l3 = alignment.Entities.AddFixedLine(new Point3d(x0 + 450, 250, 0), new Point3d(x0 + 550, 250, 0));
alignment.Entities.AddFreeCurve(l1.EntityId, l2.EntityId, new Point3d(x0 + 147, 4, 0));
alignment.Entities.AddFreeSCS(l2.EntityId, l3.EntityId, 15, 15, SpiralParamType.Length, 40, false, SpiralType.Clothoid);
var views = new List<ObjectId>();
foreach (var (name, y) in new[] { (p + "-PV1", -200.0), (p + "-PV2", -400.0) })
{
  var pvId = ProfileView.Create(alignmentId, new Point3d(x0, y, 0), name, Civil.Styles.ProfileViewBandSetStyles[0], Civil.Styles.ProfileViewStyles[0]);
  var pv = Open<ProfileView>(pvId, forWrite: true);
  pv.ElevationRangeMode = ElevationRangeType.UserSpecified;
  pv.ElevationMin = -20;
  pv.ElevationMax = 20;
  views.Add(pvId);
}
var netName = p + "-NET";
var net = Open<Network>(Network.Create(Civil, ref netName), forWrite: true);
net.PartsListId = partsList;
var list = Open<PartsList>(partsList);
var pipeFamily = list.GetPartFamilyIdsByDomain(DomainType.Pipe)[0];
var structFamily = list.GetPartFamilyIdsByDomain(DomainType.Structure)[0];
ObjectId s1 = ObjectId.Null, s2 = ObjectId.Null, pipe = ObjectId.Null, lone = ObjectId.Null;
net.AddStructure(structFamily, Open<PartFamily>(structFamily)[0], new Point3d(x0 + 10, 0, 0), 0, ref s1, false);
net.AddStructure(structFamily, Open<PartFamily>(structFamily)[0], new Point3d(x0 + 90, 0, 0), 0, ref s2, false);
net.AddLinePipe(pipeFamily, Open<PartFamily>(pipeFamily)[0], new LineSegment3d(new Point3d(x0 + 10, 0, -5), new Point3d(x0 + 90, 0, -6)), ref pipe, false);
net.AddLinePipe(pipeFamily, Open<PartFamily>(pipeFamily)[0], new LineSegment3d(new Point3d(x0 + 20, 30, -5), new Point3d(x0 + 80, 30, -6)), ref lone, false);
foreach (var (id, name, shown) in new[] { (s1, "-MH1", true), (s2, "-MH2", true), (pipe, "-PIPE", true), (lone, "-PIPE-NOVIEW", false) })
{
  var part = Open<Part>(id, forWrite: true);
  part.Name = p + name;
  if (shown) foreach (var pvId in views) part.AddToProfileView(pvId);
}
// Each alignment label set with its item count: importing it should leave
// exactly that many label groups on the alignment.
var labelSets = new List<object>();
foreach (ObjectId id in Civil.Styles.LabelSetStyles.AlignmentLabelSetStyles)
{
  var set = Open<Autodesk.Civil.DatabaseServices.Styles.AlignmentLabelSetStyle>(id);
  var items = 0;
  foreach (var item in set) items++;
  labelSets.Add(new { name = set.Name, items });
}
return new { entities = alignment.Entities.Count, labelSets };
`;

// The views each profile label sits in, by its position (Civil 3D shares one
// ProfileViewPart between all views of a part, so the part says nothing).
const placementCode = String.raw`
var p = (string)Args["prefix"];
var views = ModelSpace<ProfileView>().Where(v => v.Name.StartsWith(p)).Select(v => (v.Name, v.GeometricExtents)).ToList();
string ViewOf(Entity e)
{
  Extents3d x;
  try { x = e.GeometricExtents; } catch (Exception) { return null; }
  var c = x.MinPoint + (x.MaxPoint - x.MinPoint) / 2.0;
  foreach (var (name, v) in views)
    if (c.X >= v.MinPoint.X && c.X <= v.MaxPoint.X && c.Y >= v.MinPoint.Y && c.Y <= v.MaxPoint.Y) return name.Substring(p.Length);
  return null;
}
return new
{
  pipe = ModelSpace<PipeProfileLabel>().Select(l => ViewOf(l)).Where(n => n != null).OrderBy(n => n).ToArray(),
  structure = ModelSpace<StructureProfileLabel>().Select(l => ViewOf(l)).Where(n => n != null).OrderBy(n => n).ToArray(),
};
`;

try {
  await client.connect(transport);
  const drawing = await runCode("return Doc.Name;");
  if (!String(drawing).toLowerCase().includes(expectDrawing.toLowerCase())) {
    throw new Error(`Active drawing '${drawing}' does not contain '${expectDrawing}'; stopping before any write.`);
  }

  const setup = await runCode(setupCode, { prefix, x0: (process.pid % 50) * 2000 + 10_000 }, "write");
  record("setup: alignment with 5 entities, 2 profile views, a pipe network", setup.entities === 5, setup);

  // The fullest set first, then an empty one if there is one, then the rest, so
  // the count has to change from import to import: each must replace the last.
  const sets = [...setup.labelSets].sort((a, b) => b.items - a.items);
  const empty = sets.findIndex((s) => s.items === 0);
  if (empty > 1) sets.splice(1, 0, ...sets.splice(empty, 1));
  const groups = [];
  for (const set of sets.slice(0, 4)) {
    const response = await label("alignment", `${prefix}-AL`, "label_set", set.name);
    groups.push({ set: set.name, items: set.items, labelGroups: response.isError ? text(response) : payload(response).labelGroups });
  }
  const changed = groups.some((g, i) => i > 0 && g.labelGroups !== groups[i - 1].labelGroups);
  record("label_set leaves exactly the set's label groups (each import replaces the last)",
    groups.length > 1 && groups.every((g) => g.labelGroups === g.items) && changed, groups);

  const curves = await label("alignment", `${prefix}-AL`, "curves");
  record("curves labels the simple arc and the arc inside the spiral-curve-spiral",
    !curves.isError && (payload(curves).handles ?? []).length === 2, curves.isError ? text(curves) : payload(curves));

  for (const [type, name] of [["pipe", `${prefix}-PIPE`], ["structure", `${prefix}-MH1`]]) {
    const plan = await label(type, name, "plan");
    record(`${type} plan: one label`, !plan.isError && payload(plan).created === 1, plan.isError ? text(plan) : payload(plan));
    const profile = await label(type, name, "profile");
    const r = payload(profile);
    record(`${type} profile: a label in each of the 2 profile views`,
      !profile.isError && r.created === 2 && (r.handles ?? []).length === 2 && r.skipped === 0, profile.isError ? text(profile) : r);
  }

  const noView = await label("pipe", `${prefix}-PIPE-NOVIEW`, "profile");
  record("a pipe in no profile view: a clear error", noView.isError === true && /not drawn in any profile view/i.test(text(noView)), text(noView).slice(0, 200));

  const placed = await runCode(placementCode, { prefix });
  record("each profile label sits in its own view",
    JSON.stringify(placed.pipe) === JSON.stringify(["-PV1", "-PV2"]) && JSON.stringify(placed.structure) === JSON.stringify(["-PV1", "-PV2"]), placed);

  console.log(JSON.stringify({ status: failures === 0 ? "passed" : "failed", failures, drawing, prefix, results }, null, 2));
  process.exitCode = failures === 0 ? 0 : 1;
} catch (error) {
  console.error(`Live label smoke stopped: ${error instanceof Error ? error.message : String(error)}`);
  console.log(JSON.stringify({ status: "stopped", failures, results }, null, 2));
  process.exitCode = 1;
} finally {
  // Closing the stdio transport can leave a promise that never settles once the
  // server exits (Node would end with code 13); bound it and exit explicitly.
  await Promise.race([client.close().catch(() => undefined), new Promise((resolve) => setTimeout(resolve, 5000))]);
}
process.exit(process.exitCode ?? 0);
