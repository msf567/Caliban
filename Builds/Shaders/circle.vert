#version 330 core

uniform vec2 uResolution;
uniform vec2 uCenter;

void main()
{
    const float radius = 100.0;
    const int segments = 64;

    vec2 center = uCenter;

    vec2 p;

    // Vertex 0 = center of circle
    if (gl_VertexID == 0)
    {
        p = center;
    }
    else
    {
        // Vertices 1..65 = circle perimeter
        float angle =
            float(gl_VertexID - 1) /
            float(segments) *
            6.28318530718;

        p = center +
            vec2(
                cos(angle),
                sin(angle)
            ) * radius;
    }

    // Convert pixels to OpenGL NDC

    float ndcX =
        (p.x / uResolution.x) * 2.0 - 1.0;

    float ndcY =
        1.0 -
        (p.y / uResolution.y) * 2.0;

    gl_Position =
        vec4(
            ndcX,
            ndcY,
            0.0,
            1.0
        );
}
