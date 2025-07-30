#version 330 core
in vec2 texCoord;
in vec3 fColor;
out vec4 FragColor;

uniform sampler2D uTexture;

void main()
{
    vec4 texColor = texture(uTexture, texCoord);
    FragColor = vec4(fColor, 1.0) * texColor;
}