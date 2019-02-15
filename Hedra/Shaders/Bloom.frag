#version 330 core

uniform sampler2D Sampler;
uniform float Modifier;
in vec2 TexCoords;
layout(location = 0) out vec4 Color; 

float luma(vec3 color);

void main(){
	vec4 val = texture(Sampler, TexCoords);
	float bright = luma(val.rgb) - 0.55 / Modifier;
	
	Color = val * bright;
	
}

float luma(vec3 color){
	return 0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b;
}
