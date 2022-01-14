#version 330 core

in vec2 TexCoords;

layout(location = 0)out vec4 OutColor;

uniform sampler2D SSAOInput;
uniform sampler2D ColorInput;
const float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);
const vec2 direction = vec2(0.0, 1.0);


void main() {

    vec2 UVCoords = vec2(TexCoords.x, 1.0-TexCoords.y);
    vec2 texelSize = 1.0 / vec2(textureSize(SSAOInput, 0));
    float result = 0.0;
    int size = 2;
    for (int x = -size; x < size; ++x)
    {
        for (int y = -size; y < size; ++y)
        {
            vec2 offset = vec2(float(x), float(y)) * texelSize;
            result += texture(SSAOInput, UVCoords + offset).r;
        }
    }
    float AO = result / float((2.0 * size) * (2.0 * size));
    /*vec2 texelSize = 1.0 / vec2(textureSize(SSAOInput, 0));

    float AO = texture(SSAOInput, UVCoords).r * weight[0];
    for (int i = 1; i < 5; ++i)
    {
        AO += texture(SSAOInput, UVCoords + direction * texelSize * i).r * weight[i];
        AO += texture(SSAOInput, UVCoords - direction * texelSize * i).r * weight[i];
    }*/
    OutColor = vec4(texture(ColorInput, TexCoords).xyz, 1.0) * vec4(AO, AO, AO, 1.0);
}