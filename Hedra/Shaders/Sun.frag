#version 330 compatibility

in vec2 UV;
uniform sampler2D Texture;

layout(location = 0) out vec4 OutColor;

void main(void){
	OutColor = texture(Texture, UV) * 1.0;
}	