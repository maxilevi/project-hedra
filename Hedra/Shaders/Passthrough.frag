#version 330 core

layout(location = 0)out vec4 OutColor;
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

in vec4 pass_color;

void main()
{
    OutColor = pass_color;
    OutPosition = vec4(0.0);
    OutNormal = vec4(0.0);
}