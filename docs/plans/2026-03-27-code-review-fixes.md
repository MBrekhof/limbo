# Code Review Fixes Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Fix all 19 findings from the code review — 3 critical, 8 important, 8 recommendations.

**Architecture:** Incremental fixes grouped by dependency. Start with project-level settings (NRT), then critical bugs, then shared infrastructure (helper method), then remaining fixes in dependency order. Each task is a single commit.

**Tech Stack:** Godot 4 (.NET/C#), .NET 8, Godot.NET.Sdk 4.6.0

---

### Task 1: Enable nullable reference types

**Files:**
- Modify: `limbo.csproj`

**Step 1: Add NRT to project**

In `limbo.csproj`, add `<Nullable>enable</Nullable>` inside the `<PropertyGroup>`:

```xml
<Project Sdk="Godot.NET.Sdk/4.6.0">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <TargetFramework Condition="$([MSBuild]::IsOSPlatform('linux'))">net8.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <RootNamespace>Limbo</RootNamespace>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

**Step 2: Build and catalog warnings**

Run: `dotnet build`
Expected: Build succeeds with nullable warnings (CS8618, CS8600, CS8602, etc.)

**Step 3: Fix all nullable warnings across the codebase**

Apply `?` annotations and null checks as needed. Key patterns:
- Singleton `Instance` properties → `public static GameManager? Instance { get; private set; }`
- Private node fields set in `_Ready()` → mark as nullable or use `null!` with a comment
- `GetNode<T>()` results → switch to `GetNodeOrNull<T>()` where safe fallback exists (this overlaps with Task 4)

**Step 4: Build to verify zero warnings**

Run: `dotnet build`
Expected: 0 warnings, 0 errors

**Step 5: Commit**

```
fix: enable nullable reference types and fix all warnings
```

---

### Task 2: Fix DeathZone to call PlayerController.Die()

**Files:**
- Modify: `Scripts/Puzzles/DeathZone.cs`

**Step 1: Fix the bug**

Replace the `OnBodyEntered` method to cast the body and call `Die()`:

```csharp
private void OnBodyEntered(Node2D body)
{
    if (body is PlayerController player)
    {
        player.Die();
    }
}
```

This properly sets `_isDead`, zeroes velocity, disables physics, sets death animation, and emits the signal — all through the existing `Die()` method.

**Step 2: Build**

Run: `dotnet build`
Expected: 0 errors

**Step 3: Commit**

```
fix: DeathZone now calls PlayerController.Die() instead of bypassing it
```

---

### Task 3: Wrap async void methods with try/catch and post-await validation

**Files:**
- Modify: `Scripts/Core/LevelManager.cs`
- Modify: `Scripts/Player/RespawnManager.cs`

**Step 1: Fix LevelManager.OnLevelTransitionRequested**

Wrap the entire body in try/catch. Add `IsInsideTree()` checks after each await:

```csharp
private async void OnLevelTransitionRequested(string levelPath)
{
    try
    {
        Tween fadeOut = CreateTween();
        fadeOut.TweenProperty(_fadeRect, "color:a", 1.0f, 0.5f);
        await ToSignal(fadeOut, Tween.SignalName.Finished);

        if (!IsInsideTree()) return;

        GameManager.Instance?.ResetCheckpoint();
        GetTree().ChangeSceneToFile(levelPath);

        await ToSignal(GetTree(), SceneTree.SignalName.TreeChanged);
        if (!IsInsideTree()) return;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        if (!IsInsideTree()) return;

        Node root = GetTree().CurrentScene;
        if (root != null)
        {
            Marker2D? spawn = root.GetNodeOrNull<Marker2D>("PlayerSpawn");
            if (spawn != null && GameManager.Instance != null)
            {
                GameManager.Instance.LevelStartPosition = spawn.GlobalPosition;
            }
        }

        Tween fadeIn = CreateTween();
        fadeIn.TweenProperty(_fadeRect, "color:a", 0.0f, 0.5f);
        await ToSignal(fadeIn, Tween.SignalName.Finished);
    }
    catch (System.Exception ex)
    {
        GD.PrintErr($"Level transition failed: {ex.Message}");
    }
}
```

**Step 2: Fix RespawnManager.OnPlayerDied**

Wrap in try/catch, add post-await tree validation:

```csharp
private async void OnPlayerDied()
{
    try
    {
        await ToSignal(GetTree().CreateTimer(RespawnDelay), SceneTreeTimer.SignalName.Timeout);

        if (!IsInsideTree() || _player == null) return;

        Vector2 respawnPosition = GameManager.Instance?.RespawnPosition ?? Vector2.Zero;
        _player.Respawn(respawnPosition);
        GameManager.Instance?.OnPlayerRespawned();
    }
    catch (System.Exception ex)
    {
        GD.PrintErr($"Respawn failed: {ex.Message}");
    }
}
```

**Step 3: Build**

Run: `dotnet build`
Expected: 0 errors

**Step 4: Commit**

```
fix: wrap async void methods with try/catch and post-await tree validation
```

---

### Task 4: Add singleton guards to GameManager and LevelManager

**Files:**
- Modify: `Scripts/Core/GameManager.cs`
- Modify: `Scripts/Core/LevelManager.cs`

**Step 1: Add duplicate guard to GameManager._Ready()**

```csharp
public override void _Ready()
{
    if (Instance != null && Instance != this)
    {
        GD.PushWarning("Duplicate GameManager detected. Freeing this instance.");
        QueueFree();
        return;
    }
    Instance = this;
    ProcessMode = ProcessModeEnum.Always;
}
```

**Step 2: Add duplicate guard to LevelManager._Ready()**

Same pattern, plus null-check `GameManager.Instance` before subscribing:

```csharp
public override void _Ready()
{
    if (Instance != null && Instance != this)
    {
        GD.PushWarning("Duplicate LevelManager detected. Freeing this instance.");
        QueueFree();
        return;
    }
    Instance = this;
    ProcessMode = ProcessModeEnum.Always;

    // ... create fade overlay ...

    if (GameManager.Instance != null)
    {
        GameManager.Instance.LevelTransitionRequested += OnLevelTransitionRequested;
    }
    else
    {
        GD.PushWarning("LevelManager: GameManager not ready. Ensure autoload order.");
    }
}
```

**Step 3: Build**

Run: `dotnet build`
Expected: 0 errors

**Step 4: Commit**

```
fix: add singleton duplicate guards to GameManager and LevelManager
```

---

### Task 5: Extract shared target-resolution helper and cache targets in _Ready()

**Files:**
- Create: `Scripts/Puzzles/TargetResolver.cs`
- Modify: `Scripts/Puzzles/LeverSwitch.cs`
- Modify: `Scripts/Puzzles/PressurePlate.cs`

**Step 1: Create TargetResolver helper**

```csharp
using Godot;

namespace Limbo;

public static class TargetResolver
{
    public static IActivatable[] ResolveTargets(Node owner, NodePath[]? targets)
    {
        if (targets == null || targets.Length == 0)
            return [];

        var resolved = new System.Collections.Generic.List<IActivatable>();
        foreach (NodePath path in targets)
        {
            if (path == null) continue;
            Node? node = owner.GetNodeOrNull(path);
            if (node is IActivatable activatable)
            {
                resolved.Add(activatable);
            }
            else if (node != null)
            {
                GD.PushWarning($"{owner.Name}: Target '{path}' is not IActivatable.");
            }
            else
            {
                GD.PushWarning($"{owner.Name}: Target '{path}' not found.");
            }
        }
        return resolved.ToArray();
    }

    public static void SetTargets(IActivatable[] targets, bool active)
    {
        foreach (var target in targets)
        {
            if (active)
                target.Activate();
            else
                target.Deactivate();
        }
    }
}
```

**Step 2: Refactor LeverSwitch to use TargetResolver**

```csharp
using Godot;

namespace Limbo;

public partial class LeverSwitch : StaticBody2D, IInteractable
{
    [Export] public NodePath[] Targets { get; set; } = [];

    private bool _isOn;
    private ColorRect? _visual;
    private IActivatable[] _resolvedTargets = [];

    public override void _Ready()
    {
        AddToGroup(Constants.GroupInteractable);
        _visual = GetNodeOrNull<ColorRect>("Visual");
        _resolvedTargets = TargetResolver.ResolveTargets(this, Targets);
    }

    public void Interact(Node2D interactor)
    {
        _isOn = !_isOn;

        if (_visual != null)
        {
            _visual.RotationDegrees = _isOn ? 45.0f : 0.0f;
        }

        TargetResolver.SetTargets(_resolvedTargets, _isOn);
    }
}
```

**Step 3: Refactor PressurePlate to use TargetResolver**

```csharp
using Godot;

namespace Limbo;

public partial class PressurePlate : Area2D
{
    [Export] public NodePath[] Targets { get; set; } = [];

    private int _bodyCount;
    private bool _isActive;
    private ColorRect? _visual;
    private IActivatable[] _resolvedTargets = [];

    public override void _Ready()
    {
        _visual = GetNodeOrNull<ColorRect>("Visual");
        _resolvedTargets = TargetResolver.ResolveTargets(this, Targets);

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup(Constants.GroupPlayer) || body.IsInGroup(Constants.GroupPushable))
        {
            _bodyCount++;
            if (_bodyCount == 1)
            {
                SetActive(true);
            }
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup(Constants.GroupPlayer) || body.IsInGroup(Constants.GroupPushable))
        {
            _bodyCount--;
            if (_bodyCount <= 0)
            {
                _bodyCount = 0;
                SetActive(false);
            }
        }
    }

    private void SetActive(bool active)
    {
        if (_isActive == active)
            return;

        _isActive = active;

        if (_visual != null)
        {
            _visual.Scale = _isActive ? new Vector2(1.0f, 0.5f) : new Vector2(1.0f, 1.0f);
        }

        TargetResolver.SetTargets(_resolvedTargets, _isActive);
    }
}
```

**Step 4: Build**

Run: `dotnet build`
Expected: 0 errors

**Step 5: Commit**

```
refactor: extract TargetResolver helper, cache targets in _Ready()
```

---

### Task 6: Fix Door collision asymmetry

**Files:**
- Modify: `Scripts/Puzzles/Door.cs`

**Step 1: Fix AnimateDoor to defer collision re-enable to after close animation**

```csharp
private void AnimateDoor(Vector2 targetPosition, bool opening)
{
    _tween?.Kill();
    _tween = CreateTween();
    _tween.TweenProperty(this, "position", targetPosition, 1.0 / OpenSpeed);

    if (_collisionShape != null)
    {
        if (opening)
        {
            _tween.TweenCallback(Callable.From(() =>
            {
                _collisionShape.Disabled = true;
            }));
        }
        else
        {
            _tween.TweenCallback(Callable.From(() =>
            {
                _collisionShape.Disabled = false;
            }));
        }
    }
}
```

**Step 2: Build**

Run: `dotnet build`
Expected: 0 errors

**Step 3: Commit**

```
fix: Door collision now re-enables after close animation completes
```

---

### Task 7: Remove dead code and fix remaining important issues

**Files:**
- Modify: `Scripts/Player/PlayerController.cs` (remove unreachable `_isDead` check)
- Modify: `Scripts/Visuals/FogLayer.cs` (remove duplicate method)
- Modify: `Scripts/Visuals/SwayEffect.cs` (fix field exports to properties, use delta-based time)

**Step 1: Remove unreachable dead code in PlayerController**

In `_PhysicsProcess`, the `_isDead` check at line 109 is unreachable because of the early return at line 40. Remove lines 109-112:

```csharp
// After MoveAndSlide() and facing direction update, the animation state block becomes:
if (!IsOnFloor() && Velocity.Y < 0)
{
    CurrentAnimState = AnimState.Jump;
}
else if (!IsOnFloor() && Velocity.Y > 0)
{
    CurrentAnimState = AnimState.Fall;
}
else if (Mathf.Abs(Velocity.X) > 10)
{
    CurrentAnimState = AnimState.Walk;
}
else
{
    CurrentAnimState = AnimState.Idle;
}
```

**Step 2: Remove duplicate method in FogLayer**

Remove `UpdateScrollSpeed` entirely and have `_Process` call `ApplyShaderParams`:

```csharp
public override void _Process(double delta)
{
    ApplyShaderParams(_fogRect1, 1.0f);
    ApplyShaderParams(_fogRect2, 0.6f);
}
```

**Step 3: Fix SwayEffect — properties + delta-based time**

```csharp
using Godot;

namespace Limbo;

public partial class SwayEffect : Node2D
{
    [Export] public float SwayAmount { get; set; } = 3.0f;
    [Export] public float SwaySpeed { get; set; } = 1.5f;

    private float _timeAccumulator;
    private float _timeOffset;

    public override void _Ready()
    {
        _timeOffset = (float)GD.RandRange(0, Mathf.Tau);
    }

    public override void _Process(double delta)
    {
        _timeAccumulator += (float)delta;
        RotationDegrees = Mathf.Sin(_timeAccumulator * SwaySpeed + _timeOffset) * SwayAmount;
    }
}
```

**Step 4: Build**

Run: `dotnet build`
Expected: 0 errors

**Step 5: Commit**

```
refactor: remove dead code, duplicate methods, fix SwayEffect to use delta time
```

---

### Task 8: Remaining GetNode safety and style fixes

**Files:**
- Modify: `Scripts/Player/PlayerController.cs` (GetNodeOrNull for HazardDetector)
- Modify: `Scripts/Puzzles/Door.cs` (GetNodeOrNull for CollisionShape)
- Modify: `Scripts/Puzzles/FallingPlatform.cs` (GetNodeOrNull for nodes)
- Modify: `Scripts/UI/DeathScreen.cs` (GetNodeOrNull for Overlay)
- Modify: `Scripts/Puzzles/MovingPlatform.cs` (standardize array init)

**Step 1: Apply GetNodeOrNull pattern across all files**

For each file, replace `GetNode<T>("X")` with `GetNodeOrNull<T>("X")` and add appropriate null guards in methods that use the result. The exact pattern depends on the file:

- `PlayerController._Ready()`: HazardDetector is critical — use GetNodeOrNull + early return with warning
- `Door._Ready()`: CollisionShape — already null-checked in `AnimateDoor`, just need GetNodeOrNull
- `FallingPlatform._Ready()`: All three nodes — use GetNodeOrNull, already has null guards in usage
- `DeathScreen._Ready()`: Overlay — needs null guard in `OnPlayerDied`/`OnPlayerRespawned`

**Step 2: Standardize array initialization to C# 12 `[]` syntax**

In `MovingPlatform.cs` line 7:
```csharp
// Before:
[Export] public Vector2[] Waypoints { get; set; } = System.Array.Empty<Vector2>();
// After:
[Export] public Vector2[] Waypoints { get; set; } = [];
```

(LeverSwitch and PressurePlate already fixed in Task 5)

**Step 3: Build**

Run: `dotnet build`
Expected: 0 errors

**Step 4: Commit**

```
fix: use GetNodeOrNull for scene-dependent nodes, standardize array init syntax
```

---

### Task 9: Delete CODE_REVIEW.md

**Step 1: Remove the review file**

The findings are now resolved. Delete `CODE_REVIEW.md`.

**Step 2: Commit**

```
chore: remove CODE_REVIEW.md after all findings resolved
```
