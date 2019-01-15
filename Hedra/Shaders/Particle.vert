#version 330 core

!include<"Includes/Lighting.shader">

layout(location = 0) in vec3 v_pos;
layout(location = 1) in vec3 v_Normal;
layout(location = 2) in vec4 v_Color;
layout(location = 3) in vec4 Col1;
layout(location = 4) in vec4 Col2;
layout(location = 5) in vec4 Col3;
layout(location = 6) in vec4 Col4;
 
smooth out vec4 Color; 
out float Visibility;
out float pass_height;
out vec4 pass_botColor;
out vec4 pass_topColor;

uniform vec3 PlayerPosition;
layout(std140) uniform FogSettings {
	vec4 U_BotColor;
	vec4 U_TopColor;
	float MinDist;
	float MaxDist;	
	float U_Height;
};

 void main()
 {
 	mat4 TransMatrix = mat4(Col1, Col2, Col3, Col4);
 	vec4 v = vec4(v_pos, 1.0); 
 	v =  v * TransMatrix;
	gl_Position = _modelViewProjectionMatrix * v;
	
	pass_height = U_Height;
	pass_botColor = U_BotColor;
	pass_topColor = U_TopColor;
	
	float DistanceToCamera = length(vec3(PlayerPosition - v.xyz).xz);
	Visibility = clamp( (MaxDist - DistanceToCamera) / (MaxDist - MinDist), 0.0, 1.0);
	
	
	//Lighting
	vec3 unitNormal = normalize(v_Normal);
	vec3 unitToLight = normalize(LightPosition);
	vec3 unitToCamera = normalize((inverse(_modelViewMatrix) * vec4(0.0, 0.0, 0.0, 1.0) ).xyz - v.xyz);
	vec3 real_light_color = clamp(calculate_lights(LightColor, v.xyz) + LightColor, vec3(0.0, 0.0, 0.0), vec3(1.0, 1.0, 1.0));
	
	Ambient = 0.75;
	vec3 Diffuse = diffuse(unitToLight, unitNormal, real_light_color).xyz;
	
	Color = (vec4(Diffuse, 1.0) * v_Color);
 }

