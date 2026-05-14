🎲 DiceSpirit — Unity Assignment
Unity 6 • Event-Driven Architecture • ScriptableObject Systems
A modular dice-based gameplay prototype built in Unity, focused on event-driven architecture, data-driven gameplay systems, and scalable Spirit Card mechanics.
---
👨‍💻 Submission Details
Submitted By: Nikhil Chaudhary  
Email: nikhilchaudhary285@gmail.com
Position: Unity Developer — Evolx Games
Date: 15-03-2026
---
🛠 Unity Version
Unity 6000.0.x LTS (Unity 6)
Current Version Used: 6000.3.11f1
Render Pipeline: Universal Render Pipeline (URP)
---
▶️ Setup Instructions
Unzip the project folder
Open Unity Hub
Click Open and select the `DiceSpirit` folder
Wait for Unity to import assets (first launch may take 1–2 minutes)
If prompted to upgrade the project, click Confirm
Open scene: `Assets/_Project/Scenes/GameScene.unity`
Press Play
> No external packages required beyond TextMeshPro (auto-imported by Unity).
---
🎮 Controls
Input	Action
Click ROLL	Roll the dice
Click Force Roll = 3	Forces next roll result to 3
Click Force Roll = 6	Forces next roll result to 6
Click Clear Force	Returns to random rolls
---
🎯 Game Rules
Dice produces a result from 1–6
Default equation: Points × 10 = Total
Spirit Card A — Fortune's Edge
If dice result equals 6:
```txt
6 × 2 = 12
```
Multiplier overrides to 2
---
Spirit Card B — Trinity's Gift
If dice result equals 3:
```txt
13 × 10 = 130
```
Adds +10 bonus points before calculation
---
Cards apply after the dice roll finalizes and before the final equation displays.
---
🏗 Architecture Overview
The entire project follows an event-driven and modular architecture where systems communicate through a centralized event bus.
```txt
GameEvents (Static Event Bus)
│
├── DiceRoller
│   └── Emits OnRollComplete(int)
│
├── GameCalculator
│   └── Owns Points / Multiplier / Total state
│
├── SpiritCardManager
│   └── Evaluates SpiritCardData rules
│
├── UIEquationView
│   └── Handles animated UI number transitions
│
├── SpiritCardView
│   └── Plays card VFX and visual feedback
│
├── UIRollHistory
│   └── Displays last 5 dice results
│
└── AudioManager
    └── Event-driven pooled audio playback
```
---
🧠 Design Patterns Used
Observer / Event Bus
A centralized `GameEvents` static class decouples gameplay systems completely.
Benefits
Zero direct references between systems
Cleaner architecture
Easier scalability
Safer system communication
---
Data-Driven Design
All Spirit Card logic exists inside ScriptableObjects rather than hardcoded scripts.
Benefits
Designer-friendly workflow
No code modifications required
Fast iteration speed
Easily extensible systems
---
Object Pooling
`AudioManager` pools multiple AudioSources for overlapping sound playback.
Benefits
Prevents audio cutoff
Reduces allocations
Improves runtime performance
---
Single Responsibility Principle
Each script is responsible for only one system or feature.
Examples
Dice rolling
Calculation logic
UI display
Audio playback
Spirit Card effects
---
SOLID Principles
The project follows extensible architecture principles.
Example
Adding a new Spirit Card requires:
Creating a new `SpiritCardData` asset
Setting values in Inspector
No code changes required
---
📂 Project Structure
```txt
Assets/_Project/
├── Scripts/
│   ├── Core/
│   │   ├── GameEvents
│   │   ├── GameCalculator
│   │   └── AudioManager
│   │
│   ├── Dice/
│   │   └── DiceRoller
│   │
│   ├── SpiritCards/
│   │   ├── SpiritCardData
│   │   ├── SpiritCardManager
│   │   └── SpiritCardView
│   │
│   ├── UI/
│   │   ├── UIEquationView
│   │   ├── UIRollButton
│   │   ├── UIRollHistory
│   │   └── DebugPanel
│   │
│   └── VFX/
│       └── NumberJuice
│
├── ScriptableObjects/
│   ├── CardA_Multiplier
│   └── CardB_BonusPoints
│
├── Prefabs/
├── Materials/
├── Audio/
├── Scenes/
└── Sprites/
```
---
🔊 Third-Party Assets
Asset	Source	License
Audio SFX	Generated using sfxr.me	Free / Royalty-Free
Dice Sprites	Custom made via Canva	Free for use
TextMeshPro	Unity Built-In Package	Unity Companion License
> No paid assets used.
---
⚙️ Implementation Notes
Why URP?
URP was chosen for emission-based glow effects and lightweight rendering.
Benefits
Better visual effects
Improved performance
Cleaner material workflows
---
Why ScriptableObjects?
Spirit Card logic belongs in reusable data assets rather than gameplay scripts.
Benefits
Designer-friendly architecture
Easy balancing
Faster iteration
Open/Closed principle support
---
Why Static C# Events?
Static events provide lightweight and high-performance communication between systems.
Benefits
Compile-time safety
No Inspector wiring
Cleaner dependencies
Better scalability
---
Why Coroutines for Animation?
All animations use coroutines instead of Update-based tweens.
Benefits
Self-contained animation flow
Easier interruption handling
Cleaner animation logic
---
Why Audio Pooling?
Multiple pooled AudioSources allow rapid overlapping UI tick sounds.
Benefits
Prevents audio interruption
Cleaner sound playback
Lower runtime allocation cost
---
🧪 Extension Examples
Feature	Implementation Approach
New Spirit Card	Create new `SpiritCardData` asset
New UI Animation	Add coroutine-based visual effect
Additional Dice Types	Extend DiceRoller logic
Combo System	Subscribe to roll events in new system
Multiplayer Support	Replace local event flow with network events
---
📚 Key Learnings
This project helped improve understanding of:
Event-driven architecture
ScriptableObject workflows
Modular gameplay systems
Data-driven design
UI animation architecture
Audio pooling systems
Decoupled Unity architecture
SOLID principles in gameplay programming
---
📝 Complete Project Summary
Step	Description
Step 1–2	Unity 6 project setup and folder structure
Step 3–4	Event bus and gameplay systems
Step 5–6	Spirit Card ScriptableObjects
Step 7	DiceRoller and gameplay logic
Step 8	UI systems and visual effects
Step 9	Full gameplay assembly and testing
Step 10	AudioManager, SFX integration, README
