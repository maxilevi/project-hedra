#version 330 core

out vec4 PassColor;

layout(location = 0)in vec3 InVertex;
layout(location = 1)in vec4 InColor;

void main(){

    gl_Position = _modelViewProjectionMatrix * vec4(InVertex, 1.0);
    PassColor = InColor;
}