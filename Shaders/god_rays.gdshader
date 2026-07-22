shader_type canvas_item;
render_mode blend_add;

uniform float intensity : hint_range(0.0, 1.0) = 0.35;
uniform float ray_angle : hint_range(-1.0, 1.0) = 0.35;
uniform float ray_scale : hint_range(2.0, 40.0) = 14.0;
uniform float speed : hint_range(0.0, 0.5) = 0.05;
uniform vec4 color : source_color = vec4(0.85, 0.9, 1.0, 1.0);

float hash(float n) {
    return fract(sin(n * 127.1) * 43758.5453);
}

float value_noise(float x) {
    float i = floor(x);
    float f = fract(x);
    float u = f * f * (3.0 - 2.0 * f);
    return mix(hash(i), hash(i + 1.0), u);
}

void fragment() {
    // Skew X by Y so the shafts run diagonally, like light through a canopy.
    float x = UV.x + UV.y * ray_angle;

    // Two noise octaves drifting at different speeds = shafts that slowly
    // brighten, fade, and swap places.
    float n = value_noise(x * ray_scale + TIME * speed) * 0.6
            + value_noise(x * ray_scale * 2.7 - TIME * speed * 1.4) * 0.4;

    // Only the brightest bands become visible shafts.
    float shaft = smoothstep(0.55, 0.95, n);

    // Rays pour from the top and dissolve toward the ground.
    float fade = pow(1.0 - UV.y, 1.8);

    COLOR = vec4(color.rgb, shaft * fade * intensity * color.a);
}
