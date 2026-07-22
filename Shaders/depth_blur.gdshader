shader_type canvas_item;

// Blurs everything drawn so far (the far parallax layers) and pushes it toward
// a haze gray — cheap depth of field. Placed in canvas order between the far
// and near parallax layers; later items (near layers, gameplay) stay sharp.
uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform float blur_radius : hint_range(0.0, 8.0) = 3.0;
uniform vec4 haze_color : source_color = vec4(0.55, 0.57, 0.62, 1.0);
uniform float haze_amount : hint_range(0.0, 1.0) = 0.22;

void fragment() {
    vec2 r = SCREEN_PIXEL_SIZE * blur_radius;

    // 9-tap ring blur: center + 8 surrounding samples.
    vec3 c = texture(screen_texture, SCREEN_UV).rgb * 0.2;
    c += texture(screen_texture, SCREEN_UV + vec2( r.x,  0.0)).rgb * 0.1;
    c += texture(screen_texture, SCREEN_UV + vec2(-r.x,  0.0)).rgb * 0.1;
    c += texture(screen_texture, SCREEN_UV + vec2( 0.0,  r.y)).rgb * 0.1;
    c += texture(screen_texture, SCREEN_UV + vec2( 0.0, -r.y)).rgb * 0.1;
    c += texture(screen_texture, SCREEN_UV + vec2( r.x,  r.y) * 0.7).rgb * 0.1;
    c += texture(screen_texture, SCREEN_UV + vec2(-r.x,  r.y) * 0.7).rgb * 0.1;
    c += texture(screen_texture, SCREEN_UV + vec2( r.x, -r.y) * 0.7).rgb * 0.1;
    c += texture(screen_texture, SCREEN_UV + vec2(-r.x, -r.y) * 0.7).rgb * 0.1;

    c = mix(c, haze_color.rgb, haze_amount);
    COLOR = vec4(c, 1.0);
}
