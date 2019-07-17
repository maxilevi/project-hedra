#version 330 core

layout(location = 0)out vec4 OutColor;
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

void main()
{
    OutColor = vec4(1.0, 1.0, 1.0, 1.0);
    OutPosition = vec4(0.0);
    OutNormal = vec4(0.0);
}