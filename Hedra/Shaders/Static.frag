#version 330 core

!include<"Includes/GammaCorrection.shader">
!include<"Includes/Sky.shader">
!include<"Includes/NoiseTexture.shader">
!include<"Includes/Lighting.shader">

in float Config;
in vec4 raw_color;
in vec4 InPos;
in vec4 InNorm;
in float Visibility;
in vec4 Coords;
in float DitherVisibility;
in float NoiseScaling;
in vec3 base_vertex_position;
in float use_shadows;
in float shadow_quality;
in vec3 worldColor;
in vec3 unitToLight;

layout(location = 0)out vec4 OutColor;
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

const vec2 poissonDisk[4] = vec2[](
vec2(-0.94201624, -0.39906216),
vec2(0.94558609, -0.76890725),
vec2(-0.094184101, -0.92938870),
vec2(0.34495938, 0.29387760)
);

const mat4 ditherMat = mat4(
1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
);

uniform sampler2DShadow  ShadowTex;
uniform bool Dither;
const float NoShadowsFlag = -1.0;
const float NoHighlightFlag = -2.0;
const float FlagEpsilon = 0.1;

float CalculateShadows();

void main()
{
    if (Visibility < 0.005)
    {
        discard;
    }
    if (Dither)
    {
        if (DitherVisibility - ditherMat[int(gl_FragCoord.x) % 4][int(gl_FragCoord.y) % 4] < 0.0) discard;
    }

    vec3 FLightColor = calculate_lights(LightColor, InPos.xyz);
    vec3 point_light_color = diffuse(normalize(unitToLight), normalize(InNorm.xyz), FLightColor).rgb;
    vec3 completeColor = worldColor + point_light_color * raw_color.xyz;

    int scalingFactor = int(NoiseScaling / 0.1);
    float layeredScaling = mix(0.15, 1.0, scalingFactor / 10.0);
    
    float ShadowVisibility = use_shadows > 0.0 ? CalculateShadows() : 1.0;
    float tex = CalculateNoiseTex(InNorm.xyz, base_vertex_position * 0.55) * 0.95;
    vec3 final_color = linear_to_srbg(completeColor * (tex + 1.0) * ShadowVisibility);
    vec4 NewColor = mix(sky_color(), vec4(final_color, raw_color.w), Visibility);

    if (Visibility < 0.005)
    {
        OutColor = NewColor;
        OutPosition = vec4(InPos.xyz, gl_FragCoord.z);
        OutNormal = vec4(0.0, 0.0, 0.0, 1.0);
    }
    else
    {
        float ditherVisibility = (Dither ? DitherVisibility : 1.0);
        mat3 NormalMat = mat3(transpose(inverse(_modelViewMatrix)));
        vec3 transformedPosition = (_modelViewMatrix * vec4(InPos.xyz, 1.0)).xyz;
        OutPosition = vec4(transformedPosition, gl_FragCoord.z) * ditherVisibility;
        OutColor = NewColor;
        OutNormal = vec4(NormalMat * InNorm.xyz, 0.0) * ditherVisibility;
    }
}

float CalculateShadows()
{
    float bias = max(0.001 * (1.0 - dot(InNorm.xyz, unitToLight)), 0.0) + 0.0015;
    vec4 ShadowCoords = Coords * vec4(.5, .5, .5, 1.0) + vec4(.5, .5, .5, 0.0);
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(ShadowTex, 0);
    float samples = 0.0;
    int addedMax = shadow_quality > 1.0 ? shadow_quality > 2.0 ? 1 : 1 : 0;
    for (int x = -1; x < 1 + addedMax; ++x)
    {
        for (int y = -1; y < 1 + addedMax; ++y)
        {
            vec3 shadowUV = vec3(ShadowCoords.xy + vec2(x, y) * texelSize, ShadowCoords.z - bias);
            if (shadow_quality > 1.9)//Check if quality is equal or higher than 2.0
            {
                for (int i = 0; i < 4; i++)
                {

                    float fetch = 1.0 - texture(ShadowTex, shadowUV + vec3(poissonDisk[i] * texelSize * 2.0, 0.0));
                    shadow += fetch * Coords.w;
                    samples += 1.0;
                }
            }
            else
            {
                float fetch = 1.0 - texture(ShadowTex, shadowUV);
                shadow += fetch * Coords.w;
                samples += 1.0;
            }
        }
    }
    return 1.0 - ((shadow / samples) * .8);
}