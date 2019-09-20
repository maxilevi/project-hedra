#version 330 core

!include<"Includes/Sky.shader">

in vec3 pass_color;
in vec3 pass_normal;
in float pass_visibility;
in vec4 pass_clipSpace;
in vec2 textureCoords;
in vec3 toCamera;
in vec3 pass_specular;
in vec3 pass_diffuse;
in vec3 pass_highlights;

layout(location = 0)out vec4 OutColor; 
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

uniform sampler2D depthMap;
uniform sampler2D refractionMap;
uniform sampler2D reflectionMap;
uniform sampler2D dudvMap;

uniform float WaveMovement;
uniform float Smoothness;

const vec3 waterColor = vec3(0.604, 0.867, 0.851);
const float waveStrength = 0.0075;
const float speed = 0.02;
const float fresnelReflective = 0.05;
const float edgeSoftness = 4.0;
const float minBlueness = 0.1;
const float maxBlueness = 0.4;
const float murkyDepth = 128;

vec3 applyMurkiness(vec3 refractColor, float waterDepth) {
	float murkyFactor = smoothstep(0, murkyDepth, waterDepth);
	float murkiness =  minBlueness + murkyFactor * (maxBlueness - minBlueness);
	return mix(refractColor, waterColor, murkiness);
}

float toLinearDepth(float zDepth){
	const float near = 2.0;
	const float far = 4096.0;
	return 2.0 * near * far / (far + near - (2.0 * zDepth - 1.0) * (far - near));
}

float calculateWaterDepth(vec2 texCoords){
	float depth = texture(depthMap, texCoords).a;
	float floorDistance = toLinearDepth(depth);
	depth = gl_FragCoord.z;
	float waterDistance = toLinearDepth(depth);
	return floorDistance - waterDistance;
}

float calculateFresnel(){
	vec3 viewVector = normalize(toCamera);
	vec3 normal = normalize(pass_normal);
	float refractiveFactor = dot(viewVector, normal);
	refractiveFactor = pow(refractiveFactor, fresnelReflective);
	return clamp(refractiveFactor, 0.0, 1.0);
}

vec2 clipSpaceToTexCoords(vec4 clipSpace){
	vec2 ndc = (clipSpace.xy / clipSpace.w);
	vec2 texCoords = ndc / 2.0 + 0.5;
	return clamp(texCoords, 0.002, 0.998);
}

void main()
{
	if(pass_visibility < 0.0005) discard;
	
	vec2 projectiveCoords = clipSpaceToTexCoords(pass_clipSpace);
	vec2 invertedProjectiveCoords =	vec2(projectiveCoords.x, 1.0 - projectiveCoords.y);

	vec2 distortedTexCoords = texture(dudvMap, vec2(textureCoords.x + WaveMovement * speed, textureCoords.y)).rg * 0.1;
	distortedTexCoords = textureCoords + vec2(distortedTexCoords.x, distortedTexCoords.y + WaveMovement * speed);
	vec2 totalDistortion = (texture(dudvMap, distortedTexCoords).rg * 2.0 - 1.0) * waveStrength;

	vec3 refractColour = texture(refractionMap, projectiveCoords + totalDistortion).rgb;
	vec3 reflectColour = texture(reflectionMap, invertedProjectiveCoords + totalDistortion).rgb;

	float waterDepth = calculateWaterDepth(projectiveCoords);

	//apply some blueness
	refractColour = applyMurkiness(refractColour, waterDepth);
	reflectColour = mix(reflectColour, pass_color, minBlueness);

	vec3 finalColour = mix(reflectColour, refractColour, calculateFresnel());
	finalColour = finalColour * pass_diffuse * pass_highlights + pass_specular;
	float alpha = clamp(waterDepth / edgeSoftness, 0.0, 1.0);
	
	OutColor = mix(sky_color(), vec4(finalColour, alpha), pass_visibility);
	OutPosition = vec4(0.0, 0.0, 0.0, 0.0);
	OutNormal = vec4(0.0, 0.0, 0.0, 0.0);
}