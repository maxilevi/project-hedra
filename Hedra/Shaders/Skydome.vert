#version 330 compatibility

uniform mat4 TransMatrix;

out vec4 Vertex;
out vec3 InNormal;

void main()
{
	Vertex = vec4(gl_Vertex.xyz, 1.0);
	InNormal = gl_Normal.xyz;
	gl_Position = Vertex; 
} 