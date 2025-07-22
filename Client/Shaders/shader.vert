#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vUv;
layout (location = 2) in float vTextureIndex;
layout (location = 3) in float aBrightness;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec2 fUv;
flat out int fTextureIndex; // Must be flat to avoid interpolation
out float fBrightness;

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
    fUv = vUv;
    fTextureIndex = int(vTextureIndex);
    fBrightness = aBrightness; 
}
