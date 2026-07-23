# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

VR Country Fair - A Master's Thesis project for stroke rehabilitation using personalized VR serious games. Targets Meta Quest 3.

**Unity Version:** 6000.2.5f1 (required)

## Build & Development

### Opening the Project
```bash
# Open Unity Hub and add the CountryFair folder, or:
# In Unity Hub CLI (if available)
unity-hub --open path/to/CountryFair
```

### Key Dependencies (Packages/manifest.json)
- Meta XR SDK All (v78.0.0) - VR hand tracking, interaction
- Unity XR Hands (v1.5.1) - Hand tracking
- Unity XR OpenXR (v1.15.1) - VR runtime
- FMOD Unity integration - Audio (custom plugin in Assets/Plugins/FMOD)
- DOTween - Animation tweening (Assets/Plugins/Demigiant/DOTween)
- Newtonsoft.Json - JSON serialization for save data
- Unity Netcode (+ UnityTransport) - Network synchronization of the emotion displays

---

## Architecture

### Directory Structure

```
CountryFair/
├── Assets/
│   ├── Scripts/                      # ALL game scripts are in this folder
│   │   ├── General/                  # Shared systems used across the game
│   │   │   ├── GameManagment/        # GameManager, AudioManager, CheatCodes, FoveatedRenderingController
│   │   │   ├── DataManagment/        # DataFileManager + DataFileStructure/ (DataFileRoot, MiniGameData, SessionData)
│   │   │   ├── Animals/              # Animal AI system (AnimalUtility + States/: AnimalState, AnimalWalk, AnimalEat, AnimalIdle)
│   │   │   ├── Balloons/             # BalloonsSpawner, PopBalloon
│   │   │   ├── Others/               # Utility behaviors (WanderingPerson, AnimatableState, TextAnim, ButtonPressed)
│   │   │   └── UIDialog/             # JSON-driven dialogue system base class (UIDialog, JSONData)
│   │   ├── CountryFair/              # Hub world (central fair area)
│   │   │   ├── Management/           # CountryFairAudioManager, CountryFairCheatCodes
│   │   │   ├── Dialogue/             # CountryFairDialogue, JSONData/ (IntroData, SessionCompletedData)
│   │   │   ├── Tents/                # Tent personalization system (see section 11)
│   │   │   └── Other/                # Ambient elements (CabinScript, RotateWheel, SheepAnim,
│   │   │                             #   CountryFairBalloonSpawner) + PlayerScale (giant mode, section 12)
│   │   ├── MiniGames/                # Mini-game modules
│   │   │   ├── CommonElements/       # Shared between all mini-games
│   │   │   │   ├── MiniGamesManagment/  # MiniGameManager, MiniGameAudioManager, MiniGameCheatCodes
│   │   │   │   ├── DDASystem/           # CarnyWise (DDA logic)
│   │   │   │   │   └── ChangeDifficultyFeedback/  # CarnyWiseDiffFeedback, DiffcultyFeedBackData
│   │   │   │   ├── Tutorial/            # Tutorial system, TutorialData (JSON)
│   │   │   │   ├── UI/                  # ScoreAndStreakSystem
│   │   │   │   │   └── Emotions/        # Emotion display system (see section 9)
│   │   │   │   │       ├── Display/         # EmotionDisplay (base), ExpressionDisplay, SliderDisplay
│   │   │   │   │       └── ServerConections/ # ConnectionManager, ServerListener, MainThreadDispatcher
│   │   │   │   └── ReturnToFair.cs      # Scene transition back to hub (single file, not a folder)
│   │   │   ├── ArcheryGame/          # Archery mini-game module
│   │   │   │   ├── GameManagment/    # ArcheryGameManager, ArcheryAudioManager, ArcheryCheatCodes
│   │   │   │   ├── ArcheryStuff/     # BowHandTracking, Arrow, TrajectoryLine
│   │   │   │   ├── Persons/          # Crowd, IdlePerson
│   │   │   │   └── Balloons/         # BalloonArcheryGame (target logic)
│   │   │   ├── FrisbeeGame/          # Frisbee mini-game module
│   │   │   │   ├── GameManagment/    # FrisbeeGameManager, FrisbeeAudioManager, FrisbeeCheatCodes
│   │   │   │   ├── Dog/              # DogFootSteps + States/ (DogState base, GoToTarget, DogIdle,
│   │   │   │   │                     #   Jump, CatchFrisbee, GiveFrisbeeToPlayer)
│   │   │   │   ├── Frisbee/          # FrisbeeTrajectory, FollowPlayerHead + States/ (FrisbeeState base,
│   │   │   │   │                     #   Landed, OnPlayerFront, OnMovement)
│   │   │   │   └── ScoreArea/        # Scoring zones (ScoreAreaProperties, ScoreAreaAnimations)
│   │   │   └── DuckGame/             # Duck mini-game (WIP — only DuckSwim ambient AI so far)
│   │   └── Utils/                    # Core utilities
│   │       ├── FSM/                  # Finite State Machine (FSM, State, Transition)
│   │       └── Utils.cs              # Helper functions (CastRayMetaQuest, RandomValueInRange, GetChildren)
│   └── Plugins/                      # Third-party (FMOD, DOTween, Meta XR, Demigiant)
```

**Note:** `AnimatableState.cs` lives in `General/Others/`, not in `Utils/FSM/`, even though it is part of the FSM hierarchy.

---

## Core Systems Deep Dive

### 1. Finite State Machine (FSM) Framework

**Location:** `Utils/FSM/` (plus `General/Others/AnimatableState.cs`)

A custom serializable FSM system for behavior management:

```
State Hierarchy:
└── State (abstract base)
    └── AnimatableState (adds Animator support)
        ├── AnimalState (animal needs system)
        │   ├── AnimalWalk
        │   ├── AnimalEat
        │   └── AnimalIdle
        └── DogState (dog AI for Frisbee)
            ├── GoToTarget
            ├── DogIdle
            ├── Jump
            ├── CatchFrisbee
            └── GiveFrisbeeToPlayer
```

**Key Files:**
| File | Purpose |
|------|---------|
| `FSM.cs` | Manages state list, transition list, `ChangeState(string)` method |
| `State.cs` | Base class with `Enter()`, `Execute()`, `Exit()`, `LateStart()` lifecycle |
| `AnimatableState.cs` | Adds Animator integration, `IsPlayingNewAnimation()` helper (in `General/Others/`) |
| `Transition.cs` | Serializable transition with `from`, `to`, `name` fields |

**Usage Pattern:**
```csharp
// In State subclass
protected override void Awake() {
    base.Awake(); // Calls SetStateProprieties() - sets fSM reference and StateName
}

public override void Enter() { /* setup */ }
public override void Execute() { /* per-frame logic */ }
public override void Exit() { /* cleanup */ }

// Trigger transition
fSM.ChangeState("ToIdle"); // Case-insensitive, whitespace-insensitive
```

---

### 2. Animal AI System (Needs-Based)

**Location:** `General/Animals/`

Animals have three stats that drive behavior selection:
- **Hunger** (0-1): Increases over time, recovered by eating
- **Boredom** (0-1): Increases over time, recovered by walking
- **Fatigue** (0-1): Increases over time, recovered by idling

**Architecture:**
```
AnimalUtility (component)
├── Stats struct (hunger, boredom, fatigue)
└── Dictionary<string, Func<float>> _registeredActions

AnimalState (base for all animal states)
├── Registers action: _animalUtility.RegisterAction("GoEat", () => stats.hunger)
└── DecideNextAction() picks highest stat value
```

**States:**
| State | Recovers | Increases |
|-------|----------|-----------|
| `AnimalEat` | Hunger | Boredom, Fatigue |
| `AnimalWalk` | Boredom | Hunger, Fatigue |
| `AnimalIdle` | Fatigue | Hunger, Boredom |

---

### 3. Dialogue System (JSON-Driven)

**Location:** `General/UIDialog/`, `CountryFair/Dialogue/`

**Base Class:** `UIDialog.cs`
- Loads JSON files from `StreamingAssets/DialogFiles/`
- Uses `UnityWebRequest` on Android, `File.ReadAllText` on PC
- Abstract methods: `GetJSONDataType()`, `SetJSONFileName()`, `OnDataLoaded()`, `NextStep()`

**Dialog Flow:**
```
UIDialog (base)
├── CountryFairDialogue (hub world intro/session complete)
│   ├── IntroData (Zeca Bigodes + Carny Wise dialogue)
│   └── SessionCompletedData (congratulations message)
├── Tutorial (mini-game tutorials)
│   └── TutorialData (rules, guide, end message)
└── CarnyWiseDiffFeedback (difficulty change feedback)
    └── DiffcultyFeedBackData (increase/decrease texts)
```

**State Machine (CountryFairDialogue):**
```csharp
enum DialogueState {
    BEGIN_INTRO,
    ZECA_INTRO_PART1,
    ZECA_INTRO_PART2,
    CARNY_WISE_INTRO,
    INTRO_COMPLETED
}
```

**Post-intro hand-off:** `CountryFairDialogue` no longer holds a direct `postIntroElements` GameObject reference.
When the intro finishes it invokes the `playerFinishedIntro` UnityEvent (wired in the Inspector), which is what
enables the wrist-menu button (`TentPersonalizationMenu.IntroCompleted()`) and the other post-intro elements.

---

### 4. Data Management System

**Location:** `General/DataManagment/`

**JSON Structure:**
```json
{
  "frisbeeGame": {
    "SessionsData": { "2024-01-01_10-00-00": { "SessionGoal": "3", "AverageTaskPrecision": "85%", ... } },
    "AdadaptiveParameters": { ... }
  },
  "archeryGame": { ... }
}
```

**Key Classes:**
| Class | Purpose |
|-------|---------|
| `DataFileRoot` | Root JSON structure with frisbeeGame + archeryGame |
| `MiniGameData` | Holds SessionsData dictionary + adaptive parameters |
| `SessionData` | Per-session metrics (goal, precision, time) |
| `DataFileManager` | Singleton, handles load/save with Newtonsoft.Json |

**Save Location:**
- Editor: Project root (`survivorData.json`)
- Android: `Application.persistentDataPath`

---

### 5. Mini-Game Architecture

**Location:** `MiniGames/CommonElements/MiniGamesManagment/`

**Inheritance:**
```
MiniGameManager (abstract base)
├── FrisbeeGameManager
└── ArcheryGameManager

MiniGameCheatCodes (base)
├── FrisbeeCheatCodes
└── ArcheryCheatCodes

MiniGameAudioManager (base)
├── FrisbeeAudioManager
└── ArcheryAudioManager
```

**MiniGameManager Responsibilities:**
- Target spawning (`AddTarget`, `RemoveTarget`, `SyncTargets`)
- Difficulty curves (AnimationCurve assets)
- DDA integration via `CarnyWise`

**DDA Flow:**
```
CarnyWise (in CommonElements/DDASystem/)
├── Tracks precision, time, consecutive attempts
├── Thresholds: precisionThresholdToIncreaseDiff (0.7), precisionThresholdToDecreaseDiff (0.3)
├── Buffers: struggleCounter, excelCounter (thresholdToChangeDiff: 3 consecutive)
├── Safety net: failingConsecutiveThreshold (4) — 4 misses on the same task drop difficulty immediately
└── Invokes: changeDifficulty(bool) → MiniGameManager.ChangeDifficulty()
```

---

### 6. Score & Streak System

**Location:** `MiniGames/CommonElements/UI/ScoreAndStreakSystem.cs`

**Features:**
- Tracks score (indefinite) and streak (resets on miss)
- DOTween animations: punch scale, color flash, shake, rotation
- Session goal tracking via PlayerPrefs

**Thresholds:**
| Metric | Value |
|--------|-------|
| Streak mid threshold | 5 (orange) |
| Streak high threshold | 10 (bright orange) |
| High streak bonus | Rotation animation at 5+ |

---

### 6b. Balloon Color Indicator (Archery)

**Location:** `MiniGames/ArcheryGame/GameManagment/ArcheryGameManager.cs`

The UI text indicating which balloon color scores points shows only the color name (e.g. "Vermelho") with its text color set to match the balloon color. On each update a DOTween `DOPunchScale` animation plays on the text to draw attention.

**Miss Conditions (`Arrow.cs`):** An arrow triggers `playerMissed` when hitting objects tagged `Ground` **or** `OutOfBounds`.

---

### 7. Hand Tracking (Archery)

**Location:** `MiniGames/ArcheryGame/ArcheryStuff/BowHandTracking.cs`

**Features:**
- Hand pinch detection for grab/release
- Dynamic bow string deformation (LineRenderer)
- Pull force calculation based on hand backward distance
- Trajectory visualization

**Pinch Detection:**
```csharp
bool IsHandClosed() {
    // Hand tracking: GetFingerPinchStrength(Index/Middle/Ring) > 0.25
    // Controller fallback: PrimaryIndexTrigger > 0.2f
}
```

**Force Calculation:**
```csharp
_shootForce = Mathf.Lerp(minForce (5f), maxForce (60f), _currentPull);
```

---

### 8. Dog AI (Frisbee Game)

**Location:** `MiniGames/FrisbeeGame/Dog/`

**States:**
| State | Behavior |
|-------|----------|
| `DogIdle` | Waits, rotates towards player |
| `GoToTarget` | Navigates to adaptive position in front of player |
| `Jump` | Catches frisbee in air |
| `CatchFrisbee` | Picks up landed frisbee |
| `GiveFrisbeeToPlayer` | Returns frisbee to player |

**Adaptive Positioning:**
- Distance from player scales with difficulty level
- Random angle in 180° arc in front of player
- NavMesh validation with recursive retry (see optimization doc for issues)

**Frisbee Activation Flow:** `GiveFrisbeeToPlayer.Exit()` calls `frisbee.SetActive(true)` to re-enable the frisbee. `OnPlayerFront` no longer calls `SetActive(true)` itself — activation is exclusively managed by the dog state on handoff.

---

### 9. Emotion Display System (Networked)

**Location:** `MiniGames/CommonElements/UI/Emotions/`

The patient's emotional state is pushed from an external emotion-recognition server and rendered by one of
**three interchangeable display modes**. Each mode is a `NetworkBehaviour` so the therapist's device sees the
same state as the headset.

**Class Hierarchy:**
```
EmotionDisplay : NetworkBehaviour       (abstract base — Display/EmotionDisplay.cs)
├── _messageSeparator = ';'             Server message format: "EXPRESSION;CATEGORY"
├── _isUpdatingFromNetwork              Re-entrancy guard: blocks echo back to the network
├── ProcessServerString(string)         Validates exactly one ';' then delegates to the subclass
├── ExpressionDisplay                   Shows one child GameObject per expression
│   ├── NetworkVariable<EXPRESSION_TYPE> _netExpressionState (write: Server)
│   ├── UpdateVisuals(EXPRESSION_TYPE) → SyncToNetwork() → RequestDisplayChangeServerRpc()
│   └── Used TWICE in the scene: once as the emoji display, once as the face display
└── SliderDisplay                       Cumulative mood bar (0 = negative, 1 = positive)
    ├── NetworkVariable<EMOJI_CATEGORY> _netSliderState (write: Server)
    ├── UpdateSlider(EMOJI_CATEGORY) nudges value by sliderChangeAmountThreshold (0.05)
    └── Thresholds: positive >= 0.6, negative <= 0.4; recolors fill + swaps handle sprite
```

**Enums:**
| Enum | Values |
|------|--------|
| `ExpressionDisplay.EXPRESSION_TYPE` | SAD, HAPPY, ANGRY, DISGUST, SURPRISE, FEAR, NEUTRAL |
| `SliderDisplay.EMOJI_CATEGORY` | NEUTRAL, POSITIVE, NEGATIVE |
| `ServerListener.DISPLAYMODE` | EMOJI, FACE, SLIDER |

**Server Connection (`ServerConections/`):**
| Class | Purpose |
|-------|---------|
| `ServerListener` | Raw TCP client (default `172.20.10.2:50050`). Background thread writes into a `volatile` latest-value buffer; `InvokeRepeating(FlushLatestMessage, pollIntervalSeconds)` drains it on the main thread via `Interlocked.Exchange`. Older unprocessed messages are intentionally discarded. Connection failure is **non-fatal** (logs a warning). |
| `ConnectionManager` | Configures `UnityTransport` IP/port and auto-starts as Netcode **client**; `StartHost()` is Inspector-wired for the therapist device. |
| `MainThreadDispatcher` | Marshals work from the receive thread onto the Unity main thread. |

**Display mode selection:** `ServerListener.Awake()` calls `SetCurrentDisplayMode()`, which picks one of the three
modes **at random** (≈1/3 each) and deactivates the other two. `UpdateDisplayMode(DISPLAYMODE)` switches it at runtime
and is what the cheat codes invoke through the `changeEmotionDisplay` UnityEvent.

---

### 10. Cheat Code System

**Location:** `General/GameManagment/CheatCodes.cs`, `MiniGames/CommonElements/MiniGamesManagment/MiniGameCheatCodes.cs`, `CountryFair/Management/CountryFairCheatCodes.cs`

**Base System:**
- Captures keyboard text input (`InputSystem.onTextInput`)
- Buffer with rolling window (`_maxCheatLength`)
- Dictionary-based command registration

**Gating:** in mini-games every cheat except `return` and `tutorial` is ignored until the tutorial is completed.
In the hub, every `CountryFairCheatCodes` cheat except `intro` requires the intro to be completed.

**Registered Cheats (all mini-games):**
| Code | Effect |
|------|--------|
| `return` | Return to CountryFair scene |
| `tutorial` | Skip tutorial |
| `reset` | Reset difficulty to 0 |
| `miss` | Trigger miss event |
| `score` | Trigger score event |
| `complete` | Complete session goal |
| `increase` / `decrease` | Manual difficulty change |
| `e` + expression | Switch to EMOJI mode and show it — `ehappy`, `eneutral`, `esad`, `eangry`, `edisgust`, `esurprise`, `efear` |
| `f` + expression | Switch to FACE mode and show it — `fhappy`, `fneutral`, `fsad`, `fangry`, `fdisgust`, `fsurprise`, `ffear` |
| `positive` / `negative` | Switch to SLIDER mode and nudge the mood bar |

**Registered Cheats (CountryFair hub):**
| Code | Effect |
|------|--------|
| `intro` | Complete the intro dialogue |
| `frisbee` / `archery` | Jump straight into the mini-game scene |
| `giant` | Toggle giant mode (`PlayerScale.ToggleScale()`) |

---

### 11. Tent Personalization System

**Location:** `CountryFair/Tents/`

Lets the patient reorder the fair's mini-game tents, either by **distance-grabbing the tents in the world**
or by **dragging panels inside a wrist menu**. Both surfaces are kept in sync.

**Class Hierarchy:**
```
OrderableTentElement : MonoBehaviour        (OrderableElement.cs — [RequireComponent(typeof(Collider))])
├── miniGame : MINI_GAMES { ARCHERY, DUCK, FISHING, FRISBEE }   ← identity used to pair world ↔ menu
├── currentPlaceHolder / _previousPlaceHolder
├── OnElementSelectionChanged : UnityEvent<bool, OrderableTentElement>
├── HandleGrab(bool)                        abstract-in-spirit (base logs an error)
├── SnapToCurrentPlaceHolder()              virtual
├── SnapToPlaceHolderNextFixedUpdate()      coroutine: waits one FixedUpdate so the Grabbable
│                                           finishes writing throw velocity, then snaps
├── MiniGameTent                            the 3D tent in the hub world
└── TentPanel                               the 2D card inside the wrist menu

PlaceHolder : MonoBehaviour                 slot marker; `number` drives the ribbon badge
├── OnTriggerEnter/Exit → element.UpdateTentPlaceHolder(this / null)   ← tag "TentElement"
└── TentPlaceHolder                         world slot; adds miniGameButtonPlaceHolderTransform
                                            + looping DOTween squash-and-stretch bounce
```

**PlaceHolderManager** (one per surface: one for the world tents, one for the menu panels)
| Member | Purpose |
|--------|---------|
| `_elementsMap : Dictionary<OrderableTentElement, PlaceHolder>` | Which slot each element occupies |
| `HandleTentSelection(bool, element)` | Inspector entry point for `OnElementSelectionChanged` |
| `ElementSelected` | Hides every *other* element, shows all placeholders except the occupied one |
| `ElementUnselected` | Swaps map entries if the drop slot was taken, restores visibility, hides placeholders |
| `UpdateElementPosition` | Performs the actual swap and teleports the displaced element |
| `updateOtherManagers : UnityEvent<OrderableTentElement[]>` | Broadcasts the new order to the twin manager |
| `OnOtherManagerUpdate(elements[])` | Receives the twin's order and mirrors it, matching by `miniGame` and `PlaceHolder.number` |

**Sync flow (menu ↔ world):**
```
Player drags a TentPanel in the wrist menu
  → TentPanel.HandleGrab(false) → SnapToPlaceHolderNextFixedUpdate()
  → OnElementSelectionChanged(false, panel) → PlaceHolderManager.HandleTentSelection
  → ElementUnselected() → UpdateElementPosition() (swap)
  → updateOtherManagers.Invoke(order)
  → world PlaceHolderManager.OnOtherManagerUpdate() → MiniGameTent snaps to the matching slot
```

**Other tent scripts:**
| Class | Purpose |
|-------|---------|
| `TentPersonalizationMenu` | Wrist-menu root. Starts disabled; `IntroCompleted()` enables it (wired to `playerFinishedIntro`), `ButtonClicked()` toggles the panel |
| `MiniGameTent` | Raycasts every `LateUpdate` (`Utils.CastRayMetaQuest`) to show/hide the play button; `GoToMiniGame()` loads `ArcheryGame` / `FrisbeeGame` |
| `TentPanel` | Sets the card sprite, slot number and Portuguese tent name from `miniGame` |
| `TentFishAnim` | Decorative fish animation on the fishing tent |

**Tag:** grabbable tent elements must carry the **`TentElement`** tag (renamed from `Tent`) and an
`OrderableTentElement` component — `PlaceHolder` trigger callbacks log an error otherwise.

**Mini-game availability:** `MINI_GAMES` already declares `DUCK` and `FISHING`, but `MiniGameTent.GoToMiniGame()`
only loads scenes for `ARCHERY` and `FRISBEE`; the others log a "not implemented yet" warning.

---

### 12. Player Scale / Giant Mode

**Location:** `CountryFair/Other/PlayerScale.cs`

Toggles the camera rig between normal and giant scale (`giantScaleFactor`, default 4x) with a Mario-style
grow/shrink animation. Comfort is the driving constraint — the implementation notes are worth preserving:

| Technique | Reason |
|-----------|--------|
| Scale interpolated in **logarithmic space** (`Mathf.Log` → tween → `Mathf.Exp`) | A linear 1x→4x sweep rushes the world across the eyes at the start; log space keeps the *relative* growth rate constant |
| Pivot anchored to the floor point under the head (`KeepPivotAnchored`) | Only eye height changes — no sideways swoop |
| `Ease.InOutSine` (no overshoot/bounce) | Avoids direction changes at the end of the motion |
| `OVRVignette` tunnel (170° → 60°, reopens 2x slower) | Narrowed FOV strongly reduces cybersickness |
| Anticipation squash before growing only | Skipped when shrinking, where it reads as an unexpected extra drop |

`ToggleScale()` ignores input while a sequence is still playing. Fires `onScaleToGiant` / `onScaleToNormal`
(`UnityEvent<AudioManager.GameSoundEffects>`) plus `onScaleAnimationStarted` / `onScaleAnimationCompleted`.

**Audio:** `AudioManager.GameSoundEffects` gained `SCALE_TO_GIANT` and `SCALE_TO_NORMAL`; `CountryFairAudioManager`
overrides `PlaySoundEffect` to map them to FMOD events (`scale_to_giant.wav` / `scale_to_normal.wav`).

---

## Component Interactions

### Scene Flow
```
CountryFair (Hub)
├── CountryFairDialogue (intro) ──playerFinishedIntro──> TentPersonalizationMenu + post-intro elements
├── Animal AI (wandering NPCs)
├── PlayerScale (giant mode toggle)
├── Tent personalization (world PlaceHolderManager <──> wrist-menu PlaceHolderManager)
└── MiniGameTent.GoToMiniGame() → Load "ArcheryGame" / "FrisbeeGame"

ArcheryGame / FrisbeeGame
├── Tutorial (if first time)
├── MiniGameManager
├── CarnyWise (DDA)
├── ScoreAndStreakSystem
├── ServerListener → one of ExpressionDisplay (emoji) / ExpressionDisplay (face) / SliderDisplay
└── ReturnToFair → CountryFair
```

### DDA Loop
```
Player Action → CarnyWise.PlayerScored() / PlayerMissed()
              → EvaluatePerformance(precision, time)
              → CheckToChangeDifficulty()
              → Invoke changeDifficulty(bool)
              → MiniGameManager.ChangeDifficulty()
              → ApplyDifficultySettings()
              → SyncTargets() + UpdateTargetsProperties()
```

---

## Key Configuration Files

### JSON Dialog Files (StreamingAssets/DialogFiles/)
| File | Used By | Content |
|------|---------|---------|
| `intro.json` | CountryFairDialogue | Zeca Bigodes + Carny Wise intro |
| `archery_tutorial.json` | Tutorial (Archery) | Rules, practice guide |
| `frisbee_tutorial.json` | Tutorial (Frisbee) | Rules, practice guide |
| `change_difficulty.json` | CarnyWiseDiffFeedback | Encouragement texts |
| `archery_session_completed.json` | CountryFairDialogue | Session complete message |
| `frisbee_session_completed.json` | CountryFairDialogue | Session complete message |

### Save Data (survivorData.json)
- Per-patient session history
- Adaptive difficulty parameters
- Session metrics (precision, time, goal)

---

## VR-Specific Components

| Component | Purpose |
|-----------|---------|
| `FoveatedRenderingController.cs` | Sets OVRManager.foveatedRenderingLevel |
| `FollowPlayerHead.cs` | Dynamically tracks UI in front of player eyes every `LateUpdate`. Located in `FrisbeeGame/Frisbee/`. |
| `BowHandTracking.cs` | Hand pinch detection, controller fallback |
| `PlayerScale.cs` | Giant-mode toggle with log-space scaling + comfort vignette (section 12) |
| `Utils.CastRayMetaQuest()` | Gaze ray from the object tagged `MainCamera`; used by `MiniGameTent` for the play button |

---

## User Testing

Python scripts in `UserTests/Results/`:
- `get_sus.py` - Calculates System Usability Scale (SUS) scores
- Requires: `matplotlib`
- Output: `Graphs/SUS_Scores.png`

---

## External Tools
- **Blender 4.3+** - 3D models/animations
- **FMOD Studio 2.02.32** - Audio event design

---

## Common Development Patterns

### Singleton Pattern (Game Managers)
```csharp
private static GameManager _instance = null;
public static GameManager GetInstance() {
    return _instance ??= new GameManager();
}
```

### Event-Based Communication
```csharp
[SerializeField] private UnityEvent<int> playerScored;
[SerializeField] private UnityEvent<AudioManager.GameSoundEffects, GameObject> soundFeedback;
```

### PlayerPrefs for Persistent Settings
Keys currently in use: `FrisbeeDifficultyLevel`, `ArcheryDifficultyLevel`, `DogDistance`,
`BalloonColorToScore`, `SessionGoal`.
```csharp
PlayerPrefs.GetInt("FrisbeeDifficultyLevel", 0);
PlayerPrefs.SetFloat("DogDistance", 20f);
PlayerPrefs.SetString("BalloonColorToScore", "red");
```

### GameManager Session Defaults
All session flags in `GameManager.cs` default to `false` and live **in memory only** — they are never written to
disk and reset when the process restarts:
```csharp
public bool IntroCompleted { get; set; } = false;
public bool FrisbeeTutorialCompleted { get; set; } = false;
public bool ArcheryTutorialCompleted { get; set; } = false;
public bool FrisbeeSessionCompleted { get; set; } = false;  // set on return from the mini-game,
public bool ArcherySessionCompleted { get; set; } = false;  // triggers the hub's session-complete dialogue
```

### Serializable Enum Naming
Newer enums use `SCREAMING_SNAKE_CASE` for both the type and its members (`MINI_GAMES`, `EXPRESSION_TYPE`,
`EMOJI_CATEGORY`, `DISPLAYMODE`). Older ones use PascalCase types (`GameSoundEffects`, `DialogueState`).
Match whichever convention the surrounding file already uses.
