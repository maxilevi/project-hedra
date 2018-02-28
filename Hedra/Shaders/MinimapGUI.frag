#version 330 compatibility

in vec2 UV;

uniform sampler2D Texture;
uniform sampler2D Fill;
uniform vec4 Color;
uniform int Flipped;
uniform float Opacity;
uniform bool Grayscale;
uniform vec4 Tint;
uniform int AntiAlias;
uniform vec2 size;

layout(location = 0) out vec4 OutColor;

float luma(vec3 color){
	return 0.2126 * color.r + 0.7152 * color.g + 0.0722 * color.b;
}

void main(void){
	vec2 TexCoords;
	if(Flipped == int(1.0))
		TexCoords = vec2( clamp(1.0-UV.x, 0.001, 0.999), clamp(1.0-UV.y, 0.001, 0.999) );
	else
		TexCoords = vec2( clamp(1.0-UV.x, 0.001, 0.999), clamp(UV.y, 0.001, 0.999) );
	
	vec4 Color;
	vec4 texel = texture2D(Texture, TexCoords);
	if(texel.r > .3 && texel.g < 0.7 && texel.b > .3){
		 Color = texture2D(Fill, TexCoords)  * vec4(1.0, 1.0, 1.0, Opacity);
		 Color.a = texel.a;
	}else{
		 Color = texel * vec4(1.0, 1.0, 1.0, Opacity);
	}
	OutColor = Color;
	
	if(Grayscale){
		float Scale = (OutColor.r + OutColor.g + OutColor.b) / 3.0;
		OutColor = vec4(Scale, Scale, Scale, OutColor.a) * Tint;
	}
	
	
}	