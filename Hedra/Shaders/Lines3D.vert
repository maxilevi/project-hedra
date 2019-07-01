#version 330 core

out vec4 PassColor;
out float Visibility;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;

layout(location = 0)in vec3 InVertex;
layout(location = 1)in vec4 InColor;

uniform vec3 PlayerPosition;
layout(std140) uniform FogSettings {
    vec4 U_BotColor;
    vec4 U_TopColor;
    float MinDist;
    float MaxDist;
    float U_Height;
};

void main()
{
    pass_height = U_Height;
    pass_botColor = U_BotColor;
    pass_topColor = U_TopColor;

    float DistanceToCamera = length(vec3(PlayerPosition - InVertex.xyz).xz);
    Visibility = clamp((MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
    PassColor = InColor;
    
    gl_Position = _modelViewProjectionMatrix * vec4(InVertex, 1.0);
}