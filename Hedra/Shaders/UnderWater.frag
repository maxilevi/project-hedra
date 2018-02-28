#version 330 compatibility

in vec2 UV;
uniform float Time;
uniform sampler2D Texture;
uniform vec4 Multiplier = vec4(.7,.7,.7, 1.0);
uniform float Flipped;

layout(location = 0)out vec4 OutColor;

void main(void){
	vec4 Color = texture(Texture, UV);
	
	OutColor = Color * Multiplier;
}	