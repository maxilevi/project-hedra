#version 330 core

in vec2 UV;
uniform float Time;
uniform sampler2D Texture;
uniform sampler2D DuDvMap;

layout(location = 0)out vec4 OutColor;

void main(void){
    vec2 Distortion = (texture(DuDvMap, UV).rg * 2.0 - 1.0) * 0.1;

    vec2 TexCoords = vec2(clamp(UV.x + Distortion.x, 0.001, 0.999), clamp(1.0-(UV.y + Distortion.y), 0.001, 0.999));
    vec4 Color = texture(Texture, TexCoords);

    OutColor = Color;
}	