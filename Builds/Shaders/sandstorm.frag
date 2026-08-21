#version 330 core

// Draws a single sand grain for each point sprite: a soft round dot tinted a
// sandy colour and faded by the particle's own alpha.

in float vAlpha;

out vec4 FragColor;

void main()
{
    // gl_PointCoord is 0..1 across the sprite; measure distance from centre.
    vec2 c = gl_PointCoord - vec2(0.5);
    float d = dot(c, c);

    // Discard the corners so the grain is round rather than a square block.
    if (d > 0.25)
        discard;

    // Soft edge from centre (1.0) to rim (0.0).
    float soft = smoothstep(0.25, 0.0, d);

    vec3 sand = vec3(0.78, 0.63, 0.38);

    FragColor = vec4(sand, vAlpha * soft);
}
