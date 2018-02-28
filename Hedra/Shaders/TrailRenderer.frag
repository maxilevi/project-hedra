#version 330 compatibility

layout(location = 0)out vec4 OutColor; 
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

in vec4 Color;

void main(){
	OutColor = vec4(Color);
	OutPosition = vec4(0.0, 0.0, 0.0, gl_FragCoord.z);
	OutNormal = vec4(0.0, 0.0, 0.0, 1.0);
}