#version 330 compatibility
layout(location = 0) in vec3 InVertex;
layout(location = 1) in vec3 InNormal;
layout(location = 2) in vec4 InColor;
layout(location = 3) in vec4 Col1;
layout(location = 4) in vec4 Col2;
layout(location = 5) in vec4 Col3;
layout(location = 6) in vec4 Col4;
 
out vec4 Color;
out float Visibility;
out float Height;
out vec4 BotColor;
out vec4 TopColor;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

uniform vec3 PlayerPosition;
uniform bool HasWind;
uniform vec3 HighestPoint;
uniform vec3 LowestPoint;
uniform float Time;
uniform vec3 LightPosition = vec3(-500.0, 1000.0, 0.0);
uniform vec3 LightColor = vec3(0.0, 0.0, 0.0);

const float Damper = 1.0;
const float Reflectivity = 0.02;

vec3 DiffuseModel(vec3 unitToLight, vec3 unitNormal){	
	float Brightness = max(dot(unitNormal, unitToLight) ,0.2);
	return (Brightness * mix(LightColor, vec3(0.0, 0.0, 0.0), .9));
}
 
 void main(){
 	mat4 TransMatrix = mat4(Col1, Col2, Col3, Col4);
 	Color = InColor;
 	
 	vec4 Vertex = vec4(InVertex,1.0);
 	if(HasWind){
		float Shade = dot(Vertex.xyz - LowestPoint, vec3(0.0, 1.0, 0.0)) / dot(HighestPoint - LowestPoint, vec3(0.0, 1.0, 0.0));
		Vertex.x += (cos(Time) +0.8) * .5 * 0.2 * Shade;
 	}
 	Vertex = Vertex * TransMatrix;
	gl_Position = gl_ModelViewProjectionMatrix * Vertex;
	
	Height = U_Height;
	BotColor = U_BotColor;
	TopColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - Vertex.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	
	//Lighting
	vec3 unitNormal = normalize(InNormal);
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(gl_ModelViewMatrix) * vec4(0.0,0.0,0.0,1.0) ).xyz - Vertex.xyz);

	//Specular
	vec3 ReflectedDir = reflect(-unitToLight, unitNormal);
	float SpecBrightness = max(dot(ReflectedDir, unitToCamera), 0.0);
	float Damp = pow(SpecBrightness, Damper) * Reflectivity;
	vec4 Specular = vec4(Damp*LightColor,1.0);
	
	//Rim Lighting
	float rim = 1.0 - max(dot(unitToCamera, unitNormal), 0.0);
	rim = smoothstep(0.6, 1.0, rim);
	vec3 finalRim = InColor.rgb * 0.1 * rim;

	//Diffuse Lighting
	vec4 Ambient = InColor * 0.0;
	vec3 Diffuse = DiffuseModel(vec3(0.0, 0.0, 1.0), unitNormal) * .25 + DiffuseModel(vec3(0.0, 0.0, -1.0), unitNormal) * .25
				   + DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal) * .25 + DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal) * .25 + DiffuseModel(unitToLight, unitNormal) * .95;
	
	Color = (Ambient + vec4(finalRim, 0.0) + (vec4(Diffuse,1.0) * InColor) + Specular);
 }

