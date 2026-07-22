# Graphics Overhaul — Cinematic Procedural Pass

Date: 2026-07-22 · Approved by Martin (session dialogue)

## Goal

Bring the visuals to the next level while staying 100% procedural (no image assets),
grayscale, and on `gl_compatibility`. Three fronts: atmosphere, player polish,
per-level identity.

## Changes

### 1. Fog shader rewrite (`Shaders/fog.gdshader`)

Current "noise" is a sin/cos wave combo producing visible repeating ripples.
Replace with hash-based value noise + 4-octave FBM + light domain warp.
Uniform names unchanged (`scroll_speed`, `density`, `color`, `time_scale`) so
`FogLayer.cs` and `FogOverlay.tscn` keep working untouched.

### 2. God rays (`Shaders/god_rays.gdshader` + `Scenes/Environment/GodRays.tscn`)

Procedural diagonal light shafts: 1-D value noise across a skewed X axis, faded
toward the bottom of the screen, `blend_add`, animated slowly. Full-screen
ColorRect in a CanvasLayer at layer 4 (above dust=3, below fog=5, so fog drifts
in front of the shafts). Intensity driven per level by AtmosphereController.

### 3. Parallax depth blur (`Shaders/depth_blur.gdshader` + edit `ParallaxBackground.tscn`)

The two far layers (mountains, dead trees/ruins) render pin-sharp, killing depth.
Insert a screen-reading blur+haze ColorRect between Layer2 and Layer3 in canvas
order: a `Parallax2D` with `scroll_scale (0,0)` (screen-fixed) containing an
oversized ColorRect whose shader samples `hint_screen_texture` (9-tap ring blur)
and mixes toward a haze gray. Everything drawn later (near layers, gameplay)
stays sharp — Limbo's depth-of-field look. Godot auto-inserts the backbuffer
copy for mid-canvas screen reads; the film-grain shader already relies on the
same mechanism.

### 4. Player polish (`Scripts/Player/PlayerAnimator.cs`)

- Tiny light-gray eyes on the head (Idle/Walk/Jump/Fall — not Death), with a
  periodic blink (~every 4 s). The Limbo boy look.
- Squash & stretch: stretch on entering Jump, squash on landing (Fall → grounded),
  decaying back to 1.0; combined with the existing facing flip on `Scale`.
- Dust puffs: a small one-shot `GpuParticles2D` created in code at the feet;
  burst on landing, tiny puff on each stride frame.

### 5. Per-level atmosphere identity

New `Scripts/Visuals/AtmosphereController.cs` on the `EnvironmentTemplate` root.
Exports: `AmbientColor`, `FogDensity`, `GodRayStrength`, `Signature` (enum).
On ready it pushes values into AmbientLight, FogLayer, the GodRays shader, and a
new `SignatureParticles` node (`Scripts/Visuals/SignatureParticles.cs`,
GpuParticles2D in a CanvasLayer, configured per mode in code):

| Level      | Ambient              | Fog  | God rays | Signature |
|------------|----------------------|------|----------|-----------|
| Forest     | 0.60, 0.65, 0.70     | 0.30 | 0.35     | Fireflies (slow soft dots) |
| Industrial | 0.55, 0.52, 0.50     | 0.25 | 0.18     | Embers (rising sparks)     |
| Depths     | 0.38, 0.42, 0.50     | 0.45 | 0.08     | Drips (falling streaks)    |

Levels override these as instance-root properties on their `Environment` node.

### 6. Post stack tuning

Vignette intensity 0.7 → 0.8, softness 0.4 → 0.35. Film grain unchanged.
No bloom: `gl_compatibility` has no 2D HDR glow; light "glow" stays with the
existing PointLight2D gradient textures.

## Rejected alternative

Switching to Forward+ for real 2D bloom — repo deliberately targets
`gl_compatibility` for broad support; bloom is the only gain.

## Verification

`dotnet build` clean; Godot headless scene load check if a CLI is available;
manual F5 pass by Martin (no automated screenshot path for Godot here).
