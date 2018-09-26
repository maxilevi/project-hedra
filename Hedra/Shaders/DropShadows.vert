#version 330 core
layout(location = 0)in vec2 InVertex;


out vec2 uv;
out float Visibility;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;

uniform vec3 PlayerPosition;
uniform vec3 Scale;
uniform vec3 Position;
uniform mat3 Rotation;


layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

void main(void){

	vec4 Vertex = vec4( (Rotation * (vec3(InVertex.y, 0.0, InVertex.x) * Scale) ) + Position, 1.0);
	gl_Position = gl_ModelViewProjectionMatrix * Vertex;
	uv = vec2((InVertex.y+1.0)/2.0 - .5, 1.0 - (InVertex.x+1.0)/2.0 - .5);
	
	pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - Vertex.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
}