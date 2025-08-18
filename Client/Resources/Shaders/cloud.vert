#version 330 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in float vBrightness;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out float fBrightness;

void main() {
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
    fBrightness = vBrightness;
}
