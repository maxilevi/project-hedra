#version 330 core

layout(location = 0)in vec3 in_vertex;

void main() {
    gl_Position = _modelViewProjectionMatrix * vec4(in_vertex, 1.0);
}