#version 330 core

!include<"Includes/Sky.shader">

flat in vec4 Color;
in vec4 InPos;
in vec4 InNorm;
in float Visibility;
in vec4 ClipSpace;
in float Movement;

layout(location = 0)out vec4 OutColor; 
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

uniform sampler2D DepthMap;
uniform bool Dither;
uniform float Smoothness;

const float Strength = 0.01;

void main()
{
	if(Visibility < 0.005)
	{
		discard;
	}
	if(Dither)
	{
		float d = dot( gl_FragCoord.xy, vec2(.5,.5));
		if(d - floor(d) < 0.5) discard;
	}
	vec4 InputColor = Color;
	vec2 TexCoords = (ClipSpace.xy / ClipSpace.w) / 2.0 + 0.5;

	const float Near = 2.0;
	const float Far = 4096.0;
	float ObjectDepth = clamp(texture(DepthMap, TexCoords).a, 0.0, 1.0);
	float FloorDistance = 2.0 * Near * Far / (Far + Near - (2.0 * ObjectDepth - 1.0) * (Far - Near));

	float Depth = gl_FragCoord.z;
	float WaterDistance = 2.0 * Near * Far / (Far + Near - (2.0 * Depth - 1.0) * (Far - Near));
	float WaterDepth = FloorDistance - WaterDistance;
	vec4 NewColor = mix(sky_color(), InputColor, Visibility);
	OutColor = NewColor;
	OutColor.a = OutColor.a * (
		ObjectDepth > 0.05 
			? clamp((WaterDepth / 6.0 / Smoothness) * .25, 0.0, 1.0)
			: 1.0
	);
	
	OutPosition = vec4(0.0, 0.0, 0.0, 0.0);
	OutNormal = vec4(0.0, 0.0, 0.0, 0.0);
}