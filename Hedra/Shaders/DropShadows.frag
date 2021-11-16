#version 330 core

!include<"Includes/Sky.shader">

in vec2 uv;
in float Visibility;

uniform float Opacity;

layout(location = 0)out vec4 fragment_color;
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

void main(void)
{
    float distance = clamp(sqrt(uv.x * uv.x + uv.y * uv.y), 0.0, 1.0);

    vec4 color = vec4(0.0, 0.0, 0.0, min(max(1.0 - sqrt(distance + .5), 0.0) * 3.0, 0.4) -.4 * (1.0-Opacity));
    if (color.a < 0.05) discard;

    fragment_color = mix(sky_color(), color, Visibility);

    OutPosition = vec4(0.0, 0.0, 0.0, 0.0);
    OutNormal = vec4(0.0, 0.0, 0.0, 0.0);
}