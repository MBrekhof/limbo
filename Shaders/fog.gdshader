shader_type canvas_item;

uniform vec2 scroll_speed = vec2(20.0, 5.0);
uniform float density : hint_range(0.0, 1.0) = 0.3;
uniform vec4 color : source_color = vec4(0.7, 0.7, 0.7, 0.2);
uniform float time_scale : hint_range(0.1, 5.0) = 1.0;

// Hash-based value noise — replaces the old sin/cos combo that produced
// visible repeating ripples.
float hash(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

float value_noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(
        mix(hash(i), hash(i + vec2(1.0, 0.0)), u.x),
        mix(hash(i + vec2(0.0, 1.0)), hash(i + vec2(1.0, 1.0)), u.x),
        u.y);
}

float fbm(vec2 p) {
    float v = 0.0;
    float amp = 0.5;
    for (int i = 0; i < 4; i++) {
        v += amp * value_noise(p);
        p = p * 2.03 + vec2(17.3, 9.1);
        amp *= 0.5;
    }
    return v;
}

void fragment() {
    vec2 uv = UV;
    float t = TIME * time_scale;

    // Scroll the UVs to create drifting motion.
    vec2 scrolled_uv = uv + scroll_speed * t * 0.001;

    // Light domain warp for wispy, curling shapes.
    vec2 p = scrolled_uv * 6.0 + vec2(t * 0.03, t * 0.01);
    float warp = fbm(p * 0.5 + vec2(-t * 0.02, t * 0.015));
    float fog_value = fbm(p + warp * 1.5);

    // Apply density control.
    float combined = smoothstep(0.5 - density * 0.5, 0.5 + density * 0.5, fog_value);

    // Soft edges: fade at the top and bottom.
    float edge_fade = smoothstep(0.0, 0.15, uv.y) * smoothstep(1.0, 0.85, uv.y);
    combined *= edge_fade;

    COLOR = vec4(color.rgb, combined * color.a);
}
