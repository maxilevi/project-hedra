#version 330 core

in vec4 PassColor;

layout(location = 0) out vec4 OutColor;

void main()
{
	OutColor = PassColor;
}