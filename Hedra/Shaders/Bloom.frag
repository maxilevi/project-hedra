#version 330 compatibility

uniform sampler2D Sampler;
uniform vec2 Resolution;
in vec2 TexCoords;
const float LumThresh = 0.65;
 
layout(location = 0) out vec4 Color; 

float luma(vec3 color);

uniform vec4 BotColor;
uniform vec4 TopColor;
uniform float Height;


void main(){
	vec4 val = texture(Sampler, TexCoords);
	float bright = luma(val.rgb) - 0.0;
	
	if(bright <= LumThresh) discard;
	
	
	Color = val * bright * .25;
	
}

float luma(vec3 color){
	return 0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b;
}
