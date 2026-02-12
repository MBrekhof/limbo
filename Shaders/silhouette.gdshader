shader_type canvas_item;

uniform float edge_glow_width : hint_range(0.0, 5.0) = 1.0;
uniform float edge_glow_brightness : hint_range(0.0, 0.3) = 0.08;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);

    // Sample neighboring pixels for edge detection using built-in pixel size.
    float px = TEXTURE_PIXEL_SIZE.x;
    float py = TEXTURE_PIXEL_SIZE.y;

    float alpha_up = texture(TEXTURE, UV + vec2(0.0, -py * edge_glow_width)).a;
    float alpha_down = texture(TEXTURE, UV + vec2(0.0, py * edge_glow_width)).a;
    float alpha_left = texture(TEXTURE, UV + vec2(-px * edge_glow_width, 0.0)).a;
    float alpha_right = texture(TEXTURE, UV + vec2(px * edge_glow_width, 0.0)).a;

    // Edge factor: difference between center alpha and average neighbor alpha.
    float neighbor_avg = (alpha_up + alpha_down + alpha_left + alpha_right) / 4.0;
    float edge = abs(tex.a - neighbor_avg);

    // Near-black with subtle gray glow at edges.
    float brightness = edge * edge_glow_brightness;
    COLOR = vec4(vec3(brightness), tex.a);
}
