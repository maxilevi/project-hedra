#version 330 core

layout(location = 0)in vec3 in_vertex;
layout(location = 1)in vec4 in_color;

out vec4 pass_color;

void main() {
    pass_color = in_color;
    gl_Position = _modelViewProjectionMatrix * vec4(in_vertex, 1.0);
}