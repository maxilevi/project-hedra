
#version 330 core
!include<"Includes/GammaCorrection.shader">
!include<"Includes/Conditionals.shader">
!include<"Includes/Sky.shader">

in vec4 raw_color;
in vec4 Color;
in vec3 pointlight_color;
in vec4 InPos;
in vec4 InNorm;
in float Visibility;
in vec4 Coords;
in vec3 LightDir;
in float Depth;
in float CastShadows;
in float DitherVisibility;
in vec3 base_vertex_position;

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
uniform float UseShadows;
uniform mat4 ShadowMVP;
uniform float Snow = 0.0;
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
    float ShadowVisibility = CalculateShadows();
    float tex = texture(noiseTexture, base_vertex_position).r;
    vec3 completeColor = (Color.xyz * ShadowVisibility + pointlight_color * raw_color.xyz) * (tex + 1.0);

	vec3 final_color = linear_to_srbg(completeColor);
	vec4 NewColor = 
	    mix(sky_color(), vec4(final_color, Color.w), Visibility);

	if(Visibility == 0.0)
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
	float has_shadows = when_gt(UseShadows, 0.0);
	float bias = max(0.001 * (1.0 - dot(InNorm.xyz, LightDir)), 0.0) + 0.001;
	vec4 ShadowCoords = Coords * vec4(.5,.5,.5,1.0) + vec4(.5,.5,.5, 0.0);		
	float shadow = 0.0;
	float use_disk = or(when_eq(UseShadows,3.0), when_eq(UseShadows, 2.0));
	vec2 texelSize = 1.0 / textureSize(ShadowTex, 0);
	
	for(int x = int(-1.0 * use_disk * has_shadows + 2.0 * not(use_disk) ); x <= 1.0 * use_disk * has_shadows; ++x)
	{
		for(float y = -1.0; y <= 1.0; ++y)
		{
			for (int i=int(0.0);i<4.0;i++)
			{
				vec4 fetch = texture(ShadowTex, ShadowCoords.xy + vec2(x, y) * texelSize + poissonDisk[i] / 1500.0);
				float pcfDepth = fetch.r; 
				if ( pcfDepth  <  ShadowCoords.z - bias)
					shadow += 1.0 * Coords.w * fetch.w;  
			}					
		}    
	}
	shadow += use_disk * has_shadows * (shadow / (9.0*4.0) - shadow);

	for(int x = int(-1.0 * not(use_disk) * has_shadows + 2.0 * use_disk); x <= 1.0 * not(use_disk) * has_shadows; ++x)
	{
		for(float y = -1.0; y <= 1.0; ++y)
		{
			vec4 fetch = texture(ShadowTex, ShadowCoords.xy  + vec2(x, y) * texelSize );
			float pcfDepth = fetch.r; 
			if ( pcfDepth  <  ShadowCoords.z - bias)
				shadow += 1.0 * Coords.w * fetch.a; 
		}    
	}
	shadow += not(use_disk) * has_shadows * ( shadow / 9.0 - shadow);
	return 1.0 - (shadow * .65);
}