#version 330 core

layout(location = 0)in vec2 InVertex;
out vec4 InColor;

void main(){

	gl_Position = vec4(InVertex, 0.0, 1.0);
	InColor = gl_Color;
}