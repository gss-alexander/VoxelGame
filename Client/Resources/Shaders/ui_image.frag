#version 330 core
in vec2 fUv;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform vec3 uColor;
uniform float uAlpha;

void main()
{
    vec4 texColor = texture(uTexture, fUv);
    FragColor = vec4(uColor, uAlpha) * texColor;
}
