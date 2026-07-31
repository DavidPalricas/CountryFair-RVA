Act as a senior engineer working on a hybrid project made of a Unity VR game (Meta Quest, Meta XR SDK, C#) and a companion web platform (React + React Three Fiber client, Colyseus multiplayer server, TypeScript).

## Task
Update and complete the in-code documentation of the project's source files, **using the documentation conventions of the language and stack of each file** — never apply C#/Unity conventions to TypeScript files, or vice-versa.

## Arguments

The argument is matched case-insensitively, in this order:

| Argument | Files documented |
|----------|------------------|
| *(none)* | All source files of **both** codebases (see Scope) |
| `game` | Unity VR game only — `CountryFair/Assets/Scripts/**/*.cs` |
| `web` (also accepts `webapp`) | Web platform only — `CountryFairWebApp/ClientSide/src/**/*.{ts,tsx}` **and** `CountryFairWebApp/ServerSide/src/**/*.ts` |
| One or more file paths | Only those files, whichever codebase they belong to |

**Rules when a codebase is selected:**

- Read and edit **only** the files of the selected codebase. Never modify a file outside it, not even to keep a comment in sync.
- Apply **only** the conventions of that codebase: with `game`, use the C#/Unity rules and ignore the TypeScript ones entirely; with `web`, use the ClientSide and ServerSide rules and ignore the C#/Unity ones.
- With `web`, both `ClientSide/` and `ServerSide/` are in scope — they are one deliverable.
- Cross-checking is still allowed to *understand* a contract (e.g. reading Unity's `ConnectionManager`/`ServerListener` while documenting a Colyseus message), but the file you read for context is never edited.
- In the final output, list only the sections for the selected codebase.
- If the argument is not recognized as `game`, `web`/`webapp`, or an existing file path, stop and ask instead of guessing.

## Scope

- Source files of the project, filtered by the argument above:
  - `CountryFair/Assets/Scripts/**/*.cs` (Unity game)
  - `CountryFairWebApp/ClientSide/src/**/*.{ts,tsx}` (web client)
  - `CountryFairWebApp/ServerSide/src/**/*.ts` (Colyseus server)
- Never touch `node_modules/`, `dist/`, `build/`, `Library/`, `Temp/`, `Assets/Plugins/`, or any generated/third-party code
- Do not refactor logic, do not rename variables, do not reorder members
- If existing documentation is correct, leave it unchanged

## Step 1 — Detect the target for each file

Resolve the target **per file**, from its extension and path:

| File | Target | Convention |
|------|--------|------------|
| `.cs` under `CountryFair/Assets/Scripts/` | Unity / C# | XML doc comments (`///`) |
| `.tsx` under `ClientSide/src/GameProps/` or `ClientSide/src/screens/` | React + React Three Fiber component | JSDoc (`/** */`) |
| `.ts`/`.tsx` elsewhere under `ClientSide/src/` | TypeScript module (network, hooks, utils, entry points) | JSDoc (`/** */`) |
| `.ts` under `ServerSide/src/rooms/` | Colyseus Room | JSDoc (`/** */`) |
| `.ts` under `ServerSide/src/schemas/` | Colyseus Schema (synced state) | JSDoc (`/** */`) |
| `.ts` elsewhere under `ServerSide/src/` | Node/TypeScript module (config, entry point) | JSDoc (`/** */`) |
| Any other language (e.g. `.py` analysis scripts) | Use that language's idiomatic convention (e.g. PEP 257 docstrings) | — |

Before writing, **read a couple of already-documented files in the same target** and match their existing comment style, density and language. Consistency with the surrounding code wins over the templates below.

## Step 2 — Apply the rules for that target

### C# / Unity

- Use `/// <summary>`, `/// <param>`, `/// <returns>`, `/// <remarks>` as appropriate
- Document all `[SerializeField]` fields and public properties with `/// <summary>`
- For Unity messages (`Awake`, `Start`, `Update`, `LateUpdate`, `FixedUpdate`, `OnTriggerEnter`, ...), document the **specific purpose in the context of this script**, not the generic Unity behaviour
- For `State`/`FSM` subclasses, document in `Enter`/`Execute`/`Exit` what the state does and which transitions it triggers (`fSM.ChangeState("...")`)
- For `NetworkBehaviour` classes, state which `NetworkVariable`s are written and by whom (server/client)
- Inspector-driven members:
  - Add `/// <remarks>Invoked via Inspector in [PrefabName/Scene]</remarks>` on public methods with no direct call in code
  - Identify these by looking for public signatures with no internal references (typical `UnityEvent`, UI Button and XR Interactable callbacks)

### TypeScript — React / React Three Fiber components (`ClientSide`)

- One JSDoc block above each exported component: what it renders in the fair scene, and any side effect (model loading, animation, subscription)
- Document the props `type`/`interface` — one `/** */` line per property, including units and coordinate space where relevant (e.g. `position` in world units, `rotation` in radians)
- Document module-level constants (model paths, scale factors, tuning values) explaining **why** the value is what it is, not what it is
- For `useFrame`, `useEffect`, `useMemo` blocks that are not self-evident, add a short `//` comment stating what drives the per-frame/effect work and its cleanup
- Note asset dependencies: `useGLTF` / `useGLTF.preload` calls should mention the expected file under `public/models/`
- Do not add JSDoc `@param`/`@returns` type annotations that merely repeat the TypeScript types — types are already in the signature; document meaning and constraints instead

### TypeScript — Colyseus server (`ServerSide`)

- On each `Room` subclass: purpose of the room, its expected clients/platforms, and its state type
- On the lifecycle hooks (`onCreate`, `onJoin`, `onLeave`, `onDispose`, `onAuth`): the concrete rules enforced — accepted `options`, rejection/`client.leave()` conditions, and thrown errors
- Document every message handler (`onMessage("...")`): the message name, the expected payload shape, and what it changes in the state
- Document every broadcast/`client.send`: the channel name and payload, so the client side can be matched to it
- On `Schema` classes: what each `@type` field represents and which side is allowed to mutate it — these fields are the network contract, so document them all
- Note synchronization assumptions (e.g. one client per platform, room lifetime, reconnection behaviour)

## General rules (all languages)

- Do not document the obvious — the comment must add context, not restate the identifier name
- Prefer explaining **why** and **what invariant holds** over **what the line does**
- Keep the language of comments consistent with the file being edited (existing comments in this project are in English; leave any Portuguese user-facing strings untouched)
- Never change behaviour, formatting of code, or imports — only add/adjust comments

## Output

- State at the top which argument was resolved and which codebase(s) were documented
- Return the changed files with complete documentation
- Then list, grouped by target (omitting any group outside the selected scope):
  - **Unity/C#** — public methods that appear to be Inspector-driven and need confirmation
  - **ClientSide** — components whose props or asset paths could not be inferred with confidence
  - **ServerSide** — messages/broadcasts whose payload shape could not be inferred from the code alone (cross-check against the client and the Unity `ConnectionManager`/`ServerListener` if needed)
