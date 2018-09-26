#version 120

uniform sampler2D s_texture;
 
varying vec2 v_texCoord;
varying vec2 v_blurTexCoords[14];
vec2 BlurTexCoords[14];

void main()
{
	//BlurTexCoords = vec2[14];
	for(int i = 0; i < v_blurTexCoords.length(); i++){
		BlurTexCoords[i] = clamp(v_blurTexCoords[i], vec2(.001,.001), vec2(.999,.999));
	}

    gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    gl_FragColor += texture2D(s_texture, BlurTexCoords[ 0])*0.0044299121055113265;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[ 1])*0.00895781211794;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[ 2])*0.0215963866053;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[ 3])*0.0443683338718;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[ 4])*0.0776744219933;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[ 5])*0.115876621105;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[ 6])*0.147308056121;
    gl_FragColor += texture2D(s_texture, clamp(v_texCoord, vec2(.001,.001), vec2(.999,.999)) )*0.159576912161;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[ 7])*0.147308056121;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[ 8])*0.115876621105;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[ 9])*0.0776744219933;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[10])*0.0443683338718;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[11])*0.0215963866053;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[12])*0.00895781211794;
    gl_FragColor += texture2D(s_texture, BlurTexCoords[13])*0.0044299121055113265;
}