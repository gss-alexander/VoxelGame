#version 330 core

in float fBrightness;

out vec4 FragColor;

void main() {
    FragColor = vec4(vec3(1.0, 1.0, 1.0) * fBrightness, 1.0);
}
