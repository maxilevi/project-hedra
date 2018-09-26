#version 330 core
!include<"Includes/Lighting.shader">

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
out float Movement;
out vec4 ClipSpace;

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

uniform PointLight Lights[12];

uniform vec3 PlayerPosition;
uniform float WaveMovement;
uniform float Transparency = .7;
uniform vec3 Scale;
uniform vec3 Offset;
uniform vec3 BakedOffset;
uniform mat4 TransformationMatrix;

float GetY(float x, float z);
float GetOffset(float x, float z, float val1, float val2);
vec3 Cross(vec3 v1, vec3 v2);

vec2 Unpack(float inp, int prec)
{
    vec2 outp = vec2(0.0, 0.0);

    outp.y = mod(inp, prec);
    outp.x = floor(inp / prec);

    return outp / (prec - 1.0);
}

void main()
{
	Movement = WaveMovement;
    vec4 v = vec4((InVertex + BakedOffset) * Scale + Offset, 1.0);
    v.y = GetY(v.x, v.z) * InNormal.z * .6 * Scale.y + (InVertex.y + BakedOffset.y) * Scale.y + Offset.y;
    
    vec2 Unpacked1 = Unpack(InNormal.x, int(4096.0));
    vec2 Unpacked2 = Unpack(InNormal.y, int(4096.0));
    
    vec3 V0 = vec3(Unpacked1.x, 0.0, Unpacked1.y);
    vec3 V1 = vec3(Unpacked2.x, 0.0, Unpacked2.y);
    
    V0.x = (V0.x + InVertex.x + BakedOffset.x) * Scale.x + Offset.x;
    V0.z = (V0.z + InVertex.z + BakedOffset.z) * Scale.z + Offset.z;
    V1.x = (V1.x + InVertex.x + BakedOffset.x) * Scale.x + Offset.x;
    V1.z = (V1.z + InVertex.z + BakedOffset.z) * Scale.z + Offset.z;
    V0.y = (V0.y + InVertex.y + BakedOffset.y) * Scale.y + Offset.y;
    V1.y = (V1.y + InVertex.y + BakedOffset.y) * Scale.y + Offset.y;
    
    V0.y = GetY(V0.x, V0.z) * 0.4 * Scale.y + (InVertex.y+BakedOffset.y) * Scale.y + Offset.y;
    V1.y = GetY(V1.x, V1.z) * 0.4 * Scale.y + (InVertex.y+BakedOffset.y) * Scale.y + Offset.y;
    
    vec3 Normal = normalize(Cross(v.xyz - V0, V1 - v.xyz));
 	
    pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - v.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	
	//Lighting
	vec3 unitNormal = Normal;
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(_modelViewMatrix) * vec4(0.0,0.0,0.0,1.0) ).xyz - v.xyz);

	vec3 FullLightColor = LightColor;
	for(int i = 0; i < 12; i++){
		float dist = length(Lights[i].Position - v.xyz);
		vec3 toLightPoint = normalize(Lights[i].Position);
		float att = 1.0 / (1.0 + .35*dist*dist);
		att *= Lights[i].Radius;
		att = min(att, 1.0);
		
		FullLightColor += Lights[i].Color * att; 
	}
	FullLightColor = clamp(FullLightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));

	Ambient = 0.75;
	Color = ( diffuse(unitToLight, unitNormal, max(FullLightColor, vec3(.55, .55, .55))) * 0.8 + vec4(0.5, 0.5, 0.5, 0.0) * .0) * InColor 
	+ specular(unitToLight, unitNormal, unitToCamera, FullLightColor);

 	Color.a = Transparency;
 	
	v = TransformationMatrix * v;
 	gl_Position = _modelViewProjectionMatrix * v;

	InPos = v;
	ClipSpace = gl_Position;
	InNorm = vec4(Normal,1.0);
	
}

float mod2(float x, float y){
	return x - y * floor(x/y);
}

const float waveLength = 1.75;
const float waveTime = 0.5;
const float PI = 3.14159265;

float GetOffset(float x, float z, float val1, float val2){

	float rX = ((mod(x+z*x*val1, waveLength)/waveLength)+Movement*0.2) * 2.0 * PI;
	float rZ = ( (mod( val2 * (z*x + x*z), waveLength) / waveLength) +Movement*0.2 * 2.0) * 2.0 * PI;
	
	return 1.4 * 0.5 * (sin(rX) + sin(rZ));
}

float GetY(float x, float z){
    return GetOffset(x,z, 0.1, 0.3) + 0.0;
}

vec3 Cross(vec3 v1, vec3 v2){
	return vec3(v1.y * v2.z - v1.z * v2.y,
		     v1.z * v2.x - v1.x * v2.z,
		     v1.x * v2.y - v1.y * v2.x);
}