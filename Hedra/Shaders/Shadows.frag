#version 330 core

layout(location = 0) out vec4 Depth;

in float Alpha;

void main()
{
	if (Alpha < 0.0) discard;	
	Depth = vec4(gl_FragCoord.z, 0.0, 0.0, 1.0);
}