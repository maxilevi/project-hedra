#version 330 core

!include<"Includes/GammaCorrection.shader">
!include<"Includes/Lighting.shader">

const int MAX_JOINTS = 50;//max joints allowed in a skeleton
const int MAX_WEIGHTS = 3;//max number of joints that can affect a vertex

layout(location = 0)in vec3 in_position;
layout(location = 1)in vec3 in_color;
layout(location = 2)in vec3 in_normal;
layout(location = 3)in vec3 in_jointIndices;
layout(location = 4)in vec3 in_weights;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

out vec4 pass_color;
out vec3 pass_normal;
out float pass_visibility;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;
out vec3 pass_position;
out vec4 pass_coords;
out vec3 pass_lightDir;
out vec4 pass_lightDiffuse;

const float ShadowTransition = 20.0;

uniform float ShadowDistance;
uniform bool UseFog = false;
uniform vec3 PlayerPosition;
uniform mat4 jointTransforms[MAX_JOINTS];
uniform mat4 projectionViewMatrix;
uniform mat4 ShadowMVP;
uniform float Alpha;

struct PointLight
{
    vec3 Position;
    vec3 Color;
    float Radius;
};

uniform PointLight Lights[12];

void main(void)
{
	vec3 linear_color = srgb_to_linear(in_color);
	pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;
	
	vec4 totalLocalPos = vec4(0.0, 0.0, 0.0, 0.0);
	vec4 totalNormal = vec4(0.0, 0.0, 0.0, 0.0);
	mat4 identity = mat4(
        1.0, 0.0, 0.0, 0.0,
        0.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 1.0, 0.0,
        0.0, 0.0, 0.0, 1.0
	);
	
	float sum = 0.0;
	for(int i=0;i<MAX_WEIGHTS;i++){
		mat4 jointTransform = jointTransforms[ int(in_jointIndices[i]) ];
		vec4 posePosition = jointTransform * vec4(in_position, 1.0);
		totalLocalPos += posePosition * in_weights[i];
		sum += in_weights[i];
		
		vec4 worldNormal = jointTransform * vec4(in_normal, 0.0);
		totalNormal += worldNormal * in_weights[i];
	}
	 
	float DistanceToCamera = length(vec3(PlayerPosition - totalLocalPos.xyz).xz);
	pass_visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);

	pass_position = totalLocalPos.xyz;
	pass_normal = totalNormal.xyz; 
	
	vec3 unitNormal = normalize(pass_normal.xyz);
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(_modelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - pass_position.xyz);

	vec3 FLightColor = vec3(0.0, 0.0, 0.0);
	for(int i = 0; i < 12; i++){
		float dist = length(Lights[i].Position - pass_position.xyz );
		vec3 toLightPoint = normalize(Lights[i].Position);
		float att = 1.0 / (1.0 + .35*dist*dist);
		att *= Lights[i].Radius;
		att = min(att, 1.0);
		
		FLightColor += Lights[i].Color * att; 
	}
	FLightColor = clamp(FLightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));

	vec4 Specular = specular(unitToLight, unitNormal, unitToCamera, LightColor);
	vec4 Rim = rim(linear_color, LightColor, unitToCamera, unitNormal);

	//Diffuse Lighting
	vec3 FullLightColor = clamp(LightColor + FLightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));
	vec4 Diffuse = diffuse(unitToLight, unitNormal, LightColor * 1.15);
	vec4 final_color = Rim + Diffuse * vec4(linear_color,1.0) + Specular;
	vec3 lightDiffuse = diffuse(unitToLight, unitNormal, FLightColor).rgb;

	pass_lightDiffuse = vec4(lightDiffuse, 0.0) * vec4(linear_color, 0.0);
	pass_color = vec4(final_color.xyz, Alpha);
	
	//Shadows Stuff

	float ShadowDist = DistanceToCamera - (ShadowDistance - ShadowTransition);
	ShadowDist /= ShadowTransition;
	ShadowDist = clamp(1.0 - ShadowDist, 0.0, 1.0);
	pass_coords = ShadowMVP * vec4(totalLocalPos.xyz,1.0);

	gl_Position = projectionViewMatrix * totalLocalPos;

}