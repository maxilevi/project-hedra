#version 330 core
!include<"Includes/Lighting.shader">
!include<"Includes/Highlights.shader">

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

layout(location = 0)in vec3 InVertex;
layout(location = 1)in vec4 InColor;
layout(location = 2)in vec3 InNormal;

out vec3 pass_normal;
out vec3 pass_color;
out vec3 pass_highlights;
out float pass_visibility;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;
out vec4 pass_clipSpace;
out vec2 textureCoords;
out vec3 toCamera;

out vec3 pass_diffuse;
out vec3 pass_specular;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

uniform vec3 PlayerPosition;
uniform vec3 Scale;
uniform vec3 Offset;
uniform vec3 BakedOffset;
uniform mat4 TransformationMatrix;

const float specularReflectivity = 0.4;
const float shineDamper = 20.0;
const vec3 lightBias = vec3(1.0);

vec3 calcSpecularLighting(vec3 toCamVector, vec3 toLightVector, vec3 normal)
{
	vec3 reflectedLightDirection = reflect(-toLightVector, normal);
	float specularFactor = dot(reflectedLightDirection , toCamVector);
	specularFactor = max(specularFactor,0.0);
	specularFactor = pow(specularFactor, shineDamper);
	return specularFactor * specularReflectivity * LightColor;
}

vec3 calculateDiffuseLighting(vec3 toLightVector, vec3 normal)
{
	float brightness = max(dot(toLightVector, normal), 0.0);
	return (LightColor * lightBias.x) + (brightness * LightColor * lightBias.y);
}

void main()
{
    vec4 vertex = TransformationMatrix * (vec4((InVertex + BakedOffset) * Scale + Offset, 1.0));

    pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;
	
	float distanceToCamera = length(vec3(PlayerPosition - vertex.xyz).xz);
	pass_visibility = clamp( (MaxDist - distanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);

	vec3 toLightVector = normalize(LightPosition);
	toCamera = normalize((inverse(_modelViewMatrix) * vec4(0.0,0.0,0.0,1.0) ).xyz - vertex.xyz);

 	gl_Position = _modelViewProjectionMatrix * vertex;

	textureCoords = vertex.xz * 0.005;
	pass_highlights = apply_highlights(vec4(1.0), vertex.xyz).xyz;
	pass_clipSpace = gl_Position;
	pass_normal = InNormal;
	pass_specular = calcSpecularLighting(toCamera, toLightVector, pass_normal);
	pass_diffuse = calculateDiffuseLighting(toLightVector, pass_normal) + calculate_lights(LightColor, vertex.xyz, 2.25);
}