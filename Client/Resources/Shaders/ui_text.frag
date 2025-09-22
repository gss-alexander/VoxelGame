#version 330 core
in vec2 fUv;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform vec3 uColor;
uniform float uAlpha;

void main()
{
    float textAlpha = texture(uTexture, fUv).r;
    FragColor = vec4(uColor, uAlpha * textAlpha);
}