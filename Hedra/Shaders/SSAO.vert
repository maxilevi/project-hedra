#version 330 core

out vec2 TexCoords;

void main(){
	vec2 position = gl_Vertex.xy;
	gl_Position = vec4(position, 0.0, 1.0);
	
	TexCoords = vec2((position.x+1.0)/2.0, 1.0 - (position.y+1.0)/2.0);
	TexCoords = vec2(TexCoords.x, TexCoords.y);
}