#version 330 core
!include<"Includes/GammaCorrection.shader">
!include<"Includes/Sky.shader">
!include<"Includes/Shadows.shader">

in vec4 pass_color;
in vec3 pass_normal;
in float pass_visibility;
in vec3 pass_position;
in vec4 pass_coords;

layout(location = 0)out vec4 out_colour;
layout(location = 1)out vec4 out_position;
layout(location = 2)out vec4 out_normal;

uniform bool UseFog;
uniform bool UseShadows;
uniform vec4 Tint;
uniform vec4 BaseTint;
uniform float Alpha;
uniform bool Outline;
uniform vec4 OutlineColor;
uniform float Time;
const float bias = 0.005;

void main(void)
{
	if(UseFog && pass_visibility < 0.005)
	{
		discard;
	}
	float ShadowVisibility = UseShadows ? simple_apply_shadows(pass_coords, bias) : 1.0;	
	vec4 new_color = pass_color * (BaseTint + Tint) * ShadowVisibility;
	new_color = vec4(linear_to_srbg(new_color.xyz), new_color.w);

    if(Outline)
    {
        new_color += vec4(pass_color.xyz, -1.0) * .5;
    }

	if(UseFog)
	{
		vec4 NewColor = mix(sky_color(), new_color, pass_visibility);
		out_colour = vec4(NewColor.xyz, new_color.w);
	}
	else
	{
		out_colour = vec4(new_color.xyz, new_color.w);
	}
	
	if (Outline)
    {
        vec3 unitToCamera = normalize( (inverse(_modelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - pass_position);
        float outlineDot = max(0.4, 1.0 - dot(pass_normal, unitToCamera));
        out_colour = outlineDot * ( cos(Time * (pass_position.x + pass_position.z) * 0.001) ) * OutlineColor * Alpha;
        out_position = vec4(0.0, 0.0, 0.0, 0.0);
        out_normal = vec4(0.0, 0.0, 0.0, 0.0);
    }
    else
    {
        // Ignore the gl_FragCoord.z since it causes issues with the water
        mat3 mat = mat3(transpose(inverse(_modelViewMatrix)));
        out_position = vec4( (_modelViewMatrix * vec4(pass_position, 1.0)).xyz * Alpha, 2.0);
        out_normal = vec4( mat * pass_normal, 1.0) * Alpha;
    }	
}
