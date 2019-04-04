#version 330 core

layout(location = 0)in vec2 InVertex;

out vec2 TexCoords;

void main()
{
	vec2 position = InVertex.xy;
	gl_Position = vec4(position, 0.0, 1.0);
	
	TexCoords = vec2((position.x+1.0)/2.0, 1.0 - (position.y+1.0)/2.0);
	TexCoords = vec2(TexCoords.x, TexCoords.y);
}