#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vUv;
layout (location = 2) in float vTextureIndex;
layout (location = 3) in float aBrightness;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform vec3 uCameraPos;

out vec2 fUv;
flat out int fTextureIndex;
out float fBrightness;
out float fDistance;

void main()
{
    vec4 worldPos = uModel * vec4(vPos, 1.0);
    gl_Position = uProjection * uView * worldPos;

    fUv = vUv;
    fTextureIndex = int(vTextureIndex);
    fBrightness = aBrightness;
    fDistance = length(worldPos.xyz - uCameraPos);
}