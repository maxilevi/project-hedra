in float pass_height;
in vec4 pass_botColor;
in vec4 pass_topColor;

vec4 sky_color()
{
    return vec4(mix(pass_botColor, pass_topColor, (gl_FragCoord.y / pass_height) + .15)) * 1.0;
}