#version 330 core
!include<"Includes/GammaCorrection.shader">
!include<"Includes/Sky.shader">
!include<"Includes/Shadows.shader">

in vec4 raw_color;
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
in vec3 point_diffuse;

layout(location = 0)out vec4 FColor;
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

uniform vec4 Tint;
uniform vec4 BaseTint;
uniform vec2 res;
uniform float Alpha;
uniform bvec4 DitherFogTextureShadows;
uniform sampler3D noiseTexture;
uniform bool Outline;
uniform vec4 OutlineColor;
uniform float Time;
const float bias = 0.005;

void main()
{
	if(DitherFogTextureShadows.y && Visibility < 0.005)
	{
		discard;
	}
	
	if(DitherFogTextureShadows.x)
	{
		float d = dot( gl_FragCoord.xy, vec2(.5,.5));
		if( d-floor(d) < 0.5) discard;
	}

	float ShadowVisibility = DitherFogTextureShadows.w && DitherFogTextureShadows.y 
	    ? simple_apply_shadows(Coords, bias)
	    : 1.0;
    float tex = texture(noiseTexture, base_vertex_position).r * int(DitherFogTextureShadows.z);
    vec4 inputColor = vec4(linear_to_srbg((Color.xyz * ShadowVisibility + point_diffuse.xyz * raw_color.xyz) * (Tint.rgb + BaseTint.rgb) * (tex + 1.0)), Color.w);

    if(Outline)
    {
        inputColor += vec4(Color.xyz, -1.0) * .5;
    }

	if(DitherFogTextureShadows.y)
	{
		vec4 NewColor = mix(sky_color(), inputColor, Visibility);
		
		FColor = vec4(NewColor.xyz, Alpha);
	}
	else
	{
		FColor = vec4( inputColor.xyz, Alpha);
	}
	//FColor = vec4(InNorm.xyz, inputColor.a);

	if (Outline)
	{
		vec3 unitToCamera = normalize( (inverse(_modelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - vertex_position.xyz);
		float outlineDot = max(0.0, 1.0 - dot(base_normal, unitToCamera));
		FColor = outlineDot * ( cos(Time * 10.0) ) * 2.0 * OutlineColor * Alpha;
		OutPosition = vec4(0.0, 0.0, 0.0, 0.0);
		OutNormal = vec4(0.0, 0.0, 0.0, 0.0);
	}
	else
	{
	    // Ignore the gl_FragCoord.z since it causes issues with the water
	    mat3 NormalMat = mat3(transpose(inverse(_modelViewMatrix)));
		OutPosition = vec4((_modelViewMatrix * vec4(InPos, 1.0)).xyz * Alpha, 1.0);
		OutNormal = vec4(NormalMat * InNorm.xyz, 1.0) * Alpha;
	}
}