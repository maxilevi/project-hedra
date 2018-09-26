#version 330 core

in vec2 TexCoords;

uniform sampler2D Position1;
uniform sampler2D Normal2;
uniform sampler2D Random3;
uniform mat4 Projection;
uniform int MSAASamples = 0;
uniform float Intensity = 1.0;
const vec3 samples[] = 
	vec3[](
        vec3( -0.94201624,  -0.39906216,  0.24188840),
        vec3(  0.94558609,  -0.76890725,  0.09418411),
        vec3( -0.094184101, -0.92938870,  0.76890725),
        vec3(  0.34495938,   0.29387760,  0.87912464),
        vec3( -0.91588581,   0.45771432,  0.29387760),
        vec3( -0.81544232,  -0.87912464,  0.75648379),
        vec3( -0.38277543,   0.27676845,  0.27676845),
        vec3(  0.97484398,   0.75648379,  0.99706507),
        vec3(  0.44323325,  -0.97511554,  0.19090188),
        vec3(  0.53742981,  -0.47373420,  0.81544232),
        vec3( -0.26496911,  -0.41893023,  0.79197514),
        vec3(  0.79197514,   0.19090188,  0.53742981),
        vec3( -0.24188840,   0.99706507,  0.14383161),
        vec3( -0.81409955,   0.91437590,  0.19984126),
        vec3(  0.19984126,   0.78641367,  0.24188840),
        vec3(  0.14383161,  -0.14100790,  0.94558609)
      );
const float sample_count = 12.0;
const float radius = 1.0;
const float bias = 0.0;

layout(location = 0) out vec4 Color;


float when_ge(float x, float y) {
  return  max(sign(x - y), 0.0);
}


void main() {

	vec3 fragPos = texture(Position1, TexCoords).xyz;
	vec3 normal = normalize(texture(Normal2, TexCoords).rgb);
	vec3 randomVec = texture(Random3, gl_FragCoord.xy / vec2(4.0, 4.0)).xyz * vec3(2.0, 2.0, 1.0) - vec3(1.0, 1.0, 0.0);

	vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
	vec3 bitangent = cross(normal, tangent);
	mat3 TBN = mat3(tangent, bitangent, normal);

    float occlusion = 0.0;
    for(int i = 0; i < sample_count; ++i) {
        vec3 sampl = TBN * samples[i]; // From tangent to view-spaaaaaace
        sampl = fragPos + sampl * radius;
        
        vec4 offset = vec4(sampl, 1.0);
        offset = Projection * offset; // from view to clip-spaaaaaace
        offset.xyz /= offset.w; // perspective divide
        offset.xyz = offset.xyz * 0.5 + 0.5; // transform to range 0.0 - 1.0
        
        float sampleDepth = texture(Position1, offset.xy).z; // Get depth value of kernel sample
        
        float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth));
        float final = when_ge(sampleDepth, sampl.z + bias) * rangeCheck;
        occlusion += final;
    }
    occlusion = (occlusion / sample_count);
    occlusion = max(0.0, occlusion -0.0);
    float occ = occlusion * Intensity * .6;
    Color = vec4(occ,occ,occ, 1.0);
}