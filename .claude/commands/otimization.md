You are a senior software architect with deep expertise in two domains: Unity VR development (Meta Quest, Meta XR SDK, C#) and modern TypeScript web applications (React, React Three Fiber, Colyseus real-time servers).

This repository holds **two distinct codebases** that must be analyzed and reported on **separately**:

| Codebase | Root | Stack |
|----------|------|-------|
| **Game** | `CountryFair/` | Unity 6000.2.5f1, C#, Meta XR SDK, FMOD, Netcode for GameObjects |
| **Web App** | `CountryFairWebApp/` | React 19 + React Three Fiber + Vite (`ClientSide/`), Colyseus + Express (`ServerSide/`), TypeScript |

Generate **two** reports:

- `CountryFair/TECH_REPORT.md` — the Unity VR game
- `CountryFairWebApp/TECH_REPORT.md` — the web platform (client + server)

Each report must be self-contained and use only the criteria relevant to its own stack. Never mix Unity concerns into the web report or vice-versa. Overwrite the files if they already exist.

## Arguments

The argument is matched case-insensitively, in this order:

| Argument | Codebase analyzed | Report(s) written |
|----------|-------------------|-------------------|
| *(none)* | Both | `CountryFair/TECH_REPORT.md` **and** `CountryFairWebApp/TECH_REPORT.md` |
| `game` | Unity VR game only — everything under `CountryFair/` | `CountryFair/TECH_REPORT.md` |
| `web` (also accepts `webapp`) | Web platform only — everything under `CountryFairWebApp/` (client **and** server) | `CountryFairWebApp/TECH_REPORT.md` |
| One or more file paths | Only those files | The report of each codebase the files belong to, with every section scoped to what those files reveal |

**Rules when a codebase is selected:**

- Read **only** the files of the selected codebase. Do not open files from the other one, not even for context.
- Write **only** the selected report. Never create, touch or overwrite the other codebase's `TECH_REPORT.md`.
- Use **only** the report structure of the selected codebase (the Unity structure for `game`, the React + Colyseus structure for `web`) and only its evaluation criteria.
- With `web`, treat `ClientSide/` and `ServerSide/` as a single deliverable: both are analyzed and both appear in the one report.
- With `game`, drop the cross-platform network-contract discussion entirely — it belongs to the web report's architecture section.
- If the argument is not recognized as `game`, `web`/`webapp`, or an existing file path, stop and ask instead of guessing.

## Analysis Scope

Apply only the scope of the codebase(s) selected by the argument above.

**Game:** read every `.cs` file under `CountryFair/Assets/Scripts/` recursively. Do not skip Editor scripts, test scripts, or utility classes. Also consult `Packages/manifest.json` and `ProjectSettings/` when relevant to performance claims.

**Web App:** read every `.ts`/`.tsx` file under `CountryFairWebApp/ClientSide/src/` and `CountryFairWebApp/ServerSide/src/`, plus `ServerSide/test/` and `ServerSide/loadtest/`. Also read both `package.json`, the `tsconfig*.json` files, `vite.config.ts` and `ecosystem.config.cjs` — build and deploy configuration is in scope.

**Never analyze** `node_modules/`, `dist/`, `build/`, `Library/`, `Temp/`, `Assets/Plugins/`, or any generated/third-party code.

---

## Report Structure — `CountryFair/TECH_REPORT.md` (Unity VR game)

### 1. Executive Summary
Brief overview of the codebase's current technical state and an overall quality score (1–10) with justification.

### 2. Strengths
What is well-implemented. Be specific — reference class names, patterns already in use, and sound architectural decisions.

### 3. Weaknesses & Code Smells
For each issue: **Location** (class/method), **Problem** (what is wrong and why it matters), **Severity** (Critical / Major / Minor).

Look for:
- God classes with too many responsibilities
- Tight coupling between systems
- MonoBehaviours doing heavy logic in `Update()`
- Missing null checks on Inspector-assigned references
- Magic numbers / hardcoded strings (scene names, tags, PlayerPrefs keys)
- Repeated logic that should be abstracted
- Missing object pooling for frequently spawned objects
- Coroutines that should be async/await
- `Find()` / `GetComponent()` calls in `Update()`

### 4. Design Pattern Opportunities
For each suggestion: **Pattern**, **Where to apply**, **Why** (concrete benefit here), **Sketch** (short C# pseudo-code or interface).

Prioritize patterns common in professional Unity VR projects: event-driven architecture, State Machine, Command, Object Pooling, Repository/Service for data, Strategy for adaptive difficulty or AI.

### 5. Performance Hotspots (VR — 72/90 fps budget)
- Expensive `Update()` / `LateUpdate()` loops
- Unoptimized physics queries and raycasts
- Missing caching of `WaitForSeconds`, strings, component references
- Draw call contributors (dynamic batching issues, missing static flags)
- GC allocations per frame

### 6. Refactoring Roadmap
| Priority | Action | Effort | Impact |
|----------|--------|--------|--------|
| 1 | ... | Low/Med/High | Low/Med/High |

### 7. Conclusion
Final recommendations and suggested order of attack.

---

## Report Structure — `CountryFairWebApp/TECH_REPORT.md` (React + Colyseus)

### 1. Executive Summary
Current technical state of the web platform and an overall quality score (1–10) with justification. State clearly which parts are client, server, and shared contract.

### 2. Architecture Overview
- Client/server split and how they communicate (Colyseus rooms, message names, broadcast channels)
- How the **Unity game** connects to the same server, and which pieces form the cross-platform contract
- Screen/room flow (e.g. waiting screen → game screen) and what drives the transition
- State ownership: what lives in Colyseus `Schema` vs. React local state

### 3. Strengths
What is well-implemented. Reference actual component, room, schema and module names.

### 4. Weaknesses & Code Smells
For each issue: **Location** (file + component/function), **Problem**, **Severity** (Critical / Major / Minor).

**Client-side, look for:**
- Components with mixed responsibilities (scene graph + networking + UI state in one file)
- Missing or wrong `useEffect` cleanup (listeners, room subscriptions, timers) — a leak here survives screen changes
- Missing `useMemo`/`useCallback` around values fed to R3F objects, causing scene rebuilds
- State updates inside `useFrame` (re-renders at frame rate)
- Prop drilling that should be context or a store
- `any` types, unchecked non-null assertions, and disabled lint rules
- Hardcoded server URLs, ports, model paths and magic numbers with no named constant
- Duplicated prop/model-loading logic across the `GameProps` components

**Server-side, look for:**
- Missing validation of client-supplied `options`/message payloads (trust boundary)
- Room lifecycle correctness: leaks in `onLeave`/`onDispose`, orphaned rooms, unhandled reconnection
- Business logic living in `onMessage` handlers instead of testable functions
- Mutable state shared between rooms or module-level singletons
- Unhandled promise rejections and thrown errors that kill the room
- Schema fields that are broader than needed (over-syncing) or mutated by the wrong side
- Test coverage gaps in `test/` relative to the room logic that exists

### 5. Design Pattern Opportunities
For each suggestion: **Pattern**, **Where to apply**, **Why**, **Sketch** (short TypeScript pseudo-code or type definition).

Prioritize patterns idiomatic to this stack:
- Custom hooks to extract networking/subscription logic out of components
- Shared typed message/schema contract instead of duplicated string literals on both sides
- Context or a small store for room/connection state, replacing prop drilling
- Command/handler map for `onMessage` dispatch, keeping handlers thin and testable
- Component composition + `React.memo` boundaries to isolate re-renders in the R3F tree
- Config module / env vars replacing hardcoded endpoints

### 6. Performance Hotspots
**Rendering (React Three Fiber):**
- Per-frame allocations inside `useFrame` (new `Vector3`, arrays, closures)
- Re-renders of R3F subtrees caused by unstable props or state
- Missing `useGLTF.preload`, repeated `scene.clone()`, or unshared geometries/materials
- Draw calls and instancing opportunities for repeated props (tents, workers, balloons)
- Texture/model payload size in `public/` and its effect on load time

**Networking:**
- Broadcast/patch frequency and payload size (Colyseus `patchRate`, over-syncing schema)
- Chatty message patterns that could be batched
- Blocking or synchronous work on the server tick

**Build:**
- Bundle size, code splitting, and anything in `vite.config.ts` worth tuning

### 7. Refactoring Roadmap
| Priority | Action | Area (Client/Server/Shared) | Effort | Impact |
|----------|--------|------------------------------|--------|--------|
| 1 | ... | ... | Low/Med/High | Low/Med/High |

### 8. Conclusion
Final recommendations and suggested order of attack, calling out anything that must be changed on **both** sides of the network contract at once.

---

## Output Rules
- Write in clear, professional English
- Be specific — always reference actual class, component, room, schema, file and method names found in the code
- Do not suggest changes that contradict existing working systems
- If a pattern is already correctly implemented, acknowledge it in Strengths instead of proposing it
- Keep the two reports independent: no cross-references except in the web report's architecture section, where the Unity client is part of the network contract
- Do not report a weakness you cannot point to in a real file — no speculative findings
- Write each selected report to its own codebase root, overwriting if it already exists — and leave the non-selected report untouched
- State at the top of your reply which argument was resolved and which report(s) were written
