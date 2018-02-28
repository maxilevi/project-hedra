#version 330 compatibility

layout(location = 0) out vec4 Depth;

in vec4 Color;

void main(){
	if(Color.a < 0.0) discard;
	
	Depth = vec4(gl_FragCoord.z, 0.0, 0.0, 1.0);
}