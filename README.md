# 🎡Personalized CountryFair VR

A VR serious game for stroke rehabilitation, featuring adaptive mini-games in an immersive country fair environment built for Meta Quest 3 with hand-tracking support.

This repository contains the source code developed as part of a Master's Thesis titled *"Exploring the Effect of a Personalized Virtual Reality Serious Game for Stroke Rehabilitation"*, focused on stroke survivor rehabilitation using Virtual Reality Serious Games.

---

# 📺 Demo

### [CountryFair VR Demo](https://www.youtube.com/watch?v=bDXMD0OPzRI)

---

# 🔍 Overview

CountryFair VR is a VR serious game designed to enhance post-stroke rehabilitation through immersive, gamified exercises targeting upper-limb motor recovery. The project targets **Meta Quest 3** with hand-tracking support and was developed in **Unity 6000.2.5f1**. It features two rehabilitation mini-games set within an interactive country fair environment with 3D spatial audio, and incorporates a rule-based Dynamic Difficulty Adjustment (DDA) system **CarnyWise** that continuously adapts challenge parameters to the player's performance, maintaining an optimal flow state during rehabilitation sessions.

The system was developed using a Human-Centered Design (HCD) methodology in collaboration with the **Rovisco Pais Rehabilitation Center** and validated through user testing with **38 participants**.

---

# 🎯 The Problem

Traditional post-stroke rehabilitation often suffers from low patient adherence due to the monotony of repetitive exercises and a lack of personalization in the therapeutic process.

---

# 💡 Our Approach

We propose a solution set in a VR Country Fair environment, designed to enhance patient motivation and engagement. The system utilizes:

- **Immersive Virtual Reality** and serious mini-games to combat demotivation and increase engagement.
- A **rule-based Dynamic Difficulty Adjustment (DDA)** system CarnyWise that adapts game parameters in real time based on player performance, promoting an optimal flow state.
- **3D spatial audio** (via FMOD) to reinforce environmental immersion and provide directional feedback.
- **Hand-tracking interaction** on Meta Quest 3, enabling natural gesture-based gameplay without controllers.

## Implemented Mini-games

- 🏹🎈 **Archery:** A balloon-target shooting game where the player uses a virtual bow. Difficulty parameters such as balloon count, speed, and spawn patterns are dynamically adjusted by CarnyWise.
- 🥏🐶 **Frisbee:** A throwing game where the player tosses a frisbee into a dog's catch area. Throw distance, target size, and timing windows adapt to the player's performance.

---

# 📁 Project Structure

<!-- TODO: Regenerate this section by running the analysis against the actual repository -->

```
Assets/
├── Scenes/              # Main Unity scenes (.unity)
├── Scripts/             # C# game logic
│   ├── MiniGames/       # Per-mini-game logic (Archery, Frisbee)
│   ├── DDA/             # CarnyWise dynamic difficulty adjustment
│   ├── Managers/        # Singleton/manager scripts
│   └── Utils/           # Utility and helper classes
├── Models/              # 3D assets (Blender imports, Poly Pizza models)
├── Audio/               # FMOD audio events and banks
├── Materials/           # Shaders and materials
├── Prefabs/             # Reusable prefabs (mini-game elements, UI)
├── Resources/           # Runtime-loaded assets
└── StreamingAssets/     # FMOD bank files
Packages/                # Unity Package Manager dependencies
ProjectSettings/         # Unity project configuration
```

> **Note:** This tree is indicative. Run the full analysis against the repository for exact paths and inline descriptions.

---

# 🏗️ Architecture Overview

<!-- TODO: Validate and expand by inspecting the actual codebase -->

- **Singleton Managers:** Core systems (e.g., game state, audio, scene transitions) are expected to follow a singleton pattern for global access across scenes.
- **Scene Composition:** The project uses a modular scene structure each mini-game operates as a self-contained scene or scene segment within the overarching country fair environment.
- **CarnyWise DDA:** A rule-based difficulty adjustment system that monitors player performance metrics (accuracy, completion time, success rate) and modifies game parameters (target count, speed, distances, timing windows) to maintain flow. This is distinct from the VR project name CarnyWise refers specifically to the DDA/personalization engine.
- **Meta XR Integration:** Hand-tracking is implemented through the Meta XR SDK, using pinch gesture interactions and ray casting for in-game input.
- **FMOD Audio:** 3D spatial audio is managed via FMOD Studio integration, providing positional sound cues tied to the fair environment and gameplay events.

---

# 🚀 Getting Started

## Prerequisites

| Requirement | Version |
|---|---|
| **Unity** | 6000.2.5f1 (Unity 6) |
| **Meta Quest 3** | Required for deployment |
| **Blender** *(optional)* | 4.3 or above for editing 3D models |
| **FMOD Studio** *(optional)* | 2.02.32 for editing audio events |

## Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/DavidPalricas/CountryFair-VR.git
   ```
2. **Open in Unity Hub:** Add the cloned folder and ensure Unity version **6000.2.5f1** is installed. Unity Hub will prompt to install it if missing.
3. **Resolve packages:** On first open, Unity will import packages from `Packages/manifest.json`. Allow the import to complete before interacting with the Editor.
4. **FMOD setup:** If FMOD banks are not pre-built, open FMOD Studio, load the project's FMOD project file, and build banks to `Assets/StreamingAssets/`.
5. **Run in Editor:** Open the main scene and press **Play**. For on-device testing, configure the Meta Quest 3 build profile under **File → Build Settings** (Android, OpenXR).

---

# 🛠️ Tools Used

| Tool | Purpose |
|---|---|
| **Unity 6** | Game engine and XR runtime |
| **Blender** | 3D modeling and animation |
| **FMOD Studio** | 3D spatial audio design |
| **Meta XR SDK** | Hand-tracking and Quest 3 integration |

---

# 🙏 Acknowledgments

Some models were imported from the [Poly Pizza website](https://poly.pizza/). Here are the authors of those models:

- [Google](https://poly.pizza/u/Poly%20by%20Google)
- [Quaternius](https://poly.pizza/u/Quaternius)
- [J-Toastie](https://poly.pizza/u/J-Toastie)
- [sirkitree](https://poly.pizza/u/sirkitree)

---

# 📝 Extra Notes

- Part of this project was developed for the [Virtual And Augmented Reality](https://www.ua.pt/en/uc/12023) and [Serious Games](https://www.ua.pt/en/uc/15477) courses at the **University of Aveiro**.
- Development period: 2025–Present.
- Source repository: [github.com/DavidPalricas/CountryFair-VR](https://github.com/DavidPalricas/CountryFair-VR)
