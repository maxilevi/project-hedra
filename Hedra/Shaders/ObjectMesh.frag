#version 330 compatibility
!include<"Includes/GammaCorrection.shader">

in vec4 Color;
in vec4 IColor;
in float Visibility;
in float Height;
in vec4 BotColor;
in vec4 TopColor;
in vec3 InPos;
in vec3 InNorm;
in vec4 Coords;
in vec3 LightDir;
in vec3 vertex_position;
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
uniform bool UseBaseTint;
uniform bool Outline;
uniform vec4 OutlineColor;
uniform float Time;

vec4 doTint(vec4 color, vec4 tint);
vec3 doTint(vec3 color, vec3 tint);

void main(){

	vec3 tex = Color.xyz * vec3(1.0, 1.0, 1.0) * texture(noiseTexture, vertex_position.xyz).r;
	vec4 inputColor = vec4(linear_to_srbg(Color.xyz + tex * 0.2 * useNoiseTexture), Color.w);

	if(Outline){
		inputColor += vec4(Color.xyz, -1.0) * .5f;
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

	vec4 SkyColor = vec4( mix(BotColor, TopColor, (gl_FragCoord.y / Height) - .25) );
	vec3 pointLightColor = doTint(linear_to_srbg(point_diffuse.xyz), BaseTint.rgb) * Tint.rgb;

	if(UseFog){
		vec4 NewColor = mix(SkyColor, doTint(inputColor * ShadowVisibility, vec4(BaseTint.rgb, 1.0)) * vec4(Tint.rgb, 1.0) + vec4(pointLightColor, 0.0), Visibility);
		
		FColor = vec4(NewColor.xyz, Alpha);
	}else{
		FColor = vec4( doTint(inputColor.xyz * ShadowVisibility, BaseTint.rgb) * Tint.rgb + pointLightColor, Alpha);
	}

	if (Outline) {
		vec3 unitToCamera = normalize( (inverse(gl_ModelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - vertex_position.xyz);
		float outlineDot = max(0, 1.0-dot(base_normal, unitToCamera));
		FColor = outlineDot * (cos(Time*10)-.0) * 2.0 * OutlineColor;
		OutPosition = vec4(0.0, 0.0, 0.0, 0.0);
		OutNormal = vec4(0.0, 0.0, 0.0, 0.0);
	} else {
		OutPosition = vec4(InPos, gl_FragCoord.z) * Alpha;
		OutNormal = vec4(InNorm,1.0) * Alpha;
	}
}

vec4 doTint(vec4 color, vec4 tint) {
	if(!UseBaseTint) return color;
	float shade = color.x + color.y + color.z;
	shade = shade * 0.33;
	return vec4(shade * tint.xyz, 1.0); 
}

vec3 doTint(vec3 color, vec3 tint){
	return doTint(vec4(color, 1.0), vec4(tint, 1.0)).xyz;
}