#version 330 core
in vec2 fUv;
flat in int fTextureIndex;
in float fBrightness;
in float fDistance;

uniform sampler2DArray uTextureArray;
uniform vec3 uFogColor;
uniform float uFogNear;
uniform float uFogFar;

out vec4 FragColor;

void main()
{
    vec4 textureColor = texture(uTextureArray, vec3(fUv, float(fTextureIndex)));
    vec3 finalColor = textureColor.rgb * fBrightness;

    // Calculate fog factor
    float fogFactor = (uFogFar - fDistance) / (uFogFar - uFogNear);
    fogFactor = clamp(fogFactor, 0.0, 1.0);

    // Apply fog
    finalColor = mix(uFogColor, finalColor, fogFactor);

    FragColor = vec4(finalColor, textureColor.a);
}