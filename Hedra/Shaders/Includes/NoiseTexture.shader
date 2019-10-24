// uniform sampler2D noiseTexture;
uniform sampler3D noiseTexture;

float CalculateNoiseTex(vec3 wNormal, vec3 coords) {
    return texture(noiseTexture, coords).r * 1.25;
    /*
    vec3 blending = abs(wNormal);
    blending = normalize(max(blending, 0.00001));
    float b = (blending.x + blending.y + blending.z);
    blending /= vec3(b, b, b);
    
    vec4 xaxis = texture2D(noiseTexture, coords.yz);
    vec4 yaxis = texture2D(noiseTexture, coords.xz);
    vec4 zaxis = texture2D(noiseTexture, coords.xy);
    return (xaxis * blending.x + yaxis * blending.y + zaxis * blending.z).r * 1.0;
    */
}