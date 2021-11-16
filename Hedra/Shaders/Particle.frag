#version 330 core
!include<"Includes/Sky.shader">

smooth in vec4 Color;
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
    vec4 NewColor = mix(sky_color(), Color, Visibility);
    OutColor = NewColor;
    OutPosition = vec4(0.0, 0.0, 0.0, 0.0);
    OutNormal = vec4(0.0, 0.0, 0.0, 0.0);
}
