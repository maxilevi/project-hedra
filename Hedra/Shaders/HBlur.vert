#version 330 core
layout(location = 0)in vec2 position;
 
out vec2 v_texCoord;
out vec2 v_blurTexCoords[14];
 
uniform mat4 TransMatrix; 
uniform bool Flipped = false;
uniform float weight = 1.0;
 
void main()
{
    gl_Position = vec4(position, 0.0, 1.0);
    
    if(Flipped)
    	v_texCoord = vec2((position.x+1.0)/2.0, (position.y+1.0)/2.0);
    else
    	v_texCoord = vec2((position.x+1.0)/2.0, 1.0-(position.y+1.0)/2.0);
    
    v_blurTexCoords[ 0] = v_texCoord + vec2(-0.028, 0.0) * weight;
    v_blurTexCoords[ 1] = v_texCoord + vec2(-0.024, 0.0) * weight;
    v_blurTexCoords[ 2] = v_texCoord + vec2(-0.020, 0.0) * weight;
    v_blurTexCoords[ 3] = v_texCoord + vec2(-0.016, 0.0) * weight;
    v_blurTexCoords[ 4] = v_texCoord + vec2(-0.012, 0.0) * weight;
    v_blurTexCoords[ 5] = v_texCoord + vec2(-0.008, 0.0) * weight;
    v_blurTexCoords[ 6] = v_texCoord + vec2(-0.004, 0.0) * weight;
    v_blurTexCoords[ 7] = v_texCoord + vec2( 0.004, 0.0) * weight;
    v_blurTexCoords[ 8] = v_texCoord + vec2( 0.008, 0.0) * weight;
    v_blurTexCoords[ 9] = v_texCoord + vec2( 0.012, 0.0) * weight;
    v_blurTexCoords[10] = v_texCoord + vec2( 0.016, 0.0) * weight;
    v_blurTexCoords[11] = v_texCoord + vec2( 0.020, 0.0) * weight;
    v_blurTexCoords[12] = v_texCoord + vec2( 0.024, 0.0) * weight;
    v_blurTexCoords[13] = v_texCoord + vec2( 0.028, 0.0) * weight;
}