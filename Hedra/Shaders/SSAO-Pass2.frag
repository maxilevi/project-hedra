#version 330 core

in vec2 TexCoords;

layout(location = 0)out vec4 OutColor;


uniform sampler2D SSAOInput;
const float weight[5] = float[] (0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);
const vec2 direction = vec2(1.0, 0.0);

void main() {

    vec2 UVCoords = vec2(TexCoords.x, 1.0-TexCoords.y);
    vec2 texelSize = 1.0 / vec2(textureSize(SSAOInput, 0));

    float AO = texture(SSAOInput, UVCoords).r;// * weight[0];
    /*for (int i = 1; i < 5; ++i)
    {
        AO += texture(SSAOInput, UVCoords + direction * texelSize * i).r * weight[i];
        AO += texture(SSAOInput, UVCoords - direction * texelSize * i).r * weight[i];
    }*/
    OutColor = vec4(AO, AO, AO, 1.0);
}