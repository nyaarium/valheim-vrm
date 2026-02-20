---
name: agent-team-bridge
description: Cross-team communication for agent teams. Use when another team's work is needed via the bridge, OR when you are receiving a request from another team via the bridge.
---

# Agent Team Bridge

You have access to cross-team communication tools via the `agent-team-bridge` MCP server.
Other agent teams running in separate DevContainers are on the same network
and can be reached through these tools.

---

## Part 1 — Sending a Request

### Tools

- **bridge_discover** — List online teams and their queue depth. Always check before sending.
- **bridge_send** — Send a request to another team and wait for their response. Blocks until they respond.
- **bridge_wait** — Wait N seconds before retrying a deferred request.

### How Threading Works

Each first response from the other team includes a `session_id`. This is the agent session
ID on their side. To continue the conversation (answer a clarification, follow up on a
deferred request), pass that same `session_id` back in your next `bridge_send`. Omit it to
start a fresh conversation thread.

```
# First message — no session_id
bridge_send(to="cool-lib", subject="...", body="...")
→ response includes session_id: "bfa069ad-..."

# Follow-up — pass session_id to continue the same thread
bridge_send(to="cool-lib", session_id="bfa069ad-...", body="...")
```

Do not reuse a `session_id` across unrelated conversations. Each distinct task should be
its own thread.

### Response Statuses

**Successful:**

- **completed** — Work done. Check `summary`, `version`, `breaking`, `migration_notes`.
- **clarification** — They need more info. Answer via a follow-up `bridge_send` with the same `session_id`.
- **deferred** — They're busy, or still working on it. Use `bridge_wait`, then retry.

**Problems — propagate these back to your human:**

- **needs_human** — They need a human decision on their end.
- **error** — Something went wrong. The `reason` field has details (e.g. parse failure, agent failed to start).
- **timeout** — No response in time. The other team may be down or overloaded.

### Timeout Note

Cross-team requests can take many tens of minutes — the other agent may need to implement
a feature, run tests, build, commit, PR, and merge. If you see MCP timeouts, the MCP
client timeout may need to be increased in `.mcp.json` or the client's settings.

---

## Part 2 — Receiving a Request

When another team sends you a request via the bridge, their message is injected directly
into your session as a prompt. You must respond in a strict format — **the system parses your reply and will fail if the format is wrong.**

### ⚠️ The Golden Rule

**Your response MUST begin with a YAML frontmatter block. No exceptions.**

If your reply does not start with `---` on the very first line, the bridge cannot parse it.
The other team receives `status: error` with `reason: "Bridge could not parse agent response"`.
They get nothing. Don't let this happen — even for the simplest reply.

This applies to **every possible situation**: you understand the request, you're confused,
you need more info, you can't help, the request is nonsensical — **always use the frontmatter.**

### Response Formats

Pick exactly one status and use the matching template. Write the frontmatter first, then
your explanation below it.

**Work is done:**
```
---
status: completed
summary: One-line description of what you did
version: 1.2.3
breaking: false
migration_notes: Only needed if breaking is true
---
Detailed explanation of what was done, what changed, and any context the other agent needs.
```

**You need more information before you can proceed:**
```
---
status: clarification
question: The specific question you need answered
---
Explain what you've understood so far and exactly what's ambiguous.
```

**You can't do it right now (e.g. mid-task, tests running):**
```
---
status: deferred
reason: Why you can't take this on right now
estimated_minutes: 15
---
Optional extra context.
```

**A human on your end must make a decision before you can proceed:**
```
---
status: needs_human
reason: Why a human must decide
what_to_decide: The specific decision or approval needed
---
Context to help the human understand the situation.
```

### Choosing the Right Status

| Situation | Status |
|---|---|
| Request completed successfully | `completed` |
| Request is ambiguous or missing detail | `clarification` |
| You're in the middle of something else | `deferred` |
| Request requires a human judgment call | `needs_human` |
| Request makes no sense / out of scope | `clarification` — ask what they actually need |
| You don't have enough context | `clarification` — ask for it |

**Never respond with freeform text.** Even "I don't understand this request" must be wrapped
in a `clarification` block. The bridge has no way to deliver unstructured text to the other agent.
