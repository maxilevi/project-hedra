#version 330 core

in vec4 InColor;

layout(location = 0) out vec4 OutColor;

void main(){

	OutColor = vec4(InColor);

}