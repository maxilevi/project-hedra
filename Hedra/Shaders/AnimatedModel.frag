#version 330 core
!include<"Includes/GammaCorrection.shader">
!include<"Includes/Sky.shader">

in vec4 pass_color;
in vec3 pass_normal;
in float pass_visibility;
in vec3 pass_position;
in vec4 pass_coords;
in vec3 pass_lightDir;
in vec4 pass_lightDiffuse;

layout(location = 0)out vec4 out_colour;
layout(location = 1)out vec4 out_position;
layout(location = 2)out vec4 out_normal;

uniform bool UseFog;
uniform bool UseShadows = true;
uniform sampler2D ShadowTex;
uniform vec4 Tint;
uniform vec4 BaseTint;
uniform float Alpha;

void main(void)
{
	if(UseFog && pass_visibility < 0.005)
	{
		discard;
	}
	float ShadowVisibility = 1.0;
	float bias = 0.005;//max(0.05 * (1.0 - dot(InNorm.xyz, pass_lightDir)), 0.005);
	
	vec4 ShadowCoords = pass_coords * vec4(.5,.5,.5, 1.0) + vec4(.5,.5,.5, 0.0);
			
	float shadow = 0.0;
	vec2 texelSize = 1.0 / textureSize(ShadowTex, 0);
	for(int x = -1; x <= 1; ++x)
	{
		for(int y = -1; y <= 1; ++y)
		{
			vec4 fetch = texture(ShadowTex, ShadowCoords.xy + vec2(x, y) * texelSize);
		    float pcfDepth = fetch.r; 
		    if ( pcfDepth  <  ShadowCoords.z - bias){
			    shadow += 1.0;
			}       
		}    
	}
	shadow /= 9.0;
	ShadowVisibility = 1.0 - ( (shadow * .45) * pass_coords.w);
		
	if(ShadowCoords.z > 1.0 || !UseShadows)
		ShadowVisibility = 1.0;
	
	vec4 new_color = pass_color * ShadowVisibility * (BaseTint + Tint) + pass_lightDiffuse * (BaseTint + Tint);
	new_color = vec4(linear_to_srbg(new_color.xyz), new_color.w);

	if(UseFog)
	{
		vec4 NewColor = mix(sky_color(), new_color, pass_visibility);
		out_colour = vec4(NewColor.xyz, new_color.w);
	}
	else
	{
		out_colour = vec4(new_color.xyz, new_color.w);
	}		
	
	// Ignore the gl_FragCoord.z since it causes issues with the water
	mat3 mat = mat3(transpose(inverse(_modelViewMatrix)));
	out_position = vec4( (_modelViewMatrix * vec4(pass_position, 1.0)).xyz * Alpha, 2.0);
	out_normal = vec4( mat * pass_normal, 1.0) * Alpha;
}
