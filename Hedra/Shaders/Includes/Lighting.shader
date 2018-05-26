
uniform vec3 LightPosition = vec3(-500.0, 800.0, 0.0);
uniform vec3 LightColor = vec3(0.0, 0.0, 0.0);

const vec3 RimColor = vec3(1.0, 1.0, 1.0);
float Damper = 64;
float Reflectivity = 0.05;
float Ambient = 0.0;

vec3 DiffuseModel(vec3 unitToLight, vec3 unitNormal, vec3 LColor){	
	return LColor * max(dot(unitNormal, unitToLight), Ambient);
}

vec4 rim(vec3 InputColor, vec3 FLightColor, vec3 unitToCamera, vec3 unitNormal){
    float rim = 1.0 - max(dot(unitToCamera, unitNormal) * 1.0, 0.0);
    rim = smoothstep(0.6, 1.0, rim);
    return vec4(InputColor * 0.25 * rim * max(FLightColor, vec3(.15,.15,.15)), 0);
}

vec4 specular(vec3 unitToLight, vec3 unitNormal, vec3 unitToCamera, vec3 FLightColor) {
	return vec4(0.0, 0.0, 0.0, 0.0);
    /*vec3 ReflectedDir = reflect(-unitToLight, unitNormal);
    float SpecBrightness = max(dot(ReflectedDir, unitToCamera), 0.0);
    float Damp = pow(SpecBrightness, Damper) * Reflectivity;
    return vec4(Damp * FLightColor,1.0) * 0.0;*/
}

vec4 diffuse(vec3 unitToLight, vec3 unitNormal, vec3 FullLightColor){
	return vec4(
		DiffuseModel(vec3(1.0, 0.0, 0.0), unitNormal, FullLightColor) * 0.25
		+ DiffuseModel(vec3(-1.0, 0.0, 0.0), unitNormal, FullLightColor) * 0.25
		+ DiffuseModel(vec3(0.0, 0.0, -1.0), unitNormal, FullLightColor) * 0.25
		+ DiffuseModel(vec3(0.0, 0.0, 1.0), unitNormal, FullLightColor) * 0.25
		+ DiffuseModel(unitToLight, unitNormal, FullLightColor) * 0.75,
	1.0);
}	