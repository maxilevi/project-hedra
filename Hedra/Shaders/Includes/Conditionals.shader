
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
