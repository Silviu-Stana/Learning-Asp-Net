#version 330 core

in vec2 vTexCoords;
out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
    vec4 sampled = texture(uTexture, vTexCoords);
    FragColor = vec4(sampled.rgb, sampled.a);
}