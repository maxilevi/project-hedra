#version 330 core

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

in float in_time[];
in vec4 pass_colors[];
in vec3 pass_positions[];
in vec3 pass_normals[];
out vec4 pass_color;
out vec3 pass_normal;
out vec3 pass_position;
out float pass_visibility;

uniform mat4 projectionViewMatrix;
uniform mat4 viewMatrix;
uniform float disposeTime;

void main()
{
    vec3 ab = gl_in[1].gl_Position.xyz - gl_in[0].gl_Position.xyz;
    vec3 ac = gl_in[2].gl_Position.xyz - gl_in[0].gl_Position.xyz;

    vec3 faceNormal = normalize(cross(ac, ab));

    for (int i = 0; i < gl_in.length(); i++)
    {
        vec4 addon = vec4(vec3(0.0, 1.0, 0.0) * (abs(faceNormal.y)+.1) * disposeTime * 2.0 + pass_normals[i] * disposeTime * 0.5, 0.0);
        vec4 modelViewSpace = vec4(pass_positions[i].xyz, 1.0);

        pass_color = vec4(pass_colors[i].xyz, 1.0 - disposeTime * .25);
        pass_normal = pass_normals[i];
        pass_position = (viewMatrix * vec4(pass_positions[i], 1.0) + addon).xyz;
        pass_visibility = 1.0;
        gl_Position = projectionViewMatrix * (modelViewSpace+addon);
        EmitVertex();
    }
    EndPrimitive();
}