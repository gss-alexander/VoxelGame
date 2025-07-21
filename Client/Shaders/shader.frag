#version 330 core
in vec2 fUv;
flat in int fTextureIndex; // Receives integer texture index

uniform sampler2DArray uTextureArray; // Changed from sampler2D to sampler2DArray

out vec4 FragColor;

void main()
{
    // Sample from texture array using vec3(u, v, layer)
    FragColor = texture(uTextureArray, vec3(fUv, float(fTextureIndex)));
}
