#version 330 core

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

float noise(vec3 p);

void main(){

	vec4 inputColor = Color;
		
	
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
	
	OutPosition = vec4(InPos, gl_FragCoord.z);
	OutNormal = vec4(InNorm,1.0);
}


float mod289(float x){return x - floor(x * (1.0 / 289.0)) * 289.0;}
vec4 mod289(vec4 x){return x - floor(x * (1.0 / 289.0)) * 289.0;}
vec4 perm(vec4 x){return mod289(((x * 34.0) + 1.0) * x);}

float noise(vec3 p){
    vec3 a = floor(p);
    vec3 d = p - a;
    d = d * d * (3.0 - 2.0 * d);

    vec4 b = a.xxyy + vec4(0.0, 1.0, 0.0, 1.0);
    vec4 k1 = perm(b.xyxy);
    vec4 k2 = perm(k1.xyxy + b.zzww);

    vec4 c = k2 + a.zzzz;
    vec4 k3 = perm(c);
    vec4 k4 = perm(c + 1.0);

    vec4 o1 = fract(k3 * (1.0 / 41.0));
    vec4 o2 = fract(k4 * (1.0 / 41.0));

    vec4 o3 = o2 * d.z + o1 * (1.0 - d.z);
    vec2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);

    return o4.y * d.y + o4.x * (1.0 - d.y);
}