#version 330 compatibility

layout(location = 0) in vec2 InVertex;

uniform vec3 Offset;
uniform mat4 ViewAlign;
uniform vec2 Scale;
out vec2 UVs;

void main(){
	vec4 Position = ViewAlign * vec4(InVertex * Scale,0.0, 1.0) + vec4(Offset,0.0);
	gl_Position = gl_ModelViewProjectionMatrix * Position;

    UVs = vec2((InVertex.x+1.0)/2.0, 1.0 - (InVertex.y+1.0)/2.0);

}