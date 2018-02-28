#version 330 compatibility

uniform vec4 TopColor;
uniform vec4 BotColor;
uniform float Height;

layout(location = 0)out vec4 OutColor; 
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

in vec4 Vertex;
in vec3 InNormal;

void main()
{   
    OutColor = vec4( mix(BotColor, TopColor, (gl_FragCoord.y / Height) - .25)  );
	OutPosition = vec4(0.0, 0.0, 0.0, 0.0);
	OutNormal = vec4(0.0, 0.0, 0.0, 0.0);
}