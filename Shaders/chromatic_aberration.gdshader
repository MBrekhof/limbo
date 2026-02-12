shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform float aberration_strength : hint_range(0.0, 5.0) = 1.0;

void fragment() {
	// Direction from center and distance for edge-weighted offset.
	vec2 direction = SCREEN_UV - vec2(0.5, 0.5);
	float dist = length(direction);

	// Scale offset by distance from center so edges get stronger aberration.
	// The 0.002 base keeps the effect subtle and cinematic.
	vec2 offset = direction * dist * aberration_strength * 0.002;

	// Red channel: sample slightly outward from center.
	float r = texture(screen_texture, SCREEN_UV + offset).r;
	// Green channel: sample at original position.
	float g = texture(screen_texture, SCREEN_UV).g;
	// Blue channel: sample slightly inward from center.
	float b = texture(screen_texture, SCREEN_UV - offset).b;

	COLOR = vec4(r, g, b, 1.0);
}
