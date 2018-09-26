#version 330 core
!include<"Includes/Sky.shader">

in vec4 Color;
in float Visibility;
layout(location = 0)out vec4 OutColor;

void main()
{
	if(Visibility < 0.005)
	{
		discard;
	}
	vec4 NewColor = mix(sky_color(), Color, Visibility);	
	OutColor = NewColor;
}
