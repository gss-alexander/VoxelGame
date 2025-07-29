#version 330 core
in vec2 texCoord;
in vec2 fUv;
flat in int fTextureIndex;

uniform sampler2DArray uTextureArray;

out vec4 FragColor;

void main()
{
    vec4 textureColor = texture(uTextureArray, vec3(fUv, float(fTextureIndex)));
    FragColor = vec4(textureColor.rgb, textureColor.a);
}
