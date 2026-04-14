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
- Unity Netcode - Network synchronization (emojis)

---

## Architecture

### Directory Structure

```
CountryFair/
├── Assets/
│   ├── Scripts/                      # ALL game scripts are in this folder
│   │   ├── General/                  # Shared systems used across the game
│   │   │   ├── GameManagment/        # GameManager, AudioManager, CheatCodes, FoveatedRendering
│   │   │   ├── DataManagment/        # DataFileManager, SessionData, MiniGameData (JSON structure)
│   │   │   ├── Animals/              # Animal AI system (AnimalUtility, AnimalState, AnimalWalk, AnimalEat, AnimalIdle)
│   │   │   ├── Balloons/             # BalloonSpawner, PopBalloon
│   │   │   ├── DisplayInPlayerFront/ # UI positioning in front of player (VR)
│   │   │   ├── Others/               # Utility behaviors (WanderingPerson, AnimatableState, TextAnim, ButtonPressed)
│   │   │   └── UIDialog/             # JSON-driven dialogue system base class
│   │   ├── CountryFair/              # Hub world (central fair area)
│   │   │   ├── Management/           # CountryFairAudioManager, CountryFairCheatCodes
│   │   │   ├── Dialogue/             # CountryFairDialogue, JSONData (IntroData, SessionCompletedData)
│   │   │   ├── Tents/                # Tent interaction (ShowTentData)
│   │   │   └── Other/                # Ambient elements (CabinScript, RotateWheel, SheepAnim, CountryFairBalloonSpawner)
│   │   ├── MiniGames/                # Mini-game modules
│   │   │   ├── CommonElements/       # Shared between all mini-games
│   │   │   │   ├── MiniGamesManagment/  # MiniGameManager, MiniGameAudioManager, MiniGameCheatCodes
│   │   │   │   ├── DDASystem/           # CarnyWise (DDA logic), CarnyWiseDiffFeedback (UI feedback)
│   │   │   │   ├── Tutorial/            # Tutorial system, TutorialData (JSON)
│   │   │   │   ├── UI/                  # ScoreAndStreakSystem, Emojis (ActivateEmoji, ConnectionManager, ServerListener)
│   │   │   │   └── ReturnToFair/        # Scene transition back to hub
│   │   │   ├── ArcheryGame/          # Archery mini-game module
│   │   │   │   ├── GameManagment/    # ArcheryGameManager, ArcheryAudioManager, ArcheryCheatCodes
│   │   │   │   ├── ArcheryStuff/     # BowHandTracking, Arrow, TrajectoryLine
│   │   │   │   ├── Persons/          # Crowd behavior
│   │   │   │   └── Balloons/         # BalloonArcheryGame (target logic)
│   │   │   └── FrisbeeGame/          # Frisbee mini-game module
│   │   │       ├── GameManagment/    # FrisbeeGameManager, FrisbeeAudioManager, FrisbeeCheatCodes
│   │   │       ├── Dog/              # Dog AI (DogState base, GoToTarget, DogIdle, Jump, CatchFrisbee, GiveFrisbeeToPlayer)
│   │   │       ├── Frisbee/          # Physics (FrisbeeTrajectory, FrisbeeState base, Landed, OnPlayerFront, OnMovement)
│   │   │       └── ScoreArea/        # Scoring zones (ScoreAreaProperties, ScoreAreaAnimations)
│   │   └── Utils/                    # Core utilities
│   │       ├── FSM/                  # Finite State Machine (FSM, State, AnimatableState, Transition)
│   │       └── Utils.cs              # Helper functions (RandomValueInRange, GetChildren)
│   └── Plugins/                      # Third-party (FMOD, DOTween, Meta XR, Demigiant)
```

---

## Core Systems Deep Dive

### 1. Finite State Machine (FSM) Framework

**Location:** `General/FSM/`

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
| `AnimatableState.cs` | Adds Animator integration, `IsPlayingNewAnimation()` helper |
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
├── Buffers: struggleCounter, excelCounter (threshold: 3 consecutive)
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

---

### 9. Emoji Network System

**Location:** `MiniGames/CommonElements/UI/Emojis/`

**Network Architecture:**
```
ActivateEmoji : NetworkBehaviour
├── NetworkVariable<EmojiType> _netEmojiState
├── ServerRpc: RequestEmojiChangeServerRpc()
└── OnValueChanged callback → UpdateVisuals()
```

**Emoji Types:** SAD, HAPPY, ANGRY, DISGUST, SURPRISE, FEAR, NEUTRAL

**Cheat Integration:**
```csharp
// In MiniGameCheatCodes.cs
RegisterCheat("happy", () => activateEmoji.UpdateVisuals(EmojiType.HAPPY));
```

---

### 10. Cheat Code System

**Location:** `General/GameManagment/CheatCodes.cs`, `MiniGames/CommonElements/MiniGamesManagment/MiniGameCheatCodes.cs`

**Base System:**
- Captures keyboard text input (`InputSystem.onTextInput`)
- Buffer with rolling window (`_maxCheatLength`)
- Dictionary-based command registration

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
| `happy`, `sad`, `angry`, etc. | Change emoji expression |

---

## Component Interactions

### Scene Flow
```
CountryFair (Hub)
├── CountryFairDialogue (intro)
├── Animal AI (wandering NPCs)
└── Tents → Load mini-game scene

ArcheryScene / FrisbeeScene
├── Tutorial (if first time)
├── MiniGameManager
├── CarnyWise (DDA)
├── ScoreAndStreakSystem
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
| `DisplayInPlayerFront.cs` | Positions UI in front of player eyes |
| `BowHandTracking.cs` | Hand pinch detection, controller fallback |
| `FollowPlayerHead.cs` | Billboard objects towards player |

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
```csharp
PlayerPrefs.GetInt("FrisbeeDifficultyLevel", 0);
PlayerPrefs.SetFloat("DogDistance", 20f);
PlayerPrefs.SetString("BalloonColorToScore", "red");
```
