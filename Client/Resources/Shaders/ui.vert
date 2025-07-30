#version 330 core
layout (location = 0) in vec2 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec3 vColor;

uniform mat4 uProjection;

out vec2 texCoord;
out vec3 fColor;

void main()
{
    gl_Position = uProjection * vec4(aPosition, 0.0, 1.0);
    texCoord = aTexCoord;
    fColor = vColor;
}