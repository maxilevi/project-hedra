#version 330 compatibility

layout(location = 0) in vec3 InVertex;
layout(location = 1) in vec4 InColor;
layout(location = 2) in vec3 InNormal;

uniform vec3 LightPosition = vec3(-500.0, 800.0, 0.0);

uniform vec3 Point;
uniform vec3 LocalRotationPoint;
uniform mat4 TransMatrix;
uniform mat4 LocalRotation;
uniform vec3 LocalPosition;
uniform vec3 BeforeLocalRotation;
uniform vec3 AnimationPosition;
uniform mat4 AnimationRotation;
uniform vec3 AnimationRotationPoint;
uniform vec3 TransPos;
uniform float Time;
uniform vec3 Scale;
uniform vec3 BakedPosition;
uniform mat4 ShadowMVP;
uniform float ShadowDistance;
uniform bool UseShadows;
uniform mat4 Matrix;
uniform bool Outline;
const float ShadowTransition = 20.0;


out vec4 Color;
out float Visibility;
out float Height;
out vec4 BotColor;
out vec4 TopColor;
out vec3 InPos;
out vec3 InNorm;
out vec4 Coords;
out vec3 LightDir;
out vec3 PointDiffuse;
out vec3 vertex_position;

layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

struct PointLight
{
    vec3 Position;
    vec3 Color;
};

uniform PointLight Lights[8];

vec3 TransformNormal(vec3 norm, mat4 invMat);

uniform vec3 PlayerPosition;
uniform bool UseFog = true;
uniform vec3 LightColor = vec3(1.0, 1.0, 1.0);
const vec3 RimColor = vec3(0.2, 0.2, 0.2);
const float Damper = 32.0;
const float Reflectivity = .25;
const float gamma = 1.0/0.8;

vec3 DiffuseModel(vec3 unitToLight, vec3 unitNormal, vec3 LColor){	
	float Brightness = max(dot(unitNormal, unitToLight), 0.125);
	if(UseFog)
		return (Brightness * LColor );
	else
		return (Brightness * vec3(1.0,1.0,1.0) );
}

void main(){
	Height = U_Height;
	BotColor = U_BotColor;
	TopColor = U_TopColor;


	float realScale = Scale + (Outline) ? 0.25 : 0.0;
	vec4 Vertex = vec4((InVertex + BakedPosition) * realScale - BakedPosition, 1.0);
	
	Vertex += vec4(AnimationRotationPoint, 0.0);
	Vertex = AnimationRotation * Vertex;
	Vertex -= vec4(AnimationRotationPoint, 0.0);
	
	Vertex += vec4(BeforeLocalRotation,0.0);
	Vertex += vec4(LocalRotationPoint, 0.0);
	Vertex = LocalRotation * Vertex;
	Vertex -= vec4(LocalRotationPoint,0.0);
	
	Vertex = Matrix * Vertex;
	
	Vertex += vec4(AnimationPosition, 0.0);

	Vertex += vec4(Point,0.0);
	Vertex = TransMatrix * Vertex;
	Vertex += vec4(TransPos, 0.0);
	Vertex -= vec4(Point, 0.0);
	
	Vertex += vec4(LocalPosition,0.0);
	
	gl_Position = gl_ModelViewProjectionMatrix * Vertex;
	
	//Fog
	float DistanceToCamera = length(vec3(PlayerPosition - Vertex.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	
	vec3 SurfaceNormal = InNormal;
	
	SurfaceNormal += AnimationRotationPoint;
	SurfaceNormal = TransformNormal(SurfaceNormal, AnimationRotation);
	SurfaceNormal -= AnimationRotationPoint;
	
	SurfaceNormal += Point;
	SurfaceNormal = TransformNormal(SurfaceNormal, TransMatrix);
	SurfaceNormal -= Point;
	
	SurfaceNormal += LocalRotationPoint;
	SurfaceNormal = TransformNormal(SurfaceNormal, LocalRotation);
	SurfaceNormal -= LocalRotationPoint;
	
	SurfaceNormal = TransformNormal(SurfaceNormal, Matrix);
	
	//Lighting
	vec3 unitNormal = normalize(SurfaceNormal);
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(gl_ModelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - Vertex.xyz);

	vec3 FLightColor = vec3(0.0, 0.0, 0.0);
	for(int i = int(0.0); i < 8.0; i++){
		float dist = length(Lights[i].Position - Vertex.xyz);
		vec3 toLightPoint = normalize(Lights[i].Position);
		float att = 1.0 / (1.0 + 0.35 * dist * dist);
		att *= 20.0;
		att = min(att, 1.0);
		
		FLightColor += Lights[i].Color * att; 
	}
	FLightColor =  clamp(FLightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));

	//Specular
	vec3 ReflectedDir = reflect(-unitToLight, unitNormal);
	float SpecBrightness = max(dot(ReflectedDir, unitToCamera), 0.0);
	float Damp = pow(SpecBrightness, Damper) * Reflectivity;
	vec4 Specular = vec4(Damp*FLightColor, 1.0);
	
	//Rim Lighting
	float rim = 1.0 - max(dot(unitToCamera, unitNormal), 0.0);
	rim = smoothstep(0.6, 1.0, rim);
	vec3 finalRim = InColor.rgb * 0.2 * rim * max(FLightColor, vec3(.1, .1, .1));

	//Diffuse Lighting
	vec3 FullLight = clamp(FLightColor + LightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));
	vec4 Ambient = InColor * 0.125;
	vec3 Diffuse = DiffuseModel(vec3(0.0, 0.0, 1.0), unitNormal, FullLight) * .35 + DiffuseModel(vec3(0.0, 0.0, -1.0), unitNormal, FullLight) * .35
				   + DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal, FullLight) * .35 + DiffuseModel(vec3(-1.0, 0.0, 0.0), unitNormal, FullLight) * .35 
				   + DiffuseModel(unitToLight, unitNormal, LightColor) * .7;
	
	PointDiffuse = vec3(0,0,0);//dot(unitNormal, unitToLight) * 0.75 * FLightColor;

	mat3 mat = mat3(transpose(inverse(gl_ModelViewMatrix)));
	Color = Ambient + vec4(finalRim, 0.0) + (vec4(Diffuse,1.0) * InColor) + Specular;
	InPos = (gl_ModelViewMatrix * Vertex).xyz;
	vertex_position = Vertex.xyz;
	InNorm = normalize(mat * unitNormal); 
	
	//Shadows Stuff
	if(UseShadows){
		float ShadowDist = DistanceToCamera - (ShadowDistance - ShadowTransition);
		ShadowDist /= ShadowTransition;
		ShadowDist = clamp(1.0 - ShadowDist, 0.0, 1.0);
		Coords = ShadowMVP * vec4(Vertex.xyz, 1.0);
		Coords.w = ShadowDist;
	}
}

 vec3 TransformNormal(vec3 norm, mat4 invMat)
{
	invMat = inverse(invMat);
    vec3 n;
    n.x = dot(norm, invMat[0].xyz);
    n.y = dot(norm, invMat[1].xyz);
    n.z = dot(norm, invMat[2].xyz);
    return n;
}