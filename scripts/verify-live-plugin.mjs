import { withApplicationConnection } from "../build/utils/ConnectionManager.js";
import { getProjectContext } from "../build/orchestration/ProjectContextService.js";
import { createRequestId, runWithRequestId } from "../build/utils/requestContext.js";

const NO_DRAWING_FAIL_FAST_MS = 5000;

// With zero documents open, a drawing-dependent request must fail fast with
// CIVIL3D.NO_DRAWING instead of hanging until the command timeout. Only
// meaningful when no drawing is loaded; skipped otherwise.
async function verifyNoDrawingFailsFast(health) {
  if (health?.drawingLoaded !== false) {
    return { skipped: true, reason: "a drawing is loaded" };
  }

  const startedAt = Date.now();
  // The watchdog must abort the request it is racing, not just stop waiting:
  // otherwise the connection stays open and its 120 s command timer keeps the
  // request (and this process) alive long after the verdict is known. The
  // connection manager cancels on the request-context abort signal.
  const abort = new AbortController();
  let watchdog;
  const timeout = new Promise((_, reject) => {
    watchdog = setTimeout(() => {
      abort.abort();
      reject(new Error(`getDrawingInfo did not fail within ${NO_DRAWING_FAIL_FAST_MS}ms with no document open.`));
    }, NO_DRAWING_FAIL_FAST_MS);
    watchdog.unref();
  });
  const info = runWithRequestId(
    createRequestId(),
    () => withApplicationConnection(async (client) => client.sendCommand("getDrawingInfo", {})),
    abort.signal,
  );
  // If the watchdog wins, the aborted request rejects later; keep that from
  // surfacing as an unhandled rejection.
  info.catch(() => {});

  try {
    await Promise.race([info, timeout]);
  } catch (error) {
    if (error?.code === "CIVIL3D.NO_DRAWING") {
      return { skipped: false, code: error.code, durationMs: Date.now() - startedAt };
    }
    throw error;
  } finally {
    clearTimeout(watchdog);
  }

  throw new Error("getDrawingInfo succeeded although getCivil3DHealth reported no drawing loaded.");
}

async function main() {
  const health = await withApplicationConnection(async (client) =>
    client.sendCommand("getCivil3DHealth", {}),
  );
  const noDrawing = await verifyNoDrawingFailsFast(health);
  const context = health?.drawingLoaded === false ? null : await getProjectContext();

  process.stdout.write(`${JSON.stringify({ health, noDrawing, context }, null, 2)}\n`);
}

main().catch((error) => {
  const message = error instanceof Error ? error.message : String(error);
  process.stderr.write(`Live Civil 3D plugin validation failed: ${message}\n`);
  process.exitCode = 1;
});
