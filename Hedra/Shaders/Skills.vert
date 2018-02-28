#version 330 compatibility

layout(location = 0)in vec2 InVertex;

out vec2 UVs;
out float YCoord;

uniform vec2 MaskScale;
uniform vec2 MaskPosition;
uniform vec2 Scale;
uniform vec2 Position;

void main(){
	
	gl_Position = vec4(InVertex * Scale + Position, 0.0, 1.0);
	UVs = InVertex * 0.5 + 0.5;
	YCoord = InVertex.y * 0.5 + 1.5;
}