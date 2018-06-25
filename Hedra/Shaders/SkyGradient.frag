#version 330 compatibility
!include<"Includes/Sky.shader">

layout(location = 0)out vec4 OutColor; 
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

void main()
{   
    OutColor = sky_color();
	OutPosition = vec4(0.0, 0.0, 0.0, 0.0);
	OutNormal = vec4(0.0, 0.0, 0.0, 0.0);
}