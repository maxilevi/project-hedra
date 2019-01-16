#version 330 core

layout(location = 0)in vec3 InVertex;
layout(location = 1)in vec4 InColor;

out float Alpha;

uniform mat4 MVP;
uniform float Time;
uniform float Fancy = 0.0;

vec2 Unpack(float inp, int prec)
{
    vec2 outp = vec2(0.0, 0.0);

    outp.y = mod(inp, prec);
    outp.x = floor(inp / prec);

    return outp / (prec - 1.0);
}

float not(float x){
	return 1.0 - x;
}

float when_lt(float x, float y) {//smaller
  return max(sign(y - x), 0.0);
}

const int prec = int(2048.0);

void main()
{
	vec3 Vertex = InVertex;
	vec2 Unpacked = Unpack(InColor.a, prec);
	float Addon = Fancy * ( cos(Time + Unpacked.y * 8.0) + 0.8) * 0.715 * Unpacked.x;

	float invert_uk = when_lt(Unpacked.y, 0.5);
	Vertex.x += invert_uk * Addon;
	Vertex.z -= invert_uk * Addon;
	Vertex.x -= not(invert_uk) * Addon;
	Vertex.z += not(invert_uk) * Addon;
	gl_Position = MVP * vec4(Vertex, 1.0);
	Alpha = InColor.a;
}