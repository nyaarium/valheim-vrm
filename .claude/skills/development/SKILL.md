---
name: development
description: Provides development tools and guidance. Use when developing code, debugging, or running anything in the development environment.
---

# Development

## Technology Stack

- C# project; codebase is OS-agnostic. Build and tooling vary by OS. Debugging is done from Windows only; write to the fixed log path below.

## Development Guidelines

Do not run internal build commands yourself manually. Use the approved build commands below.

## Building

You must determine what environment you are in first, with `uname`.

Do not try to run the internal build commands yourself manually. You will fail and derail the whole project. Use one of the following build commands.

### Windows CMD (MSYS_NT)

Run: `build.bat`

### Windows Git Bash (MINGW64)

Run: `cmd.exe //c build.bat`

Due to the quirky path mangling of Git Bash, you must run with 2 slashes on the `//c`. Don't ever pipe (|) it.

### Linux

Run: `./build.sh`

## Debugging Approach

When debugging issues, follow this systematic approach to avoid drowning the codebase in unnecessary logging.

### 1. Hypothesize First

Before adding any instrumentation, state 3-5 concrete, testable hypotheses. Good ones are specific and testable (e.g. "the auth token is null at checkout" or "the loop uses `count < 10` instead of `count <= 10`"). Avoid vague statements ("something is broken in the payment flow").

### 2. Instrument to Test Hypotheses

Add write calls to `.cursor/debug.log` (see below) to confirm or reject each hypothesis (typically 2-6 logs total). Log entry/exit, key values at decision points, which branch was taken, and important return values. Don't log every line, redundant data, or things you already know are correct.

### 3. Gather Evidence

Run the code and examine the debug log. For each hypothesis, decide:

- **CONFIRMED** - The logs prove this is the issue
- **REJECTED** - The logs prove this is NOT the issue
- **INCONCLUSIVE** - Need different instrumentation to test this

Only fix issues when you have clear runtime evidence pointing to the cause. Don't guess.

### 4. Fix and Verify

Keep instrumentation in place after a fix, run a verification test, and only remove debug logs once the fix is confirmed. That avoids "fixed one thing, broke another."

## Writing to the Debug Log

Write to the fixed path under the Valheim game directory. Use C# with a Windows path (backslashes or `Path.Combine`).

**Path:** `S:\Steam\steamapps\common\Valheim\valheim-vrm\.cursor\debug.log`

**Format:** One JSON object per line (NDJSON). Each line must include **id**, **timestamp** (ms since Unix epoch), **location**, **message**, **data** (object). Optional: **hypothesisId**, **runId**.

### C# Example

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

const string DebugLogPath = @"S:\Steam\steamapps\common\Valheim\valheim-vrm\.cursor\debug.log";

// #region agent log
var logDir = Path.GetDirectoryName(DebugLogPath);
if (!Directory.Exists(logDir))
    Directory.CreateDirectory(logDir);

var entry = new Dictionary<string, object>
{
    ["id"] = $"log_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid().ToString("n")[..8]}",
    ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    ["location"] = "MyClass.cs:89",
    ["message"] = "Checking state before update",
    ["hypothesisId"] = "A",
    ["data"] = new Dictionary<string, object> { ["value"] = someValue }
};

File.AppendAllText(DebugLogPath, JsonSerializer.Serialize(entry) + "\n");
// #endregion
```

## GitHub Workflow Tips

If you need to check a Github workflow runner job's log, you can use the `gh` CLI:

- Ask for one of either:
  - URL. Route must contain: `*/actions/runs/*`
  - Run ID or Job ID. If the ID is 5 char or less, assume it's wrong; There is a shorter human friendly ID on the website <=5 chars.
- View it with:
  - `gh run view $RUN_ID --log`
  - `gh run view --log-failed --job=$JOB_ID`
