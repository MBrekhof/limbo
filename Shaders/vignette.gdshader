shader_type canvas_item;

uniform float intensity : hint_range(0.0, 1.5) = 0.7;
uniform float softness : hint_range(0.01, 1.0) = 0.4;

void fragment() {
    // Distance from center of screen.
    vec2 center = UV - vec2(0.5, 0.5);
    float dist = length(center);

    // Vignette factor: darken edges, lighter in center.
    float vignette = smoothstep(softness, softness + 0.5, dist * (1.0 + intensity));

    COLOR = vec4(0.0, 0.0, 0.0, vignette);
}
