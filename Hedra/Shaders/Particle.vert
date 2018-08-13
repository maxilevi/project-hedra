#version 330 compatibility
layout(location = 0) in vec3 v_pos;
layout(location = 1) in vec3 v_Normal;
layout(location = 2) in vec4 v_Color;
layout(location = 3) in vec4 Col1;
layout(location = 4) in vec4 Col2;
layout(location = 5) in vec4 Col3;
layout(location = 6) in vec4 Col4;
 
out vec4 Color; 
out float Visibility;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;

uniform vec3 PlayerPosition;
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
};

uniform PointLight Lights[8];
uniform vec3 LightColor = vec3(1.0, 1.0, 1.0);
uniform vec3 LightPosition = vec3(-500.0, 800.0, 0.0);

const float Damper = 32.0;
const float Reflectivity = 0.5;

vec3 DiffuseModel(vec3 unitToLight, vec3 unitNormal, vec3 LColor){	
	float Brightness = max(dot(unitNormal, unitToLight), 0.2);
	return (Brightness * LColor );
}
 
 void main(){
 	mat4 TransMatrix = mat4(Col1, Col2, Col3, Col4);
 	vec4 v = vec4(v_pos, 1.0); 
 	v =  v * TransMatrix;
	gl_Position = gl_ModelViewProjectionMatrix * v;
	
	pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - v.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	
	
	//Lighting
	vec3 unitNormal = normalize(v_Normal);
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(gl_ModelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - v.xyz);

	vec3 FLightColor = LightColor;
	for(int i = 0; i < 8; i++){
		float dist = length(Lights[i].Position - v.xyz);
		vec3 toLightPoint = normalize(Lights[i].Position);
		float att = 1.0 / (1.0 + 0.35 * dist * dist);
		att *= 20.0;
		att = min(att, 1.0);
		
		FLightColor += Lights[i].Color * att; 
	}
	FLightColor = clamp(FLightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));
	
	vec3 Diffuse = DiffuseModel(unitToLight, unitNormal, FLightColor) + DiffuseModel(vec3(0.0, 0.0, 1.0), unitNormal, FLightColor) * .5
					+ DiffuseModel(vec3(0.0, 0.0, -1.0), unitNormal, FLightColor) * .5
				   + DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal, FLightColor) * .5 + DiffuseModel(vec3(-1.0, 0.0, 0.0), unitNormal, FLightColor) * .5
				   + DiffuseModel(vec3(0.0, -1.0, 0.0), unitNormal, FLightColor) * .5 + DiffuseModel(vec3(0.0, 1.0, 0.0), unitNormal, FLightColor) * .5;
	
	Color = (vec4(Diffuse, 1.0) * v_Color);
 }

