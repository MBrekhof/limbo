# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Limbo-inspired 2D puzzle-platformer built with Godot 4 (.NET/C#). Dark silhouette art style with atmospheric fog, parallax backgrounds, and physics-based puzzles.

## Build & Run

```bash
# Build C# solution (from project root)
dotnet build

# Run from Godot Editor: open project.godot, press F5
# Note: Godot.NET.Sdk version in limbo.csproj must match your installed Godot version
```

## Architecture

### Core Systems (Scripts/Core/)
- **GameManager.cs** — Autoload singleton. Holds respawn state, emits `PlayerDied`, `PlayerRespawned`, `LevelTransitionRequested` signals.
- **LevelManager.cs** — Autoload singleton. Handles scene transitions with fade effect.
- **Constants.cs** — All shared constants: input actions, physics layer masks, node groups, scene paths.

### Conventions
- **Namespace:** `Limbo` for all C# files
- **Physics layers:** 1=Environment, 2=Player, 3=Puzzles, 4=Hazards, 5=Triggers
- **Node groups:** `player`, `hazard`, `pushable`, `interactable`, `checkpoint`
- **Input actions:** `move_left`, `move_right`, `jump`, `interact` (A/D + arrows, Space/W, E)
- **Interfaces:** `IInteractable` for player-triggered objects, `IActivatable` for switch-connected objects
- **Signals:** Use Godot signal delegates (`[Signal] public delegate void XxxEventHandler()`)
- **Exports:** Use `[Export]` for all tunable gameplay parameters

### Directory Layout
```
Scripts/Core/        — Singletons, constants, shared logic
Scripts/Interfaces/  — IInteractable, IActivatable
Scripts/Player/      — PlayerController, PlayerCamera, RespawnManager
Scripts/Puzzles/     — PushableBox, LeverSwitch, PressurePlate, MovingPlatform, etc.
Scripts/Visuals/     — ParallaxController, FogLayer, DustParticles, AmbientLight
Scripts/UI/          — DeathScreen
Scenes/              — .tscn files mirroring Scripts/ structure
Shaders/             — .gdshader files (vignette, fog, silhouette)
```

### Puzzle Wiring Pattern
Switches/plates → set `[Export] NodePath[] Targets` → call `Activate()`/`Deactivate()` on resolved IActivatable nodes. All puzzle-to-puzzle communication goes through the IActivatable interface.

### Visual Style
Grayscale world with ONE deliberate exception: the player's yellow cap (`CapColor` in PlayerAnimator.cs) is the game's single saturated accent — nothing else may use a saturated hue. Foreground = black silhouettes. Background = layered grays via ParallaxBackground. Atmosphere = scrolling fog + dust particles + vignette overlay. Rendering uses `gl_compatibility` for broad support.

## Git Conventions

- Use [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `refactor:`, `chore:`, etc.
- Godot.NET.Sdk version in `limbo.csproj` must match the installed Godot editor version exactly.
