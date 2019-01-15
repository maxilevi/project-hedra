#version 330 core

layout(location = 0)in vec3 in_vertex;

uniform vec3 Scale;
uniform vec3 Position;

void main() {
    gl_Position = _modelViewProjectionMatrix * vec4(in_vertex * Scale + Position, 1.0);
}