#version 330 compatibility

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
out float Height;
out vec4 BotColor;
out vec4 TopColor;
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
uniform float Transparency = 0.7;
uniform vec3 Scale;
uniform vec3 Offset;
uniform vec3 BakedOffset;
uniform vec3 LightPosition = vec3(-500.0, 1000.0, 0.0);
uniform vec3 LightColor = vec3(1.0, 1.0, 1.0);
uniform mat4 TransformationMatrix;

float GetY(float x, float y, float z);
float GetOffset(float x, float y, float z, float val1, float val2);
vec3 Cross(vec3 v1, vec3 v2);

vec3 DiffuseModel(vec3 unitToLight, vec3 unitNormal, vec3 LColor){	
	float Brightness = max(dot(unitNormal, unitToLight) ,0.2);
	return (Brightness * LColor );
}

vec2 Unpack(float inp, int prec)
{
    vec2 outp = vec2(0.0, 0.0);

    outp.y = mod(inp, prec);
    outp.x = floor(inp / prec);

    return outp / (prec - 1.0);
}

const float Damper = 6.0;
const float Reflectivity = 0.05;
void main()
{
	Movement = WaveMovement;
    vec4 v = vec4((InVertex + BakedOffset) * Scale + Offset, 1.0);
    v.y = GetY(v.x,v.y,v.z) * InNormal.z * .6 * Scale.y + (InVertex.y+BakedOffset.y) * Scale.y + Offset.y;
    
    vec2 Unpacked1 = Unpack(InNormal.x, int(4096.0));
    vec2 Unpacked2 = Unpack(InNormal.y, int(4096.0));
    
    vec3 V0 = vec3(Unpacked1.x, 0.0, Unpacked1.y);
    vec3 V1 = vec3(Unpacked2.x, 0.0, Unpacked2.y);
    
    V0.x = (V0.x + InVertex.x + BakedOffset.x) * Scale.x;
    V0.z = (V0.z + InVertex.z + BakedOffset.z) * Scale.z;
    V1.x = (V1.x + InVertex.x + BakedOffset.x) * Scale.x;
    V1.z = (V1.z + InVertex.z + BakedOffset.z) * Scale.z;
    V0.y = (V0.y + InVertex.y + BakedOffset.y) * Scale.y;
    V1.y = (V1.y + InVertex.y + BakedOffset.y) * Scale.y;
    
    V0.y = GetY(V0.x, V0.y, V0.z) * 0.4 * Scale.y + (InVertex.y+BakedOffset.y) * Scale.y + Offset.y;
    V1.y = GetY(V1.x, V1.y, V1.z) * 0.4 * Scale.y + (InVertex.y+BakedOffset.y) * Scale.y + Offset.y;
    
    vec3 Normal = normalize(Cross(v.xyz - V0, V1 - v.xyz));
 	
    Height = U_Height;
	BotColor = U_BotColor;
	TopColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - v.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	
	//Lighting
	vec3 unitNormal = Normal;
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(gl_ModelViewMatrix) * vec4(0.0,0.0,0.0,1.0) ).xyz - v.xyz);

	vec3 FLightColor = LightColor;
	for(int i = 0; i < 12; i++){
		float dist = length(Lights[i].Position - v.xyz);
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
	vec4 Specular = vec4(Damp * FLightColor, 1.0);
	
	//Rim Lighting
	float rim = 1.0 - max(dot(unitToCamera, unitNormal), 0.0);
	rim = smoothstep(0.6, 1.0, rim);
	vec3 finalRim = InColor.rgb * 0.1 * rim;

	//Diffuse Lighting
	vec4 Ambient = InColor * 0.0;
	vec3 Diffuse = DiffuseModel(vec3(0.0, 0.0, 1.0), unitNormal, FLightColor) * .45 + DiffuseModel(vec3(0.0, 0.0, -1.0), unitNormal, FLightColor) * .45
				   + DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal, FLightColor) * .45 + DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal, FLightColor) * .45 + DiffuseModel(unitToLight, unitNormal, FLightColor) * 1.05;
	
	Color = Ambient + vec4(finalRim, 0.0) + (vec4(Diffuse, 1.0) * InColor) + Specular;
 	Color.a = Transparency;
 	
	v = TransformationMatrix * v;
 	gl_Position = gl_ModelViewProjectionMatrix * v;

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

float GetOffset(float x, float y, float z, float val1, float val2){

	float rX = ((mod(x+z*x*val1, waveLength)/waveLength)+Movement*0.2) * 2.0 * PI;
	float rZ = ( (mod( val2 * (z*x + x*z), waveLength) / waveLength) +Movement*0.2 * 2.0) * 2.0 * PI;
	
	return 1.4 * 0.5 * (sin(rX) + sin(rZ));
}

float GetY(float x, float y, float z){
    return GetOffset(x,y,z, 0.1, 0.3) + 0.0;
}

vec3 Cross(vec3 v1, vec3 v2){
	return vec3(v1.y * v2.z - v1.z * v2.y,
		     v1.z * v2.x - v1.x * v2.z,
		     v1.x * v2.y - v1.y * v2.x);
}