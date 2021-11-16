#version 330 core

out vec4 PassColor;

layout(location = 0)in vec2 InVertex;
layout(location = 1)in vec4 InColor;

void main(){

    gl_Position = vec4(InVertex, 0.0, 1.0);
    PassColor = InColor;
}