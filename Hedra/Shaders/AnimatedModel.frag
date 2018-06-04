#version 330 compatibility
!include<"Includes/GammaCorrection.shader">

in vec4 pass_color;
in vec3 pass_normal;
in float pass_visibility;
in float pass_height;
in vec4 pass_botColor;
in vec4 pass_topColor;
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

void main(void){

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
	
	vec4 SkyColor = vec4( mix(pass_botColor, pass_topColor, (gl_FragCoord.y / pass_height) - .25) );
	vec4 new_color = pass_color * ShadowVisibility * (BaseTint + Tint) + pass_lightDiffuse * (BaseTint + Tint);
	new_color = vec4(linear_to_srbg(new_color.xyz), new_color.w);

	if(UseFog){
		vec4 NewColor = mix(SkyColor, new_color, pass_visibility);
		out_colour = vec4(NewColor.xyz, new_color.w);
	}else{
		out_colour = vec4(new_color.xyz, new_color.w);
	}		
		
	mat3 mat = mat3(transpose(inverse(gl_ModelViewMatrix)));
	out_position = vec4( (gl_ModelViewMatrix * vec4(pass_position, 1.0)).xyz, gl_FragCoord.z) * Alpha;
	out_normal = vec4( mat * pass_normal, 1.0) * Alpha;
}
