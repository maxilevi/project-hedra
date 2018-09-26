#version 330 core

layout(location = 0)in vec2 InVertex;
out vec2 UVs;

uniform vec2 Position;
uniform vec2 Scale;

void main(){
	gl_Position = vec4(InVertex * Scale + Position, 0.0, 1.0);
	UVs = vec2((InVertex.x+1.0)/2.0, 1.0 - (InVertex.y+1.0)/2.0);
}