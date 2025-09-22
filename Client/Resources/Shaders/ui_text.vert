#version 330 core
layout (location = 0) in vec2 vPosition;
layout (location = 1) in vec2 vUv;

uniform mat4 uProjection;

out vec2 fUv;

void main()
{
    gl_Position = uProjection * vec4(vPosition, 0.0, 1.0);
    fUv = vUv;
}
