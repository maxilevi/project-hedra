#version 330 core

layout(location = 0)in vec3 in_vertex;
layout(location = 0)in vec2 in_uv;
out vec2 pass_uv;
uniform mat4 mvp;
uniform mat4 trans_matrix;
void main()
{
	pass_uv = in_uv;
	gl_Position = mvp * vec4(trans_matrix * in_vertex, 1.0); 
} 