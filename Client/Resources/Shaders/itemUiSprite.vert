#version 330 core
layout (location = 0) in vec2 vPos;
layout (location = 1) in vec2 vUv;
layout (location = 2) in float vTextureIndex;

uniform mat4 uProjection;

out vec2 fUv;
flat out int fTextureIndex;

void main() {
    gl_Position = uProjection * vec4(vPos.x, vPos.y, 0, 1.0);
    fUv = vUv;
    fTextureIndex = int(vTextureIndex);
}
