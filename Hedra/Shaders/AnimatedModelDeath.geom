#version 330 core

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

in float in_time[];
in vec4 pass_colors[];
in vec3 pass_positions[];
in vec3 pass_normals[];
in float pass_heights[];
in vec3 base_vertex_positions[];
in vec4 pass_botColors[];
in vec4 pass_topColors[];
in vec4 pass_coordss[];


out vec4 pass_color;
out vec3 pass_normal;
out vec3 pass_position;
out float pass_visibility;
out float pass_height;


out vec3 base_vertex_position;
out vec4 pass_botColor;
out vec4 pass_topColor;
out vec4 pass_coords;

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
        
        pass_height = pass_heights[i];
        base_vertex_position = base_vertex_positions[i];
        pass_botColor = pass_botColors[i];
        pass_topColor = pass_topColors[i];
        pass_coords = pass_coordss[i];
        pass_visibility = 1.0;
        
        gl_Position = projectionViewMatrix * (modelViewSpace+addon);
        EmitVertex();
    }
    EndPrimitive();
}