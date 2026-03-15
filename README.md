\# DiceSpirit — Unity Assignment

\*\*Submitted by:\*\* Nikhil Chaudhary || nikhilchaudhary285@gmail.com

\*\*Position:\*\* Unity Developer — Evolx Games

\*\*Date:\*\* 15-03-2026



\---



\## Unity Version

Unity \*\*6000.0.x LTS\*\* (Unity 6) || Current Version Used: 6000.3.11f1

Render Pipeline: Universal Render Pipeline (URP)



\---



\## Setup Instructions

1\. Unzip the project folder

2\. Open \*\*Unity Hub\*\* → Click \*\*"Open"\*\* → Select the `DiceSpirit` folder

3\. Unity will import assets automatically (1–2 minutes first open)

4\. If prompted to upgrade the project, click \*\*"Confirm"\*\*

5\. Open `Assets/\\\_Project/Scenes/GameScene.unity`

6\. Press \*\*Play\*\*



> No external packages required beyond TextMeshPro (auto-imported by Unity).



\---



\## Controls

| Input | Action |

|---|---|

| Click \*\*ROLL\*\* | Roll the dice |

| Click \*\*Force Roll = 3\*\* | Forces next roll result to 3 (CardB trigger) |

| Click \*\*Force Roll = 6\*\* | Forces next roll result to 6 (CardA trigger) |

| Click \*\*Clear Force\*\* | Returns to random rolls |



\---



\## Game Rules

\- Dice produces a result 1–6

\- Default equation: \*\*Points × 10 = Total\*\*

\- \*\*Spirit Card A — Fortune's Edge:\*\* If dice = 6, Multiplier overrides to 2 → `6 × 2 = 12`

\- \*\*Spirit Card B — Trinity's Gift:\*\* If dice = 3, +10 added to Points → `13 × 10 = 130`

\- Cards apply after the roll finalises, before the equation displays



\---



\## Architecture Overview

```

GameEvents (static C# event bus)

subscribed by all systems — zero direct cross-references


DiceRoller         -> emits OnRollComplete(int)

GameCalculator     -> owns Points/Multiplier/Total state

SpiritCardManager  -> checks SpiritCardData conditions, applies effects

UIEquationView     -> animates number transitions (count-up, bounce, flash)

SpiritCardView     -> per-card VFX (glow, pulse, particles) on activation

UIRollHistory      -> sliding window of last 5 results

AudioManager       -> pooled AudioSources, event-driven SFX

```



\*\*Design Patterns used:\*\*

\- \*\*Observer / Event Bus\*\* — `GameEvents` static class decouples all systems

\- \*\*Data-Driven Design\*\* — Spirit Card rules live in ScriptableObjects, not code

\- \*\*Object Pool\*\* — AudioManager pools 6 AudioSources for overlapping SFX

\- \*\*Single Responsibility\*\* — every script does exactly one job

\- \*\*SOLID principles\*\* — Open/Closed: adding a new Spirit Card requires zero code changes



\---



\## Project Structure

```

Assets/\\\_Project/

├── Scripts/

│   ├── Core/          GameEvents, GameCalculator, AudioManager

│   ├── Dice/          DiceRoller

│   ├── SpiritCards/   SpiritCardData (SO), SpiritCardManager, SpiritCardView

│   ├── UI/            UIEquationView, UIRollButton, UIRollHistory, DebugPanel

│   └── VFX/           NumberJuice

├── ScriptableObjects/ CardA\\\_Multiplier, CardB\\\_BonusPoints

├── Prefabs/           Dice, Cards, UI elements

├── Materials/         DiceMaterial

├── Audio/             roll\\\_start, roll\\\_settle, card\\\_trigger, number\\\_tick

└── Scenes/            GameScene


└── Sprites/           Dice Sprites
    
   └── Normal\_Sprites/ Dice Face Sprites


&#x20;  └── TMPro\_Sprites/  Dice Face TMPro\_Sprite Asset Sprites

```



\---



\## Third-Party Assets

| Asset | Source | License |

|---|---|---|

| Audio SFX (4 clips) | Generated via sfxr.me | Free, royalty-free |
| Dice Sprites (1-6) | Custom made via Canva | Free for use / Custom |

| TextMeshPro | Unity Package (built-in) | Unity Companion License |



> No paid assets used. All assets are either Unity built-ins or freely generated.



\---



\## Implementation Notes



\*\*Why URP?\*\* Emission support for card glow effects without custom shaders.



\*\*Why ScriptableObjects for Spirit Cards?\*\* Allows adding new cards without

touching C# code. A designer can create a new `SpiritCardData` asset, set the

trigger value and effect, and it works automatically.



\*\*Why static C# events over UnityEvents?\*\* Static events have zero Inspector

wiring overhead, compile-time safety, and better performance. UnityEvents are

better for designer-driven connections; C# events are better for programmer-

driven architecture like this.



\*\*Coroutine animation choice:\*\* All animations run as coroutines rather than

Update()-based tweens to keep animation logic self-contained and easy to

interrupt cleanly when a new roll starts mid-animation.



\*\*Audio pool:\*\* 6 pooled AudioSources allow rapid overlapping ticks during

number count-up without sounds cutting each other off.

```

\\\*"Let's walk through on architecture"\\\*:


> "The entire project is built on an event bus — a static `GameEvents` class. No script holds a direct reference to another script. The `DiceRoller` fires an event when the animation finishes. `GameCalculator` listens, sets the initial values, then passes itself to `SpiritCardManager` through a second event. The manager checks each `SpiritCardData` ScriptableObject — if its condition matches the dice result, it modifies the calculator's state and fires a card activation event. Then `GameCalculator` computes the final total and broadcasts it. The UI layer only ever listens and displays — it never touches state. Adding a new Spirit Card means creating one ScriptableObject asset. Zero code changes needed."


\*"Why ScriptableObjects"\\\*:


> "Because card data belongs in data files, not in code. If the trigger condition or effect value is hardcoded in a script, every change requires a programmer and a recompile. With ScriptableObjects, a designer can create a new card, fill in the trigger value and effect type in the Inspector, and it works immediately. The system is open to extension without modification — that's the Open/Closed principle."


\\## Complete Project Summary

Step 1–2   Unity 6 project created, folder structure built

Step 3–4   Scene saved, GameEvents event bus created

Step 5–6   SpiritCardData ScriptableObject + CardA + CardB assets

Step 7     GameCalculator + SpiritCardManager + DiceRoller + UIRollButton

Step 8     NumberJuice + UIEquationView + SpiritCardView + UIRollHistory + DebugPanel

Step 9     Full scene assembled, all fields wired, play tested

Step 10    AudioManager + SFX + README

