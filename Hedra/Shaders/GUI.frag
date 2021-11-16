#version 330 core

in vec2 UV;

uniform sampler2D Texture;
uniform sampler2D Background;
uniform sampler2D Mask;
uniform bool Flipped;
uniform float Opacity;
uniform bool Grayscale;
uniform vec4 Tint;
uniform bool UseMask;

layout(location = 0) out vec4 OutColor;

float luma(vec3 color)
{
    return 0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b;
}

void main(void)
{
    vec2 TexCoords;

    if (Flipped)
    {
        TexCoords = vec2(clamp(UV.x, 0.001, 0.999), clamp(1.0-UV.y, 0.001, 0.999));
    }
    else
    {
        TexCoords = vec2(clamp(UV.x, 0.001, 0.999), clamp(UV.y, 0.001, 0.999));
    }

    vec4 Color = texture2D(Texture, TexCoords) * vec4(1.0, 1.0, 1.0, Opacity);


    vec4 Highlight = texture2D(Background, TexCoords)  * vec4(1.0, 1.0, 1.0, Opacity);
    OutColor = Color + vec4(Highlight.rgb, 0.0);

    if (Grayscale)
    {
        float Scale = (OutColor.r + OutColor.g + OutColor.b) / 3.0;
        OutColor = vec4(Scale, Scale, Scale, OutColor.a) * Tint;
    }
    if (UseMask)
    {
        OutColor.a = texture(Mask, TexCoords).a * OutColor.a;
    }
    /*
    * Render a redline in the middle of screen
    if(abs(gl_FragCoord.x - 960.0) < 1.0){
        OutColor = vec4(1.0, 0.0, 0.0, 1.0);
    }*/
}	