#version 330 core

!include<"Includes/Blur.shader">

void main()
{
    FragColor = vec4(blur(vec2(0.0, 1.0)), 1.0);
}