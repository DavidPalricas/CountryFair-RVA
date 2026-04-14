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

## Architecture

### Core Structure
```
CountryFair/
├── Assets/
│   ├── Scripts/
│   │   ├── General/          # Shared systems (GameManager, AudioManager, DataFileManager)
│   │   ├── CountryFair/      # Hub world logic (tents, dialogue, NPCs)
│   │   ├── MiniGames/        # Mini-game specific code
│   │   │   ├── ArcheryGame/
│   │   │   └── FrisbeeGame/
│   │   └── Utils/            # Utilities (FSM framework)
│   └── Plugins/              # Third-party (FMOD, DOTween, Meta XR)
```

### State Machine Pattern (FSM)
The project uses a custom FSM framework for AI and game object states:
- `FSM.cs` - Manages state transitions via serialized `Transition` assets
- `State.cs` - Abstract base class with `Enter()`/`Execute()`/`Exit()` lifecycle
- Used in: Dog AI (Frisbee game), Animals (hub world), Frisbee physics states

### Data Management
- Save file: `survivorData.json` (persistent per-patient data)
- `DataFileManager.cs` - Singleton handling JSON serialization via Newtonsoft.Json
- Tracks session data for both mini-games with adaptive difficulty parameters

### Mini-Game Architecture
Each mini-game follows a manager pattern:
- `MiniGameManager` (base) → `ArcheryGameManager`, `FrisbeeGameManager`
- `MiniGameAudioManager` → Game-specific audio managers
- `MiniGameCheatCodes` → Debug/testing helpers
- DDA System (`CarnyWise.cs`) - Dynamic difficulty adjustment based on performance

### Key Systems
| System | Files |
|--------|-------|
| Global Game State | `GameManager.cs` (singleton, session flags) |
| Audio | `AudioManager.cs`, FMOD integration |
| Dialogue | `UIDialog.cs`, JSON-driven dialogue trees |
| Save Data | `DataFileManager.cs`, `DataFileRoot.cs` |
| Hand Tracking | BowHandTracking.cs (Archery), Meta XR Hands |

## User Testing

Python scripts in `UserTests/Results/` analyze participant data:
- `get_sus.py` - Calculates System Usability Scale (SUS) scores from CSV results
- Requires: `matplotlib`, standard library only

## External Tools
- **Blender 4.3+** - 3D models/animations
- **FMOD Studio 2.02.32** - Audio event design

## Notes
- Project uses Unity's New Input System (`com.unity.inputsystem`)
- URP (Universal Render Pipeline) for graphics
- Meta Quest 3 deployment target (Android build)
