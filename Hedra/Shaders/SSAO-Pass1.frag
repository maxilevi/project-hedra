#version 330 core

in vec2 TexCoords;

uniform sampler2D Position1;
uniform sampler2D Normal2;
uniform sampler2D Random3;
uniform mat4 Projection;
uniform int MSAASamples = 0;
uniform float Intensity = 1;
const vec3 samples[] =
vec3[](
vec3(0.001232165, -0.0270802, 0.01733841),
vec3(0.03632024, 0.02018879, 0.07996441),
vec3(-0.01457156, -0.03273875, 0.0329445),
vec3(-0.03583886, 0.04031469, 0.04077784),
vec3(-0.02052904, 0.06830341, 0.01759148),
vec3(0.02189388, -0.03970042, 0.002819346),
vec3(0.01182553, 0.02490917, 0.009550216),
vec3(-0.06765796, -0.05821282, 0.05377859),
vec3(-0.01240797, 0.01793554, 0.01966679),
vec3(-0.01747762, 0.01873986, 0.01601099),
vec3(0.02996103, 0.004102552, 0.02064461),
vec3(0.0742167, 0.06361989, 0.03968823),
vec3(-0.03809038, 0.01483359, 0.003034199),
vec3(0.03298461, 0.02536719, 0.002999582),
vec3(0.07569861, -0.04974895, 0.04882696),
vec3(-0.01437303, -0.006748787, 0.02186127),
vec3(0.04187566, 0.02649674, 0.02110886),
vec3(-0.0665702, 0.04533948, 0.07295627),
vec3(0.0006135539, 0.01300918, 0.01214791),
vec3(0.07381942, 0.08960811, 0.08996201),
vec3(-0.03352926, -0.002765586, 0.08056606),
vec3(-0.1017952, -0.0683318, 0.02942608),
vec3(0.01575614, -0.01403853, 0.009456188),
vec3(-0.09968454, 0.03786943, 0.000389035),
vec3(0.05017738, -0.02676225, 0.05247791),
vec3(0.04266685, -0.0727782, 0.04993279),
vec3(-0.07895226, -0.03882535, 0.1718772),
vec3(0.03344429, 0.0430752, 0.02993378),
vec3(0.1087467, 0.1597256, 0.07214108),
vec3(-0.1032015, -0.06156969, 0.008357415),
vec3(-0.004013889, 0.001110614, 0.01109257),
vec3(-0.108799, -0.1625814, 0.1865968),
vec3(0.1042297, -0.03788646, 0.03563374),
vec3(-0.07795693, -0.07180866, 0.01050573),
vec3(0.2080567, 0.1388275, 0.0306961),
vec3(-0.03150545, -0.04231871, 0.2889087),
vec3(0.04018108, -0.09912898, 0.04396231),
vec3(-0.07355242, -0.1588484, 0.1100745),
vec3(-0.02787757, -0.03722453, 0.06061712),
vec3(0.01062292, -0.01851761, 0.0182702),
vec3(0.1135921, 0.1385773, 0.1371577),
vec3(0.1210116, -0.2276528, 0.05864519),
vec3(0.04680443, 0.1102613, 0.0921248),
vec3(-0.2780987, -0.1184591, 0.3726938),
vec3(0.08298305, 0.1148502, 0.06392063),
vec3(0.2711694, -0.1848757, 0.1911567),
vec3(0.4379044, 0.2580144, 0.03484287),
vec3(-0.1487105, 0.2010763, 0.1884311),
vec3(0.1254581, -0.2077282, 0.2150043),
vec3(0.1229774, -0.2279858, 0.09297559),
vec3(0.1840767, 0.1551562, 0.1363855),
vec3(-0.02095627, -0.5800143, 0.2388239),
vec3(-0.2102789, -0.4020461, 0.1841748),
vec3(-0.3299322, 0.2370198, 0.09226206),
vec3(-0.091075, -0.5068243, 0.3586721),
vec3(-0.4162733, -0.3439335, 0.2374147),
vec3(-0.02975674, -0.02421854, 0.0247276),
vec3(-0.03762399, -0.1283088, 0.3211406),
vec3(0.2701626, 0.7200997, 0.3354966),
vec3(0.2687605, -0.7489821, 0.181919),
vec3(-0.02965994, -0.1178195, 0.1089969),
vec3(-0.06047333, -0.789763, 0.4075724),
vec3(0.378882, -0.224533, 0.05231125),
vec3(0.5192277, -0.3338073, 0.240924)
);
const float sample_count = 64.0;
const float radius = 1.0;
const float bias = 0.05;

layout(location = 0) out vec4 Color;


float when_ge(float x, float y) {
    return max(sign(x - y), 0.0);
}


void main()
{
    /* Early assignment to allow early returns*/
    Color = vec4(0.0, 0.0, 0.0, 1.0);

    vec3 fragPos = texture(Position1, TexCoords).xyz;
    vec4 normalSample = texture(Normal2, TexCoords);
    float isWater = normalSample.a;
    if (isWater > 0.0) return;

    vec3 normal = normalize(normalSample.rgb);
    vec3 randomVec = texture(Random3, gl_FragCoord.xy / vec2(4.0, 4.0)).xyz * vec3(2.0, 2.0, 1.0) - vec3(1.0, 1.0, 0.0);

    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN = mat3(tangent, bitangent, normal);

    float occlusion = 0.0;
    for (int i = 0; i < sample_count; ++i) {
        vec3 sampl = TBN * samples[i];// From tangent to view-spaaaaaace
        sampl = fragPos + sampl * radius;

        vec4 offset = vec4(sampl, 1.0);
        offset = Projection * offset;// from view to clip-spaaaaaace
        offset.xyz /= offset.w;// perspective divide
        offset.xyz = offset.xyz * 0.5 + 0.5;// transform to range 0.0 - 1.0

        float sampleDepth = texture(Position1, offset.xy).z;// Get depth value of kernel sample

        float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth));
        occlusion += (sampleDepth >= sampl.z + bias ? 1.0 : 0.0) * rangeCheck;
    }
    occlusion = (occlusion / sample_count);
    occlusion = max(0.0, occlusion -0.0);
    float occ = occlusion * Intensity * .75;
    Color = vec4(occ, occ, occ, 1.0);
}