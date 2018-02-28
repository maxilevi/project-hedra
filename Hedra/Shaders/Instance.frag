#version 330 compatibility

in vec4 Color;
in float Visibility;
in float Height;
in vec4 BotColor;
in vec4 TopColor;

layout(location = 0)out vec4 OutColor;

void main(){

	vec4 SkyColor = vec4( mix(BotColor, TopColor, (gl_FragCoord.y / Height)) );
	vec4 NewColor = mix(SkyColor, Color, Visibility);

	OutColor = NewColor;	
}
