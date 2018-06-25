#version 330 core

layout(location = 0) out vec4 out_color;
layout(location = 1) out vec4 out_position;
layout(location = 2) out vec4 out_normal;

in vec2 pass_uv;
uniform sampler2D star_texture;

void main()
{
	out_color = texture(star_texture, pass_uv * 3.0);
    out_position = vec4(0.0, 0.0, 0.0, gl_FragCoord.z);
	out_normal = vec4(0.0, 0.0, 0.0, 1.0);
} 