#version 330 core

layout(location = 0)out vec4 FragColor; 

uniform sampler2D s_texture;
 
in vec2 v_texCoord;
in vec2 v_blurTexCoords[14];

void main()
{
    FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    FragColor += texture2D(s_texture, v_blurTexCoords[ 0])*0.0044299121055113265;
    FragColor += texture2D(s_texture, v_blurTexCoords[ 1])*0.00895781211794;
    FragColor += texture2D(s_texture, v_blurTexCoords[ 2])*0.0215963866053;
    FragColor += texture2D(s_texture, v_blurTexCoords[ 3])*0.0443683338718;
    FragColor += texture2D(s_texture, v_blurTexCoords[ 4])*0.0776744219933;
    FragColor += texture2D(s_texture, v_blurTexCoords[ 5])*0.115876621105;
    FragColor += texture2D(s_texture, v_blurTexCoords[ 6])*0.147308056121;
    FragColor += texture2D(s_texture, clamp(v_texCoord, vec2(.001,.001), vec2(.999,.999)) )*0.159576912161;
    FragColor += texture2D(s_texture, v_blurTexCoords[ 7])*0.147308056121;
    FragColor += texture2D(s_texture, v_blurTexCoords[ 8])*0.115876621105;
    FragColor += texture2D(s_texture, v_blurTexCoords[ 9])*0.0776744219933;
    FragColor += texture2D(s_texture, v_blurTexCoords[10])*0.0443683338718;
    FragColor += texture2D(s_texture, v_blurTexCoords[11])*0.0215963866053;
    FragColor += texture2D(s_texture, v_blurTexCoords[12])*0.00895781211794;
    FragColor += texture2D(s_texture, v_blurTexCoords[13])*0.0044299121055113265;
}