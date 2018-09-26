#version 330 core

layout(location = 0)in vec3 InVertex;
layout(location = 1)in vec4 InColor;

out vec4 Color;

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

void main(){
	Color = InColor;
	vec3 Vertex = InVertex;
	float config_set = Fancy; //If configuration is set
	vec2 Unpacked = Unpack(InColor.a, int(2048.0));
	float Addon = config_set * ( cos(Time + Unpacked.y * 8.0) +0.8) * .85 * 0.7 * Unpacked.x * 1.2;

	float invert_uk = when_lt(Unpacked.y, 0.5);
	Vertex.x += invert_uk * Addon;
	Vertex.z -= invert_uk * Addon;
	Vertex.x -= not(invert_uk) * Addon;
	Vertex.z += not(invert_uk) * Addon;
	gl_Position = MVP * vec4(Vertex,1.0);
}