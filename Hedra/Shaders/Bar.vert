#version 330 core

layout(location = 0)in vec2 InVertex;

uniform vec2 Scale;
uniform vec2 Position;

out vec2 FixedPos;
out float Gradient;
out vec2 InScale;
out vec2 UV;

void main(){
    InScale = Scale;
    gl_Position = vec4(InVertex * Scale + Position, 0.0, 1.0);
    UV = vec2((InVertex.x+1.0)/2.0, 1.0 - (InVertex.y+1.0)/2.0);
    FixedPos = InVertex * Scale;
    Gradient = FixedPos.y;
}
