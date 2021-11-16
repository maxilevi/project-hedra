#version 330 core
layout(location = 0)in vec2 InVertex;

out vec2 UV;

void main(void){

    gl_Position = vec4(InVertex, 0.0, 1.0);
    UV = vec2((InVertex.x+1.0)/2.0, 1.0 - (InVertex.y+1.0)/2.0);
}