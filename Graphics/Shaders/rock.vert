#version 330 core

uniform vec2 uResolution;
uniform vec2 uCenter;
uniform float uRadius;

out vec2 vPosition;

void main()
{
    // Triangle strip:
    //
    // 0 ---- 1
    // |      |
    // |      |
    // 2 ---- 3

    vec2 corners[4] = vec2[](
            vec2(-1.0, -1.0),
            vec2(1.0, -1.0),
            vec2(-1.0, 1.0),
            vec2(1.0, 1.0)
    );

    vec2 local = corners[gl_VertexID] * uRadius;

    vec2 pixelPosition = uCenter + local;

    // Pass actual screen pixel coordinates to the fragment shader.
    vPosition = pixelPosition;

    // Convert pixels -> NDC.
    vec2 ndc = pixelPosition / uResolution * 2.0 - 1.0;

    // OpenGL Y points upward, assuming your drawable uses the
    // same coordinate convention.
    gl_Position = vec4(ndc.x, ndc.y, 0.0, 1.0);
}