#version 330 core
!include<"Includes/Sky.shader">

in vec4 PassColor;
in float Visibility;

layout(location = 0)out vec4 OutColor;
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

void main()
{
    if (Visibility < 0.005)
    {
        discard;
    }
    OutColor = mix(sky_color(), PassColor, Visibility);
    OutPosition = vec4(0.0);
    OutNormal = vec4(0.0);
}