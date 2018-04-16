#version 330 compatibility

layout(location = 0)in vec2 InVertex;

uniform mat4 TransMatrix;

out vec4 Vertex;

void main()
{
	gl_Position = vec4(InVertex, 0.0, 1.0); 
} 