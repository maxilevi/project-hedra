#version 330 compatibility

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

uniform vec3 LightPosition = vec3(-500.0, 800.0, 0.0);
uniform vec3 LightColor = vec3(1.0, 1.0, 1.0);
uniform PointLight Lights[12];

const vec3 RimColor = vec3(0.2, 0.2, 0.2);
const float Damper = 32.0;
const float Reflectivity = 0.25;

vec3 DiffuseModel(vec3 unitToLight, vec3 unitNormal, vec3 LColor){	
	float Brightness = max(dot(unitNormal, unitToLight), 0.125);
	if(UseFog)
		return (Brightness * LColor );
	else
		return (Brightness * vec3(1.0, 1.0, 1.0) );
}

void main(void){
	
	pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;
	
	vec4 totalLocalPos = vec4(0.0, 0.0, 0.0, 0.0);
	vec4 totalNormal = vec4(0.0, 0.0, 0.0, 0.0);
	mat4 identity = mat4(
	1.0, 0.0, 0.0, 0.0,
	0.0, 1.0, 0.0, 0.0,
	0.0, 0.0, 1.0, 0.0,
	0.0, 0.0, 0.0, 1.0);
	
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
	vec3 unitToCamera = normalize((inverse(gl_ModelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - pass_position.xyz);

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

	//Specular
	vec3 ReflectedDir = reflect(-unitToLight, unitNormal);
	float SpecBrightness = max(dot(ReflectedDir, unitToCamera), 0.0);
	float Damp = pow(SpecBrightness, Damper) * Reflectivity;
	vec4 Specular = vec4(Damp*FLightColor,1.0);
	
	//Rim Lighting
	float rim = 1.0 - max(dot(unitToCamera, unitNormal), 0.0);
	rim = smoothstep(0.6, 1.0, rim);
	vec3 finalRim = in_color.rgb * 0.4 * rim * max(FLightColor, vec3(.4,.4,.4));

	//Diffuse Lighting
	vec3 FullLightColor = clamp(LightColor + FLightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));
	vec3 Diffuse = DiffuseModel(vec3(0.0, 0.0, 1.0), unitNormal, FullLightColor) * .35 + DiffuseModel(vec3(0.0, 0.0, -1.0), unitNormal, FullLightColor) * .35
				   + DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal, FullLightColor) * .35 + DiffuseModel(vec3(-1.0, 0.0, 0.0), unitNormal, FullLightColor) * .35 
				   + DiffuseModel(unitToLight, unitNormal, LightColor) * .7;

	vec4 final_color = vec4(finalRim,0.0) + (vec4(Diffuse,1.0) * vec4(in_color,1.0)) + Specular;

	vec3 lightDiffuse = max(DiffuseModel(unitToLight, unitNormal, FLightColor), vec3(.5, .5, .5)) * .7;

	pass_lightDiffuse = vec4(lightDiffuse,1.0) * vec4(in_color,1.0);
	pass_color = vec4(final_color.xyz, Alpha);
	
	//Shadows Stuff

	float ShadowDist = DistanceToCamera - (ShadowDistance - ShadowTransition);
	ShadowDist /= ShadowTransition;
	ShadowDist = clamp(1.0 - ShadowDist, 0.0, 1.0);
	pass_coords = ShadowMVP * vec4(totalLocalPos.xyz,1.0);

	
	gl_Position = projectionViewMatrix * totalLocalPos;

}