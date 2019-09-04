#version 330 core
!include<"Includes/GammaCorrection.shader">
!include<"Includes/Lighting.shader">

layout(location = 0) in vec3 InVertex;
layout(location = 1) in vec4 InColor;
layout(location = 2) in vec3 InNormal;

uniform mat4 ShadowMVP;
uniform float Time;
uniform float ShadowDistance;
const float ShadowTransition = 20.0;

out vec4 raw_color;
out vec4 Color;
out float Visibility;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;
out vec3 InPos;
out vec3 InNorm;
out vec4 Coords;
out vec3 LightDir;
out vec3 vertex_position;
out vec3 base_normal;
out vec3 base_vertex_position;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;
	float U_Height;
};

layout(std140) uniform ObjectAttributes {
	mat4 RotationMatrix;
	mat4 LocalRotationMatrix;
	mat4 TransformationMatrix;
	float Alpha;
	vec3 Scale;
	vec3 Position;
	vec3 RotationPoint;
	vec3 LocalRotationPoint;
	vec3 BeforeRotation;
	vec4 Tint;
	vec4 BaseTint;
	vec3 PlayerPosition;
	int IgnoreSSAO;
	vec4 DitherFogTextureShadows;
};

out float pass_alpha;
flat out ivec4 pass_ditherFogTextureShadows;
out vec4 pass_tint;
out vec4 pass_baseTint;
flat out int pass_ignoreSSAO;

vec3 TransformNormal(vec3 norm, mat4 invMat);

void main()
{
	pass_alpha = Alpha;
	pass_ditherFogTextureShadows = ivec4(DitherFogTextureShadows);
	pass_tint = Tint;
	pass_baseTint = BaseTint;
	pass_ignoreSSAO = IgnoreSSAO;
	
	vec4 linear_color = srgb_to_linear(InColor);
	pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;

	vec4 Vertex = vec4(InVertex * Scale, 1.0);

	Vertex += vec4(LocalRotationPoint,0.0);
	Vertex = LocalRotationMatrix * Vertex;
    Vertex -= vec4(LocalRotationPoint, 0.0);
	
	Vertex += vec4(BeforeRotation,0.0);
	Vertex += vec4(RotationPoint, 0.0);
	Vertex = RotationMatrix * Vertex;
	Vertex -= vec4(RotationPoint,0.0);

	Vertex = TransformationMatrix * Vertex;
	Vertex += vec4(Position, 0.0);
	
	gl_Position = _modelViewProjectionMatrix * Vertex;
	
	//Fog
	float DistanceToCamera = length(vec3(PlayerPosition - Vertex.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	
	vec3 SurfaceNormal = InNormal;
	SurfaceNormal = TransformNormal(SurfaceNormal, mat4(LocalRotationMatrix));
	SurfaceNormal = TransformNormal(SurfaceNormal, mat4(RotationMatrix));
    SurfaceNormal = TransformNormal(SurfaceNormal, TransformationMatrix);
	
	//Lighting
	vec3 unitNormal = normalize(SurfaceNormal);
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(_modelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - Vertex.xyz);

	vec3 FLightColor = calculate_lights(LightColor, Vertex.xyz);
	vec3 FullLight = clamp(FLightColor + LightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));

	Color = rim(linear_color.rgb, LightColor, unitToCamera, unitNormal) 
	+ (diffuse(unitToLight, unitNormal, LightColor) + diffuse(unitToLight, unitNormal, FLightColor)) * linear_color;
	Ambient = 0.25;

	InPos = Vertex.xyz;
	vertex_position = Vertex.xyz;
	base_vertex_position = InVertex.xyz;
	base_normal = unitNormal;
	
	InNorm = SurfaceNormal;
	raw_color = linear_color;
	
	if(DitherFogTextureShadows.w == int(1.0))
	{
		float ShadowDist = DistanceToCamera - (ShadowDistance - ShadowTransition);
		ShadowDist /= ShadowTransition;
		ShadowDist = clamp(1.0 - ShadowDist, 0.0, 1.0);
		Coords = ShadowMVP * vec4(Vertex.xyz, 1.0);
		Coords.w = ShadowDist;
	}
}

 vec3 TransformNormal(vec3 norm, mat4 invMat)
{
    return mat3(transpose(inverse(invMat))) * norm;
}