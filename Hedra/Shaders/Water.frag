#version 330 core

!include<"Includes/Sky.shader">

in vec3 pass_color;
in vec3 pass_normal;
in vec3 pass_vertex;
in float pass_visibility;
in vec4 pass_clipSpace;
in vec2 textureCoords;
in vec3 toCamera;
in vec3 pass_fromLightVector;
in vec3 pass_lightColour;
in vec3 pass_diffuse;
in vec4 pass_highlights;

layout(location = 0)out vec4 OutColor;
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

uniform sampler2D depthMap;
uniform sampler2D dudvMap;
uniform sampler2D normalMap;

uniform float WaveMovement;
uniform float useSSR;
uniform float Smoothness;

const float waveStrength = 0.01;
const float speed = 0.04;
const float fresnelReflective = 0.7;
const float edgeSoftness = 64.0;
const float specularReflectivity = 0.8;
const float shineDamper = 10.0;

float toLinearDepth(float zDepth) {
    const float near = 2.0;
    const float far = 4096.0;
    return 2.0 * near * far / (far + near - (2.0 * zDepth - 1.0) * (far - near));
}

float calculateWaterDepth(vec2 texCoords){
    float depth = texture(depthMap, texCoords).w;
    float floorDistance = toLinearDepth(depth);
    depth = gl_FragCoord.z;
    float waterDistance = toLinearDepth(depth);
    return floorDistance - waterDistance;
}

float calculateFresnel(vec3 normal){
    vec3 viewVector = normalize(toCamera);
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
    if (pass_visibility < 0.0005) discard;

    vec2 projectiveCoords = clipSpaceToTexCoords(pass_clipSpace);

    vec2 distortedTexCoords = texture(dudvMap, vec2(textureCoords.x + WaveMovement * speed, textureCoords.y)).rg * 0.1;
    distortedTexCoords = textureCoords + vec2(distortedTexCoords.x, distortedTexCoords.y + WaveMovement * speed);
    vec2 totalDistortion = (texture(dudvMap, distortedTexCoords).rg * 2.0 - 1.0) * waveStrength;

    vec4 normalSample = texture(normalMap, distortedTexCoords);
    vec3 normal = normalize((normalSample.xyz * 2.0 - vec3(1.0)) * 0.1 + pass_normal * 0.9);

    vec3 reflectedLight = reflect(normalize(pass_fromLightVector), normal);
    float specular = max(dot(reflectedLight, normalize(toCamera)), 0.0);
    specular = pow(specular, shineDamper);
    vec3 specularHighlights = pass_lightColour * specular * specularReflectivity;

    float fresnel = calculateFresnel(normal);
    vec3 finalColour = mix(pass_color, pass_highlights.xyz, pass_highlights.a) + specularHighlights;
    float waterDepth = calculateWaterDepth(projectiveCoords);
    float alpha = (1.0 - fresnel * 0.7) * clamp((waterDepth / edgeSoftness / Smoothness), 0.0, 1.0);

    OutColor = mix(sky_color(), vec4(finalColour, alpha), pass_visibility);
    vec3 out_position = (_modelViewMatrix * vec4(pass_vertex, 1.0)).xyz;
    OutPosition = vec4(out_position, gl_FragCoord.z) * useSSR;
    vec3 out_normal = mat3(transpose(inverse(_modelViewMatrix))) * normal;
    /* 1.0 means SSR affects this fragment and SSAO doesn't */
    OutNormal = vec4(out_normal, 1.0) * useSSR;
    //OutColor = mix(OutColor, vec4(texture(depthMap, projectiveCoords).w, 0, 0, 1), pass_visibility);
}