#version 330 core

// One vertex per sand particle, rendered as a point sprite. The CPU simulates
// the particles (position/size/alpha) and streams them into the vertex buffer
// every frame, so this shader only has to turn a pixel-space position into a
// point on screen.

layout(location = 0) in vec2 aPos;    // particle position in pixels
layout(location = 1) in float aSize;  // point size in pixels
layout(location = 2) in float aAlpha; // per-particle opacity

uniform vec2 uResolution;

out float vAlpha;

void main()
{
    vAlpha = aAlpha;
    gl_PointSize = aSize;

    // Convert pixels (top-left origin) to OpenGL NDC.
    float ndcX = (aPos.x / uResolution.x) * 2.0 - 1.0;
    float ndcY = 1.0 - (aPos.y / uResolution.y) * 2.0;

    gl_Position = vec4(ndcX, ndcY, 0.0, 1.0);
}
