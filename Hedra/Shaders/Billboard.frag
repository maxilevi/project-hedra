#version 330 core

layout(location = 0) out vec4 out_color;

uniform sampler2D texture_sampler;
uniform float opacity;
in vec2 uv;

void main()
{
    vec4 texture_sample = texture(texture_sampler, uv);
    out_color = vec4(texture_sample.rgb, texture_sample.a * opacity);
}
