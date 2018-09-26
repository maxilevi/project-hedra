#version 330 core

out vec2 TexCoords;
layout(location = 0)in vec2 InVertex;

uniform vec2 Scale;
uniform vec2 Position;

void main(){
	gl_Position = vec4(InVertex, 0.0, 1.0);
	TexCoords = vec2((InVertex.x+1.0)/2.0,  1.0-(InVertex.y+1.0)/2.0);
}