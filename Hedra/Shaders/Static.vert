#version 330 core

!include<"Includes/GammaCorrection.shader">
!include<"Includes/Conditionals.shader">
!include<"Includes/Lighting.shader">

layout(location = 0)in vec3 InVertex;
layout(location = 1)in vec4 InColor;
layout(location = 2)in vec3 InNormal;

out vec4 raw_color;
out vec4 InPos;
out vec4 InNorm;
out vec3 pointlight_color;
out vec4 Color;
out float Visibility;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;
out vec4 Coords;
out vec3 LightDir;
out float CastShadows;
out float DitherVisibility;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

uniform vec3 PlayerPosition;
uniform float Time;
uniform float Fancy = 1.0;
uniform mat4 ShadowMVP;
uniform float ShadowDistance;
uniform float UseShadows = 3.0;
uniform vec3 Scale;
uniform vec3 Offset;
uniform vec3 BakedOffset;
uniform mat4 TransformationMatrix;
uniform float MinDitherDistance;
uniform float MaxDitherDistance;
uniform vec4 AreaColors[16];
uniform vec4 AreaPositions[16];
const float ShadowTransition = 10.0;

struct PointLight
{
    vec3 Position;
    vec3 Color;
    float Radius;
};
uniform PointLight Lights[12];

vec2 Unpack(float inp, int prec)
{
    vec2 outp = vec2(0.0, 0.0);

    outp.y = mod(inp, prec);
    outp.x = floor(inp / prec);

    return outp / (prec - 1.0);
}

void main(){
    vec4 linear_color = srgb_to_linear(InColor); 
	float Config = InColor.a;
	CastShadows = InColor.a;
	vec3 unitToLight = normalize(LightPosition);
	vec4 Vertex = vec4((InVertex + BakedOffset) * Scale + Offset, 1.0);
	
	float config_set = when_ge(InColor.a, 0.0) * Fancy; //If configuration is set
	vec2 Unpacked = Unpack(InColor.a, int(2048.0));
	float Addon = config_set * ( cos(Time + Unpacked.y * 8.0) +0.8) * .85 * 0.7 * Unpacked.x * 1.2;

	float invert_uk = when_lt(Unpacked.y, 0.5);
	Vertex.x += invert_uk * Addon * Scale.x;
	Vertex.z -= invert_uk * Addon * Scale.z;
	Vertex.x -= not(invert_uk) * Addon * Scale.x;
	Vertex.z += not(invert_uk) * Addon * Scale.z;

	pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - Vertex.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	DitherVisibility = clamp( (MaxDitherDistance - DistanceToCamera) / (MaxDitherDistance - MinDitherDistance), 0.0, 1.0);

	Vertex = TransformationMatrix * Vertex;
	gl_Position = _modelViewProjectionMatrix * Vertex;

	float use_shadows = when_neq(UseShadows, 0.0) * when_neq(InColor.a, -1.0);

	float ShadowDist = DistanceToCamera - (ShadowDistance - ShadowTransition);
	ShadowDist /= ShadowTransition;
	Coords = use_shadows * ShadowMVP * vec4(InVertex,1.0);
	Coords.w = use_shadows * clamp(1.0 - ShadowDist, 0.0, 1.0);
	LightDir = use_shadows * unitToLight;
	
	InPos = Vertex;
	InNorm = vec4(InNormal, 1.0);
	
    if(!(Config+2.0 < 0.1))
    {
        for(int i = 0; i < 16; i++)
        {
            if(AreaColors[i] != vec4(0.0, 0.0, 0.0, 0.0))
            {
                linear_color = mix(AreaColors[i], linear_color, clamp(length(AreaPositions[i].xyz - InPos.xyz) / AreaPositions[i].w , 0.0, 1.0) );
            }
        }
    }
		
	//Lighting
	vec3 unitNormal = normalize(InNorm.xyz);
	vec3 unitToCamera = normalize((inverse(_modelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - Vertex.xyz);

	vec3 FLightColor = vec3(0.0, 0.0, 0.0);
	float average_color = (LightColor.r + LightColor.g + LightColor.b) / 3.0; 
	for(int i = 0; i < 12; i++){
		float dist = length(Lights[i].Position.xyz - Vertex.xyz);
		float att = 1.0 / (1.0 + dist * dist);
		att *= Lights[i].Radius * .5;
		att = min(att, 1.0);
		
		FLightColor += Lights[i].Color * att * .5 * (1.0 - average_color); 
	}
	FLightColor = clamp(FLightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));
	vec4 InputColor = vec4(linear_color.xyz, 1.0);

	Ambient += when_lt(Config + 1.0, 0.1) * 0.25;
	vec4 Specular = specular(unitToLight, unitNormal, unitToCamera, LightColor);
	vec4 Rim = rim(InputColor.rgb, LightColor, unitToCamera, unitNormal);
	vec4 Diffuse = diffuse(unitToLight, unitNormal, LightColor);
	vec4 realColor = Rim + Diffuse * InputColor + Specular;
    Color = vec4(realColor.xyz, realColor.a);	

	pointlight_color = diffuse(unitToLight, unitNormal, FLightColor).rgb;		
	raw_color = linear_color;
	
}