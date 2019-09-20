#version 330 core

layout(location = 0) out vec4 out_color;
layout(location = 1) out vec4 out_position;
layout(location = 2) out vec4 out_normal;

in vec3 pass_uv;
uniform samplerCube map;
uniform vec4 color_multiplier;

void main()
{
	out_color = texture(map, pass_uv) * color_multiplier;
    out_position = vec4(0.0, 0.0, 0.0, gl_FragCoord.z);
	out_normal = vec4(0.0, 0.0, 0.0, 0.0);
} 