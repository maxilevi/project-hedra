#version 330 core

layout(location = 0)in vec3 in_vertex;
out vec3 pass_uv;
uniform mat4 mvp;
uniform mat4 trans_matrix;

void main()
{
	pass_uv = in_vertex;
	gl_Position = mvp * trans_matrix * vec4(in_vertex, 1.0); 
} 