#version 330 compatibility

in vec2 TexCoords;

layout(location = 0)out vec4 OutColor;

uniform sampler2D SSAOInput;
uniform sampler2DMS ColorInput;
uniform int MSAASamples = 0;

const int blursize = 2;

void main() {
	ivec2 Resolution = ivec2(textureSize(ColorInput));
	vec2 UVCoords = vec2(TexCoords.x, 1.0-TexCoords.y);
	vec2 texelSize = 1.0 / vec2(textureSize(SSAOInput, 0));
    vec2 hlim = vec2(float(-blursize) * 0.5 + 0.5);
	float result = 0.0;
	for (int x = -blursize; x < blursize; ++x) {
		for (int y = -blursize; y < blursize; ++y) {
			vec2 offset = (hlim + vec2(float(x), float(y))) * texelSize;
			result += texture(SSAOInput, UVCoords + offset).r;
		}
	}

	float AO = result / (blursize * 2.0 * blursize * 2.0);
	//Recreate multisample
	vec4 AvgColor = vec4(0.0, 0.0, 0.0, 0.0);

	ivec2 res = ivec2(TexCoords * Resolution);
	for(int i = 0; i < MSAASamples; i++){
		AvgColor += texelFetch(ColorInput, res, i);
	}
	AvgColor /= MSAASamples;

	OutColor = vec4(AvgColor.xyz, 1.0) - vec4(AO,AO,AO,0.0);
}