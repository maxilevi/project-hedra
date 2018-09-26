#version 330 core
!include<"Includes/GammaCorrection.shader">
!include<"Includes/Sky.shader">

in vec4 Color;
in vec4 IColor;
in float Visibility;
in vec3 InPos;
in vec3 InNorm;
in vec4 Coords;
in vec3 LightDir;
in vec3 vertex_position;
in vec3 base_vertex_position;
in vec3 base_normal;
in vec4 point_diffuse;

layout(location = 0) out vec4 FColor;
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

uniform vec4 Tint;
uniform vec4 BaseTint;
uniform bool UseFog;
uniform vec2 res;
uniform float Alpha;
uniform sampler2D ShadowTex;
uniform bool UseShadows;
uniform bool Dither;
uniform float useNoiseTexture;
uniform sampler3D noiseTexture;
uniform bool Outline;
uniform vec4 OutlineColor;
uniform float Time;

void main()
{
	if(UseFog && Visibility < 0.005)
	{
		discard;
	}
	vec3 tex = Color.xyz * vec3(1.0, 1.0, 1.0) * texture(noiseTexture, base_vertex_position.xyz * 0.5).r;
	vec4 inputColor = vec4(linear_to_srbg(Color.xyz + tex * useNoiseTexture), Color.w);

	if(Outline)
	{
		inputColor += vec4(Color.xyz, -1.0) * .5;
	}
	if(Dither){
		float d = dot( gl_FragCoord.xy, vec2(.5,.5));
		if( d-floor(d) < 0.5) discard;
	}

	float ShadowVisibility = 1.0;
	if(UseFog && UseShadows){
		float bias = 0.005;//max(0.05 * (1.0 - dot(InNorm.xyz, LightDir)), 0.005);
		//bias = clamp(bias, 0.0, 1.0);
	
		vec4 ShadowCoords = Coords * vec4(.5,.5,.5,1.0) + vec4(.5,.5,.5, 0.0);
			
		float shadow = 0.0;
		vec2 texelSize = 1.0 / textureSize(ShadowTex, 0);
		for(int x = -1; x <= 1.0; ++x)
		{
		    for(int y = -1; y <= 1.0; ++y)
		    {
				vec4 fetch = texture(ShadowTex, ShadowCoords.xy + vec2(x, y) * texelSize);
		        float pcfDepth = fetch.r; 
		        if ( pcfDepth  <  ShadowCoords.z - bias){
			    	shadow += 1.0;
				}       
		    }    
		}
		shadow /= 9.0;
		ShadowVisibility = 1.0 - ( (shadow * .45) * Coords.w);
		
		if(ShadowCoords.z > 1.0)
			ShadowVisibility = 1.0;
	}

	vec3 pointLightColor = linear_to_srbg(point_diffuse.xyz) * (Tint.rgb + BaseTint.rgb);

	if(UseFog){
		vec4 NewColor = mix(sky_color(), inputColor * ShadowVisibility * vec4(Tint.rgb + BaseTint.rgb, 1.0) + vec4(pointLightColor, 0.0), Visibility);
		
		FColor = vec4(NewColor.xyz, Alpha);
	}else{
		FColor = vec4( inputColor.xyz * ShadowVisibility * (Tint.rgb + BaseTint.rgb) + pointLightColor, Alpha);
	}

	if (Outline)
	{
		vec3 unitToCamera = normalize( (inverse(_modelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - vertex_position.xyz);
		float outlineDot = max(0.0, 1.0 - dot(base_normal, unitToCamera));
		FColor = outlineDot * ( cos(Time * 10.0)-.0) * 2.0 * OutlineColor * Alpha;
		OutPosition = vec4(0.0, 0.0, 0.0, 0.0);
		OutNormal = vec4(0.0, 0.0, 0.0, 0.0);
	}
	else
	{
	    // Ignore the gl_FragCoord.z since it causes issues with the water
	    mat3 NormalMat = mat3(transpose(inverse(_modelViewMatrix)));
		OutPosition = vec4((_modelViewMatrix * vec4(InPos, 1.0)).xyz * Alpha, 2.0);
		OutNormal = vec4(NormalMat * InNorm.xyz, 1.0) * Alpha;
	}
}