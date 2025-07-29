#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vUv;
layout (location = 2) in float vTextureIndex;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec2 fUv;
flat out int fTextureIndex;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
    gl_Position.z -= 0.0001;
    fUv = vUv;
    fTextureIndex = int(vTextureIndex);
}
