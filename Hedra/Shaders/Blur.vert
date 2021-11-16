#version 330 core
layout(location = 0)in vec2 Position;

out vec2 TexCoords;

void main()
{
    gl_Position = vec4(Position, 0.0, 1.0);
    TexCoords = vec2((Position.x + 1.0) / 2.0, 1.0 - (Position.y + 1.0) / 2.0);
}