#version 330 compatibility

in vec2 UVs;
uniform sampler2D Texture;
uniform float Opacity = 1.0;
uniform vec4 UniformColor;

layout(location = 0) out vec4 OutColor;

void main(){
	vec4 Color = texture(Texture, UVs);
	OutColor = vec4(UniformColor.rgb,Color.a * Opacity);
}