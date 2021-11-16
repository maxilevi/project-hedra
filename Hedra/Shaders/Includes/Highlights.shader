uniform vec4 AreaColors[8];
uniform vec4 AreaPositions[8];
uniform int AreaCount = 0;

vec4 apply_highlights(vec4 linear_color, vec3 Position)
{
    vec4 new_color = linear_color;
    for (int i = int(0.0); i < AreaCount; i++)
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