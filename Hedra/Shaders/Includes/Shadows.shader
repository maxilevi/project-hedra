
const float shadow_multiplier = 0.75;

uniform sampler2DShadow ShadowTex;

float simple_apply_shadows(vec4 pass_coords, float shadow_bias)
{
    vec4 ShadowCoords = pass_coords * vec4(.5,.5,.5, 1.0) + vec4(.5,.5,.5, 0.0);			
	if(ShadowCoords.z < 1.0)
	{
		float shadow = 0.0;
		float samples = 0.0;
        vec2 texelSize = 1.0 / textureSize(ShadowTex, 0);
        for(int x = 0; x < 1; ++x)
        {
            for(int y = 0; y < 1; ++y)
            {
                shadow += 1.0 - texture(ShadowTex, vec3(ShadowCoords.xy + vec2(x, y) * texelSize, ShadowCoords.z - shadow_bias));
                samples += 1.0;
            }
        }
        shadow /= samples;
        return 1.0 - shadow * shadow_multiplier * pass_coords.w;
	}
}