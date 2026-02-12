shader_type canvas_item;

uniform vec2 scroll_speed = vec2(20.0, 5.0);
uniform float density : hint_range(0.0, 1.0) = 0.3;
uniform vec4 color : source_color = vec4(0.7, 0.7, 0.7, 0.2);
uniform float time_scale : hint_range(0.1, 5.0) = 1.0;

// Procedural noise based on sin/cos wave combination.
float noise(vec2 p) {
    return 0.5 + 0.5 * sin(p.x * 1.3 + sin(p.y * 0.9 + p.x * 0.7))
         * cos(p.y * 1.1 + cos(p.x * 0.8 + p.y * 0.6));
}

float layered_noise(vec2 p) {
    float n = 0.0;
    n += noise(p * 1.0) * 0.5;
    n += noise(p * 2.4 + vec2(3.7, 1.2)) * 0.25;
    n += noise(p * 5.1 + vec2(7.3, 4.8)) * 0.125;
    return n / 0.875;
}

void fragment() {
    vec2 uv = UV;
    float t = TIME * time_scale;

    // Scroll the UVs to create drifting motion.
    vec2 scroll_offset = scroll_speed * t * 0.001;
    vec2 scrolled_uv = uv + scroll_offset;

    // Generate wispy fog from layered procedural noise.
    float fog_value = layered_noise(scrolled_uv * 6.0 + vec2(t * 0.03, t * 0.01));

    // Second layer at different speed for depth.
    float fog_value2 = layered_noise(scrolled_uv * 4.0 + vec2(-t * 0.02, t * 0.015));

    // Combine layers.
    float combined = mix(fog_value, fog_value2, 0.5);

    // Apply density control.
    combined = smoothstep(0.5 - density * 0.5, 0.5 + density * 0.5, combined);

    // Soft edges: fade at the top and bottom.
    float edge_fade = smoothstep(0.0, 0.15, uv.y) * smoothstep(1.0, 0.85, uv.y);
    combined *= edge_fade;

    COLOR = vec4(color.rgb, combined * color.a);
}
