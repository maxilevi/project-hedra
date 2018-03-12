#version 330 compatibility

layout(location = 0)in vec3 InVertex;
layout(location = 1)in vec4 InColor;
layout(location = 2)in vec3 InNormal;

out vec4 InPos;
smooth out vec4 InNorm;
out vec4 Color;
out float Visibility;
out float Height;
out vec4 BotColor;
out vec4 TopColor;
out vec4 Coords;
out vec3 LightDir;
out float CastShadows;
out float Config;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

uniform vec3 PlayerPosition;
uniform vec3 LightPosition = vec3(-500.0, 800.0, 0.0);
uniform vec3 LightColor = vec3(0.0, 0.0, 0.0);
uniform float Time;
uniform float Fancy = 1.0;
uniform mat4 ShadowMVP;
uniform float ShadowDistance;
uniform float UseShadows = 3.0;

const float ShadowTransition = 10.0;

vec2 Unpack(float inp, int prec)
{
    vec2 outp = vec2(0.0, 0.0);

    outp.y = mod(inp, prec);
    outp.x = floor(inp / prec);

    return outp / (prec - 1.0);
}

float when_eq(float x, float y) {//equal
  return 1.0 - abs(sign(x - y));
}

float when_neq(float x, float y) {//not equal
  return abs(sign(x - y));
}

float when_gt(float x, float y) {//greater
  return max(sign(x - y), 0.0);
}

float when_lt(float x, float y) {//smaller
  return max(sign(y - x), 0.0);
}

float when_ge(float x, float y) {
  return 1.0 - when_lt(x, y);
}

float when_le(float x, float y) {
  return 1.0 - when_gt(x, y);
}

vec4 when_neq(vec4 x, vec4 y) {//not equal
  return abs(sign(x - y));
}

float not(float x){
	return 1.0 - x;
}

void main(){
	Config = InColor.a;
	CastShadows = InColor.a;
	Color = InColor;
	vec3 unitToLight = normalize(LightPosition);
	vec4 Vertex = vec4(InVertex, 1.0);
	
	float config_set = when_ge(InColor.a, 0.0) * Fancy; //If configuration is set
	vec2 Unpacked = Unpack(InColor.a, int(2048.0));
	float Addon = config_set * ( cos(Time + Unpacked.y * 8.0) +0.8) * .85 * 0.7 * Unpacked.x * 1.2;

	float invert_uk = when_lt(Unpacked.y, 0.5);
	Vertex.x += invert_uk * Addon;
	Vertex.z -= invert_uk * Addon;
	Vertex.x -= not(invert_uk) * Addon;
	Vertex.z += not(invert_uk) * Addon;

	gl_Position = gl_ModelViewProjectionMatrix * Vertex;
	Height = U_Height;
	BotColor = U_BotColor;
	TopColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - Vertex.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	
	float use_shadows = when_neq(UseShadows, 0.0) * when_neq(InColor.a, -1.0);

	float ShadowDist = DistanceToCamera - (ShadowDistance - ShadowTransition);
	ShadowDist /= ShadowTransition;
	Coords = use_shadows * ShadowMVP * vec4(InVertex,1.0);
	Coords.w = use_shadows * clamp(1.0 - ShadowDist, 0.0, 1.0);
	LightDir = use_shadows * unitToLight;
	
	InPos = Vertex;
	InNorm = vec4(InNormal, 1.0);
}

