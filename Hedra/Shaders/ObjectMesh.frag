#version 330 compatibility

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
in vec3 PointDiffuse;
in vec3 vertex_position;

layout(location = 0) out vec4 FColor;
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

uniform vec4 Tint = vec4(1.0, 1.0, 1.0, 1.0);
uniform bool UseFog;
uniform bool Outline;
uniform vec2 res;
uniform float Alpha;
uniform sampler2D ShadowTex;
uniform bool UseShadows;
uniform bool Dither = false;
uniform float useNoiseTexture;
uniform sampler3D noiseTexture;

float noise(vec3 p);

void main(){

	vec3 tex = Color.xyz * vec3(1.0, 1.0, 1.0) * texture(noiseTexture, vertex_position.xyz).r;
	vec4 inputColor = vec4(Color.xyz + tex * 0.2 * useNoiseTexture, Color.w);	
	
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
	
	if(UseFog){
		vec4 NewColor = mix(SkyColor, (inputColor * ShadowVisibility + vec4(PointDiffuse, 0.0) ) * vec4(Tint.rgb, 1.0), Visibility);
		
		FColor = vec4(NewColor.xyz, Alpha);
	}else{
		FColor = vec4( (inputColor.xyz * ShadowVisibility + PointDiffuse.xyz) * Tint.rgb , Alpha);
	}
	
	OutPosition = vec4(InPos, gl_FragCoord.z) * Alpha;
	OutNormal = vec4(InNorm,1.0) * Alpha;
}
