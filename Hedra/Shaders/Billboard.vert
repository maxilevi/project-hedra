#version 330 core
layout(location = 0)in vec2 in_vertex;

out vec2 uv;

uniform vec2 scale;
uniform vec3 position;
uniform vec3 camera_right;
uniform vec3 camera_up;


void main(void)
{
    /*
        vec3 worldspace =
            position
            + camera_right * in_vertex.x * scale.x
            + camera_up * in_vertex.y * scale.y;
        gl_Position = _modelViewProjectionMatrix * vec4(worldspace, 1.0);
        */
    gl_Position = _modelViewProjectionMatrix * vec4(position, 1.0);
    gl_Position /= gl_Position.w;
    gl_Position.xy += in_vertex * scale;
    gl_Position.xy -= in_vertex * scale * vec2(0.0, 0.0);
    uv = vec2((in_vertex.x + 1.0) / 2.0, 1.0 - (in_vertex.y + 1.0) / 2.0);
}