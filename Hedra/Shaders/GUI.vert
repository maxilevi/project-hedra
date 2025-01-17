#version 330 core
layout(location = 0)in vec2 InVertex;

out vec2 UV;

uniform vec2 Scale;
uniform vec2 Position;
uniform mat2 Rotation = mat2(1.0, 0.0, 0.0, 1.0);


void main(void)
{

    gl_Position = vec4(Rotation * InVertex * Scale + Position, 0.0, 1.0);
    UV = vec2((InVertex.x+1.0)/2.0, 1.0 - (InVertex.y+1.0)/2.0);
}