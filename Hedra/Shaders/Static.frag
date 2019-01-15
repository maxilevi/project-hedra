#version 330 core

precision mediump float;

!include<"Includes/GammaCorrection.shader">
!include<"Includes/Sky.shader">

in vec4 raw_color;
in vec4 Color;
in vec3 pointlight_color;
in vec4 InPos;
in vec4 InNorm;
in float Visibility;
in vec4 Coords;
in vec3 LightDir;
in float DitherVisibility;
in vec3 base_vertex_position;
in float use_shadows;
in float shadow_quality;

layout(location = 0)out vec4 OutColor; 
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

const vec2 poissonDisk[4] = vec2[](
  vec2( -0.94201624, -0.39906216 ),
  vec2( 0.94558609, -0.76890725 ),
  vec2( -0.094184101, -0.92938870 ),
  vec2( 0.34495938, 0.29387760 )
);

const mat4 ditherMat = mat4(
   1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
   13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
   4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
   16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
);

uniform sampler2D ShadowTex;
uniform mat4 ShadowMVP;
uniform bool Dither;
uniform sampler3D noiseTexture;

float CalculateShadows();

void main()
{
	if(Visibility < 0.005)
	{
		discard;
	}
	if (Dither)
	{
        if (DitherVisibility - ditherMat[int(gl_FragCoord.x) % 4][int(gl_FragCoord.y) % 4] < 0.0) discard;
	}
    float ShadowVisibility = use_shadows > 0.0 ? CalculateShadows() : 1.0;
    float tex = texture(noiseTexture, base_vertex_position).r;
    vec3 completeColor = (Color.xyz * ShadowVisibility + pointlight_color * raw_color.xyz) * (tex + 1.0);

	vec3 final_color = linear_to_srbg(completeColor);
	vec4 NewColor = 
	    mix(sky_color(), vec4(final_color, Color.w), Visibility);

	if(Visibility < 0.005)
	{
		OutColor = NewColor;
		OutPosition = vec4( InPos.xyz, gl_FragCoord.z);
		OutNormal = vec4(0.0, 0.0, 0.0, 1.0);
	}
	else
	{
		mat3 NormalMat = mat3(transpose(inverse(_modelViewMatrix)));
		OutColor = NewColor;
        OutPosition = vec4( (_modelViewMatrix * vec4(InPos.xyz, 1.0)).xyz, gl_FragCoord.z) * (Dither ? DitherVisibility : 1.0);
		OutNormal = vec4(NormalMat * InNorm.xyz, 1.0) * (Dither ? DitherVisibility : 1.0);
	}
}


float CalculateShadows()
{
	float bias = max(0.001 * (1.0 - dot(InNorm.xyz, LightDir)), 0.0) + 0.001;
	vec4 ShadowCoords = Coords * vec4(.5,.5,.5,1.0) + vec4(.5,.5,.5, 0.0);		
	float shadow = 0.0;
	vec2 texelSize = 1.0 / textureSize(ShadowTex, 0);
	float samples = 0.0;
    for(int x = -1; x < 1; ++x)
    {
        for(int y = -1; y < 1; ++y)
        {
            if(shadow_quality >= 2.0)
            {
                for (int i = 0; i < 4; i++)
                {
                    vec4 fetch = texture(ShadowTex, ShadowCoords.xy + vec2(x, y) * texelSize + poissonDisk[i] * .001);
                    float pcfDepth = fetch.r; 
                    if (pcfDepth  <  ShadowCoords.z - bias)
                        shadow += 1.0 * Coords.w * fetch.w;
                    samples += 1.0;
                }
            }
            else 
            {
                vec4 fetch = texture(ShadowTex, ShadowCoords.xy  + vec2(x, y) * texelSize );
                float pcfDepth = fetch.r; 
                if ( pcfDepth  <  ShadowCoords.z - bias)
                    shadow += 1.0 * Coords.w * fetch.a; 
                samples += 1.0;
            }
        }    
    }
    shadow /= samples;

	return 1.0 - (shadow * .65);
}