#version 330 compatibility
layout(location = 0)in vec2 InVertex;

out vec2 UV;

uniform vec2 Scale;
uniform vec2 Position;
uniform mat3 Rotation = mat3(1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0);


void main(void){

	gl_Position = vec4( (Rotation * vec3(InVertex, 0.0) ).xy * Scale + Position, 0.0, 1.0);
	UV = vec2((InVertex.x+1.0)/2.0, 1.0 - (InVertex.y+1.0)/2.0);
}