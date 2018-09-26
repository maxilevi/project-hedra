#version 330 core

layout(location = 0) in vec3 InVertex;
layout(location = 1) in vec4 InColor;

out vec4 Color;

void main(){
	Color = InColor;
	gl_Position = gl_ModelViewProjectionMatrix * vec4(InVertex, 1.0);
}