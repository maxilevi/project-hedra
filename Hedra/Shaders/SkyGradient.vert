#version 330 core

layout(location = 0)in vec2 InVertex;

uniform mat4 TransMatrix;
uniform vec4 topColor;
uniform vec4 botColor;
uniform float height;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;

void main()
{
    pass_height = height;
    pass_botColor = botColor;
    pass_topColor = topColor;
    gl_Position = vec4(InVertex, 0.0, 1.0);
} 