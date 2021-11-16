#version 330 core

uniform sampler2D Cursor;

in vec2 UVs;

layout(location = 0) out vec4 Color;

void main(){
    Color = texture(Cursor, UVs);
}