uniform sampler2D Texture;
uniform vec2 TexelSize;
in vec2 TexCoords;
layout(location = 0)out vec4 FragColor;
const float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

vec3 blur(vec2 direction) {
    vec3 result = texture(Texture, TexCoords).rgb * weight[0];
    for (int i = 1; i < 5; ++i)
    {
        result += texture(Texture, TexCoords + direction * TexelSize.x * i).rgb * weight[i];
        result += texture(Texture, TexCoords - direction * TexelSize.x * i).rgb * weight[i];
    }
    return result;
}