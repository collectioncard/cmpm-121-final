shader_type canvas_item;

uniform sampler2D tex1;
uniform sampler2D tex2;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec4 color1 = texture(tex1, FRAGCOORD.xy / SCREEN_PIXEL_SIZE);
	vec4 color2 = texture(tex2, FRAGCOORD.xy / SCREEN_PIXEL_SIZE);
	COLOR = mix(color1, color2, 0.3); // Adjust the weight here (0.3 means 70% tex1 and 30% tex2)
}

//void light() {
	// Called for every pixel for every light affecting the material.
	// Uncomment to replace the default light processing function with this one.
//}
