#version 330 compatibility
layout(location = 0)in vec2 InVertex;

out vec2 UV;
uniform vec3 Direction;
uniform mat4 TransMatrix;
uniform vec3 Position;

void main(void){

	gl_Position = vec4(InVertex, 0.0, 1.0) * TransMatrix + vec4(Position.xy, 0.0, 0.0);
	UV = vec2((InVertex.x+1.0)/2.0, 1.0 - (InVertex.y+1.0)/2.0);
}