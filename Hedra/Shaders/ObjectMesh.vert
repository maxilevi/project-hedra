#version 330 core
!include<"Includes/Lighting.shader">
!include<"Includes/GammaCorrection.shader">

layout(location = 0) in vec3 InVertex;
layout(location = 1) in vec4 InColor;
layout(location = 2) in vec3 InNormal;

uniform vec3 Point;
uniform vec3 LocalRotationPoint;
uniform mat4 TransMatrix;
uniform mat4 LocalRotation;
uniform vec3 LocalPosition;
uniform vec3 BeforeLocalRotation;
uniform vec3 AnimationPosition;
uniform mat4 AnimationRotation;
uniform vec3 AnimationRotationPoint;
uniform vec3 TransPos;
uniform float Time;
uniform vec3 Scale;
uniform vec3 BakedPosition;
uniform mat4 ShadowMVP;
uniform float ShadowDistance;
uniform bool UseShadows;
uniform mat4 Matrix;
uniform bool Outline;
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
out vec3 point_diffuse;
out vec3 base_vertex_position;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

struct PointLight
{
    vec3 Position;
    vec3 Color;
    float Radius;
};

uniform PointLight Lights[8];

vec3 TransformNormal(vec3 norm, mat4 invMat);

uniform vec3 PlayerPosition;
uniform bool UseFog = true;

void main(){
	vec4 linear_color = srgb_to_linear(InColor);
	pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;

	vec4 Vertex = vec4((InVertex + BakedPosition) * Scale - BakedPosition, 1.0);

	Vertex += vec4(AnimationRotationPoint, 0.0);
	Vertex = AnimationRotation * Vertex;
	Vertex -= vec4(AnimationRotationPoint, 0.0);
	
	Vertex += vec4(BeforeLocalRotation,0.0);
	Vertex += vec4(LocalRotationPoint, 0.0);
	Vertex = LocalRotation * Vertex;
	Vertex -= vec4(LocalRotationPoint,0.0);
	
	Vertex += vec4(AnimationPosition, 0.0);

	Vertex += vec4(Point,0.0);
	Vertex = TransMatrix * Vertex;
	Vertex = Matrix * Vertex;
	Vertex += vec4(TransPos, 0.0);
	Vertex -= vec4(Point, 0.0);	

	Vertex += vec4(LocalPosition,0.0);
	
	gl_Position = _modelViewProjectionMatrix * Vertex;
	
	//Fog
	float DistanceToCamera = length(vec3(PlayerPosition - Vertex.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	
	vec3 SurfaceNormal = InNormal;
	SurfaceNormal = TransformNormal(SurfaceNormal, AnimationRotation);	
	SurfaceNormal = TransformNormal(SurfaceNormal, LocalRotation);
    SurfaceNormal = TransformNormal(SurfaceNormal, TransMatrix);
    SurfaceNormal = TransformNormal(SurfaceNormal, Matrix);

	
	//Lighting
	vec3 unitNormal = normalize(SurfaceNormal);
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(_modelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - Vertex.xyz);

	vec3 FLightColor = vec3(0.0, 0.0, 0.0);
	float average_color = (LightColor.r + LightColor.g + LightColor.b) / 3.0; 
	for(int i = int(0.0); i < 8.0; i++)
	{
		float dist = length(Lights[i].Position.xyz - Vertex.xyz);
        float att = 1.0 / (1.0 + dist * dist);
        att *= Lights[i].Radius * .5;
        att = min(att, 1.0);
        
        FLightColor += Lights[i].Color * att * .5 * (1.0 - average_color); 
	}
	FLightColor =  clamp(FLightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));
	vec3 FullLight = clamp(FLightColor + LightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));

	Ambient = 0.25;
	Color = rim(linear_color.rgb, LightColor, unitToCamera, unitNormal) 
	+ diffuse(unitToLight, unitNormal, LightColor) * linear_color
	+ specular(unitToLight, unitNormal, unitToCamera, LightColor);

	point_diffuse = diffuse(unitToLight, unitNormal, FLightColor).rgb;

	InPos = Vertex.xyz;
	vertex_position = Vertex.xyz;
	base_vertex_position = InVertex.xyz;
	base_normal = unitNormal;
	
	InNorm = SurfaceNormal;
	raw_color = linear_color;
	
	//Shadows Stuff
	if(UseShadows)
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