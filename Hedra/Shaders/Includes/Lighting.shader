﻿
struct PointLight
{
    vec3 Position;
    vec3 Color;
    float Radius;
};

uniform PointLight Lights[32];
uniform int LightCount;
uniform vec4 AreaColors[8];
uniform vec4 AreaPositions[8];
uniform int AreaCount = 0;

/* Don't delete these default values */
uniform vec3 LightPosition = vec3(-500.0, 800.0, 0.0);
uniform vec3 LightColor = vec3(0.0, 0.0, 0.0);

const vec3 RimColor = vec3(1.0, 1.0, 1.0);
float Damper = 64;
float Reflectivity = 0.05;
float Ambient = 0.0;

vec3 DiffuseModel(vec3 unitToLight, vec3 unitNormal, vec3 LColor)
{	
	return LColor * max(dot(unitNormal, unitToLight), Ambient);
}

vec4 rim(vec3 InputColor, vec3 FLightColor, vec3 unitToCamera, vec3 unitNormal)
{
    float rim = 1.0 - max(dot(unitToCamera, unitNormal) * 1.0, 0.0);
    return vec4(InputColor * 0.05 * smoothstep(0.6, 1.0, rim) * max(FLightColor, vec3(.15,.15,.15)), 0);
}

vec4 diffuse(vec3 unitToLight, vec3 unitNormal, vec3 FullLightColor) {
	return vec4(
		DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal, FullLightColor) * 0.25
		+ DiffuseModel(vec3(-1.0, 0.0, 0.0), unitNormal, FullLightColor) * 0.25
		+ DiffuseModel(vec3(0.0, 0.0, -1.0), unitNormal, FullLightColor) * 0.25
		+ DiffuseModel(vec3(0.0, 0.0, 1.0), unitNormal, FullLightColor) * 0.25
		+ DiffuseModel(unitToLight, unitNormal, FullLightColor) * 0.75,
	1.0);
}

vec3 calculate_lights(vec3 LightColor, vec3 Vertex)
{
	float average_color = (LightColor.r + LightColor.g + LightColor.b) * .33;
	vec3 light_color = vec3(0.0, 0.0, 0.0);
	for(int i = int(0.0); i < LightCount; i++)
	{
	    float real_radius = Lights[i].Radius * 1.0;
		float att = pow(1.0 - (min(length(Lights[i].Position.xyz - Vertex) / real_radius, 1.0)), 1.0);
		light_color += Lights[i].Color * att * (1.0 - average_color); 
	}
	return clamp(light_color, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));
}

vec4 apply_highlights(vec4 linear_color, vec3 Position)
{
    vec4 new_color = linear_color;
    for(int i = int(0.0); i < AreaCount; i++)
    {
        new_color = mix(
            new_color,
            AreaColors[i],
            clamp(
                1.5 * (1.0 - (length(AreaPositions[i].xyz - Position) / AreaPositions[i].w)),
                0.0,
                1.0
            )
        );
    }
    return new_color;
}