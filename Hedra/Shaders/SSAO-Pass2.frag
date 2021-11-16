#version 330 core

in vec2 TexCoords;

layout(location = 0)out vec4 OutColor;

uniform sampler2D SSAOInput;


void main() {

    vec2 UVCoords = vec2(TexCoords.x, 1.0-TexCoords.y);
    vec2 texelSize = 1.0 / vec2(textureSize(SSAOInput, 0));
    float texel = texelSize.x;

    float AO = 0.0;
    AO += texture(SSAOInput, UVCoords + vec2(texel * 3.0, 0.0)).r;
    AO += texture(SSAOInput, UVCoords + vec2(texel * 2.0, 0.0)).r;
    AO += texture(SSAOInput, UVCoords + vec2(texel * 1.0, 0.0)).r;
    AO += texture(SSAOInput, UVCoords + vec2(0.0, 0.0)).r;
    AO += texture(SSAOInput, UVCoords + vec2(-texel * 1.0, 0.0)).r;
    AO += texture(SSAOInput, UVCoords + vec2(-texel * 2.0, 0.0)).r;
    AO += texture(SSAOInput, UVCoords + vec2(-texel * 3.0, 0.0)).r;

    AO = AO / 7.0;
    OutColor = vec4(AO, AO, AO, 1.0);
}