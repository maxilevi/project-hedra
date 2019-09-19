#version 330 core

!include<"Includes/Sky.shader">

flat in vec4 Color;
in vec4 InPos;
in vec4 InNorm;
in float Visibility;
in vec4 ClipSpace;
in vec2 textureCoords;

layout(location = 0)out vec4 OutColor; 
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

uniform sampler2D depthMap;
uniform sampler2D refractionMap;
uniform sampler2D dudvMap;

uniform bool Dither;
uniform float WaveMovement;
uniform float Smoothness;

const float waveStrength = 0.01;
const float speed = 0.05;
const float shineDamper = 20.0;
const float reflectivity = 0.6;

void main()
{
	if(Visibility < 0.0005) discard;
	
	if(Dither)
	{
		float d = dot( gl_FragCoord.xy, vec2(.5,.5));
		if(d - floor(d) < 0.5) discard;
	}
	vec2 projectiveCoords = (ClipSpace.xy / ClipSpace.w) / 2.0 + 0.5;
	vec2 invertedProjectiveCoords =	vec2(projectiveCoords.x, 1.0 - projectiveCoords.y);

	vec2 distortedTexCoords = texture(dudvMap, vec2(textureCoords.x + WaveMovement * speed, textureCoords.y)).rg * 0.1;
	distortedTexCoords = textureCoords + vec2(distortedTexCoords.x, distortedTexCoords.y + WaveMovement * speed);
	vec2 totalDistortion = (texture(dudvMap, distortedTexCoords).rg * 2.0 - 1.0) * waveStrength;

	vec4 refractionColor = texture(refractionMap, projectiveCoords + totalDistortion);
	vec4 textureColor = vec4(Color.xyz + specularHighlights, 1.0);
	
	OutColor = textureColor;
	
	const float Near = 2.0;
	const float Far = 4096.0;
	float ObjectDepth = clamp(texture(depthMap, projectiveCoords).a, 0.0, 1.0);
	float FloorDistance = 2.0 * Near * Far / (Far + Near - (2.0 * ObjectDepth - 1.0) * (Far - Near));

	float Depth = gl_FragCoord.z;
	float WaterDistance = 2.0 * Near * Far / (Far + Near - (2.0 * Depth - 1.0) * (Far - Near));
	float WaterDepth = FloorDistance - WaterDistance;
	vec4 NewColor = mix(sky_color(), textureColor, Visibility);
	OutColor = NewColor;
	OutColor.a = OutColor.a * (
		ObjectDepth > 0.05 
			? clamp((WaterDepth / 6.0 / Smoothness) * .25, 0.0, 1.0)
			: 1.0
	);
	
	OutPosition = vec4(0.0, 0.0, 0.0, 0.0);
	OutNormal = vec4(0.0, 0.0, 0.0, 0.0);
}