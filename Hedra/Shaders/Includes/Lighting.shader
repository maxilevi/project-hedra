struct PointLight
{
    vec3 Position;
    vec3 Color;
    float Radius;
};

layout(std140) uniform LightSettings {
    PointLight Lights[32];
    int LightCount;
    vec3 LightPosition;
    vec3 LightColor;
};

/* Don't delete these default values */

const vec3 RimColor = vec3(1.0, 1.0, 1.0);
const float Damper = 64;
const float Reflectivity = 0.05;
float Ambient = 0.0;

vec3 DiffuseModel(vec3 unitToLight, vec3 unitNormal, vec3 LColor)
{
    return LColor * max(dot(unitNormal, unitToLight), Ambient);
}

vec4 rim(vec3 InputColor, vec3 FLightColor, vec3 unitToCamera, vec3 unitNormal)
{
    float rim = 1.0 - max(dot(unitToCamera, unitNormal) * 1.0, 0.0);
    return vec4(InputColor * 0.05 * smoothstep(0.6, 1.0, rim) * max(FLightColor, vec3(.15, .15, .15)), 0.0);
}

vec4 diffuse(vec3 unitToLight, vec3 unitNormal, vec3 FullLightColor) {
    return vec4(
    DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal, FullLightColor) * 0.2
    + DiffuseModel(vec3(-1.0, 0.0, 0.0), unitNormal, FullLightColor) * 0.2
    + DiffuseModel(vec3(0.0, 0.0, -1.0), unitNormal, FullLightColor) * 0.2
    + DiffuseModel(vec3(0.0, 0.0, 1.0), unitNormal, FullLightColor) * 0.2
    + DiffuseModel(vec3(0.0, -1.0, 0.0), unitNormal, FullLightColor) * 0.025
    + DiffuseModel(unitToLight, unitNormal, FullLightColor) * 0.75,
    1.0);
}

vec3 calculate_lights(vec3 LightColor, vec3 Vertex, float radius_multiplier)
{
    float average_color = (LightColor.r + LightColor.g + LightColor.b) * .33;
    vec3 light_color = vec3(0.0, 0.0, 0.0);
    for (int i = int(0.0); i < LightCount; i++)
    {
        float real_radius = Lights[i].Radius * 1.0 * radius_multiplier;
        float att = pow(1.0 - (min(length(Lights[i].Position.xyz - Vertex) / real_radius, 1.0)), 2.0);
        light_color += Lights[i].Color * att;// * (1.0 - average_color); 
    }
    return clamp(light_color, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));
}

vec3 calculate_lights(vec3 LightColor, vec3 Vertex)
{
    return calculate_lights(LightColor, Vertex, 1.0);
}