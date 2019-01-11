#version 330 core

layout(location = 0)out vec4 OutColor;

uniform vec4 Color;
uniform sampler2D BluePrint;

in vec2 UV;
in vec2 InScale;
in vec2 FixedPos;
in float Gradient;

const float width = 0.05;
const float edge = 0.025;

void main()
{
	if( Color.a < 0.0 )
		OutColor = texture(BluePrint, UV);
	else
		OutColor = vec4(mix(Color.rgb, Color.rgb + vec3(0.12, 0.12, 0.12), Gradient), texture(BluePrint, UV).a);
}
