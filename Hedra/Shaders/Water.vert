#version 330 core
!include<"Includes/Lighting.shader">
!include<"Includes/Highlights.shader">

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

layout(location = 0)in vec3 InVertex;
layout(location = 1)in vec4 InColor;
layout(location = 2)in vec3 InNormal;

out vec4 InPos;
out vec4 InNorm;
flat out vec4 Color;
out float Visibility;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;
out vec4 ClipSpace;
out vec2 textureCoords;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

uniform vec3 PlayerPosition;
uniform float Transparency = .7;
uniform vec3 Scale;
uniform vec3 Offset;
uniform vec3 BakedOffset;
uniform mat4 TransformationMatrix;

void main()
{
    vec4 v = vec4((InVertex + BakedOffset) * Scale + Offset, 1.0);

    pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - v.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(_modelViewMatrix) * vec4(0.0,0.0,0.0,1.0) ).xyz - v.xyz);

	v = TransformationMatrix * v;
	Color = diffuse(unitToLight, InNormal, max(LightColor, vec3(.1))) * InColor;
    Color = apply_highlights(Color, v.xyz);
 	Color.a = Transparency;
 	Color += vec4(diffuse(unitToLight, InNormal, calculate_lights(LightColor, v.xyz, 2.25)).xyz * InColor.xyz, 0.0);
	
 	gl_Position = _modelViewProjectionMatrix * v;
	textureCoords = v.xz * 0.01;
	
	InPos = v;
	ClipSpace = gl_Position;
	InNorm = vec4(InNormal, 1.0);
	
}