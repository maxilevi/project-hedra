
#version 330 compatibility

in vec4 Color;
in vec4 IColor;
in vec4 InPos;
smooth in vec4 InNorm;
in float Visibility;
in float Height;
in vec4 BotColor;
in vec4 TopColor;
in vec4 Coords;
in vec3 LightDir;
in float Depth;
in float CastShadows;
in float Config;

layout(location = 0)out vec4 OutColor; 
layout(location = 1)out vec4 OutPosition;
layout(location = 2)out vec4 OutNormal;

const vec2 poissonDisk[4] = vec2[](
  vec2( -0.94201624, -0.39906216 ),
  vec2( 0.94558609, -0.76890725 ),
  vec2( -0.094184101, -0.92938870 ),
  vec2( 0.34495938, 0.29387760 )
);

struct PointLight
{
    vec3 Position;
    vec3 Color;
    float Radius;
};

uniform vec4 AreaColors[16];
uniform vec4 AreaPositions[16];
uniform sampler2D ShadowTex;
uniform float UseShadows;
uniform mat4 ShadowMVP;
uniform vec3 LightPosition;
uniform vec3 LightColor;
uniform PointLight Lights[12];
const float Snow = 0.0;
uniform sampler3D noiseTexture;
const float Damper = 10.0;
const float Reflectivity = 0.08;


float when_eq(float x, float y) {//equal
  return 1.0 - abs(sign(x - y));
}

float when_neq(float x, float y) {//not equal
  return abs(sign(x - y));
}

float when_gt(float x, float y) {//greater
  return max(sign(x - y), 0.0);
}

float when_lt(float x, float y) {//smaller
  return max(sign(y - x), 0.0);
}

float when_ge(float x, float y) {
  return 1.0 - when_lt(x, y);
}

float when_le(float x, float y) {
  return 1.0 - when_gt(x, y);
}

float when_neq(vec4 x, vec4 y) {//not equal
  vec4 r = abs(sign(x - y));
  return r.x * r.y * r.z * r.w;
}

float not(float x){
	return 1.0 - x;
}

float and(float x, float y){
	return x * y;
}

float or(float a, float b) {
  return min(a + b, 1.0);
}

vec3 DiffuseModel(vec3 unitToLight, vec3 unitNormal, vec3 LColor){
	float min = 0.0;
	float cond = when_lt(Color.a + 1.0, 0.1);
	min += not(cond) * 0.1;
	min += cond * 0.25;

	float Brightness = max(dot(unitNormal, unitToLight) ,min);
	return (Brightness * LColor );
}

void main(){

	//Lighting
	vec3 unitNormal = normalize(InNorm.xyz);
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(gl_ModelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - InPos.xyz);

	vec3 FLightColor;
	for(int i = 0; i < 12; i++){
		float dist = length(Lights[i].Position.xyz - InPos.xyz);
		vec3 toLightPoint = normalize(Lights[i].Position.xyz);
		float att = 1.0 / (1.0 + 0.35 * dist * dist);
		att *= Lights[i].Radius;
		att = min(att, 1.0);
		
		FLightColor += Lights[i].Color * att; 
	}
	FLightColor = clamp(FLightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));

	//Specular
	vec3 ReflectedDir = reflect(-unitToLight, unitNormal);
	float SpecBrightness = max(dot(ReflectedDir, unitToCamera), 0.0);
	float Damp = pow(SpecBrightness, Damper) * Reflectivity;
	vec4 Specular = vec4(Damp * FLightColor,1.0);
	
	vec3 tex = Color.xyz * vec3(1.0, 1.0, 1.0) * texture(noiseTexture, InPos.xyz).r;
	vec4 InputColor = vec4(Color.xyz + tex * 0.2, 1.0);

	if(!(Config+2.0 < 0.1)){
		for(int i = 0; i < 16; i++){
			if(AreaColors[i] != vec4(0.0, 0.0, 0.0, 0.0))
			InputColor = mix(AreaColors[i], InputColor, clamp(length(AreaPositions[i].xyz - InPos.xyz) / AreaPositions[i].w , 0.0, 1.0) );
		}
	}
	
	//Rim Lighting
	float rim = 1.0 - max(dot(unitToCamera, unitNormal), 0.0);
	rim = smoothstep(0.6, 1.0, rim);
	vec3 finalRim = InputColor.rgb * 0.4 * rim * max(FLightColor, vec3(.4,.4,.4));

	//
	vec3 FDiffuse = + DiffuseModel(unitToLight, unitNormal, FLightColor) * 1.05;
	//Diffuse Lighting
	float Ambient = 0.4;
	vec3 FullLightColor = clamp(LightColor + FLightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));
	vec3 Diffuse = DiffuseModel(vec3(0.0, 0.0, 1.0), unitNormal, FullLightColor) * Ambient + DiffuseModel(vec3(0.0, 0.0, -1.0), unitNormal, FullLightColor) * Ambient
				   + DiffuseModel(vec3(-1.0, 0.0, 0.0), unitNormal, FullLightColor) * Ambient
				   + DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal, FullLightColor) * Ambient + DiffuseModel(vec3(0.0, -1.0, 0.0), unitNormal, FullLightColor) * Ambient * 0.5 
				   + DiffuseModel(unitToLight, unitNormal, LightColor) * 1.05;

	float ShadowVisibility = 1.0;
	float has_shadows = when_gt(UseShadows, 0.0);
	float bias = max(0.001 * (1.0 - dot(InNorm.xyz, LightDir)), 0.0) + 0.001;
	vec4 ShadowCoords = Coords * vec4(.5,.5,.5,1.0) + vec4(.5,.5,.5, 0.0);
			
	float shadow = 0.0;
	float use_disk = or(when_eq(UseShadows,3.0), when_eq(UseShadows, 2.0));

	vec2 texelSize = 1.0 / textureSize(ShadowTex, 0);
	for(int x = int(-1.0 * use_disk * has_shadows + 2.0 * not(use_disk) ); x <= 1.0 * use_disk * has_shadows; ++x)
	{
		for(float y = -1.0; y <= 1.0; ++y)
		{
			for (int i=int(0.0);i<4.0;i++){
				vec4 fetch = texture(ShadowTex, ShadowCoords.xy + vec2(x, y) * texelSize + poissonDisk[i] / 1500.0);
				float pcfDepth = fetch.r; 
				if ( pcfDepth  <  ShadowCoords.z - bias)
					shadow += 1.0 * Coords.w * fetch.w;  
			}					
		}    
	}
	shadow += use_disk * has_shadows * (shadow / (9.0*4.0) - shadow);


	for(int x = int(-1.0 * not(use_disk) * has_shadows + 2.0 * use_disk); x <= 1.0 * not(use_disk) * has_shadows; ++x)
	{
		for(float y = -1.0; y <= 1.0; ++y)
		{
			vec4 fetch = texture(ShadowTex, ShadowCoords.xy  + vec2(x, y) * texelSize );
			float pcfDepth = fetch.r; 
			if ( pcfDepth  <  ShadowCoords.z - bias)
				shadow += 1.0 * Coords.w * fetch.a; 
		}    
	}
	shadow += not(use_disk) * has_shadows * ( shadow / 9.0 - shadow);
			
	//ShadowVisibility += has_shadows * (1.0 - ( (shadow * 0.55) * Coords.w)) - 1.0;
	//ShadowVisibility += has_shadows * when_eq(ShadowCoords.z, 1.0) * 1.0;
	ShadowVisibility = 1.0 - (shadow * 0.55);
	
	float cond_met = when_gt(InNorm.y, 0.4) * Snow;
	float snow_val = 1.2;
	InputColor += cond_met * when_lt(ShadowVisibility, 1.0) * vec4(snow_val-InputColor.x, snow_val-InputColor.y, snow_val-InputColor.z, 0.0) * clamp(InNorm.y, 0.0, 1.0) * 1.0 * clamp(ShadowVisibility ,0.0, 1.0);
	InputColor += cond_met * not(when_lt(ShadowVisibility, 1.0)) * vec4(snow_val-InputColor.x, snow_val-InputColor.y, snow_val-InputColor.z, 0.0) * clamp(InNorm.y, 0.0, 1.0) * 1.0;

	vec4 FinalColor = vec4(finalRim, 0.0) + (vec4(Diffuse,1.0) * InputColor) + Specular;
	vec4 SkyColor = vec4( mix(BotColor, TopColor, (gl_FragCoord.y / Height) - .25) );
	vec4 NewColor = mix(SkyColor, FinalColor * ShadowVisibility + (vec4(FDiffuse, 1.0) * InputColor), Visibility);
	
	mat3 NormalMat = mat3(transpose(inverse(gl_ModelViewMatrix)));

	if(Visibility == 0.0){
		OutColor = vec4(NewColor.xyz, 1.0);
		OutPosition = vec4( InPos.xyz, gl_FragCoord.z);
		OutNormal = vec4(0.0, 0.0, 0.0, 1.0);
	}else{
		mat3 NormalMat = mat3(transpose(inverse(gl_ModelViewMatrix)));
		OutColor = vec4(NewColor.xyz,1.0);
		OutPosition = vec4( (gl_ModelViewMatrix * vec4(InPos.xyz, 1.0)).xyz, gl_FragCoord.z);
		OutNormal = vec4(NormalMat * InNorm.xyz, 1.0);
	}

}