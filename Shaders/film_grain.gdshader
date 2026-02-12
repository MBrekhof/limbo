shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform float grain_intensity : hint_range(0.0, 0.3) = 0.04;
uniform float grain_scale : hint_range(1.0, 10.0) = 3.0;

// Hash-based pseudo-random function.
float hash(vec2 p) {
	vec3 p3 = fract(vec3(p.xyx) * 0.1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return fract((p3.x + p3.y) * p3.z);
}

void fragment() {
	vec4 screen_color = texture(screen_texture, SCREEN_UV);

	// Procedural grayscale noise that shifts every frame.
	vec2 noise_uv = UV * grain_scale + vec2(TIME * 5.0, TIME * 3.7);
	float noise = hash(noise_uv);
	vec4 noise_color = vec4(vec3(noise), 1.0);

	COLOR = mix(screen_color, noise_color, grain_intensity);
}
