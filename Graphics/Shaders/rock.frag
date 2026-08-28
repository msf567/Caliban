#version 330 core

in vec2 vPosition;

out vec4 FragColor;

#define MAX_INNER_POINTS 32
#define MAX_OUTER_POINTS 24
#define MAX_CELL_COLORS 96

uniform vec2 uResolution;
uniform vec2 uCenter;
uniform float uRadius;

uniform int uInnerPointCount;
uniform int uOuterPointCount;

uniform vec2 uInnerPoints[MAX_INNER_POINTS];
uniform vec2 uOuterPoints[MAX_OUTER_POINTS];

// Three colors per inner Voronoi cell.
//
// Cell 0:
//   [0] [1] [2]
//
// Cell 1:
//   [3] [4] [5]
//
// Cell 2:
//   [6] [7] [8]
//
// etc.
uniform vec3 uCellColors[MAX_CELL_COLORS];


void main()
{
    // ---------------------------------------------------------
    // Convert screen position into rock-local coordinates.
    //
    // C# generates points roughly in the range -1..1.
    // ---------------------------------------------------------

    vec2 p = (vPosition - uCenter) / uRadius;


    // ---------------------------------------------------------
    // Find the closest point among BOTH inner and outer points.
    // ---------------------------------------------------------

    float closestDistance = 1000000.0;

    int closestIndex = -1;

    // 0 = inner
    // 1 = outer
    int closestType = -1;


    // ---------------------------------------------------------
    // Inner points
    // ---------------------------------------------------------

    for (int i = 0; i < MAX_INNER_POINTS; i++)
    {
        if (i >= uInnerPointCount)
        break;

        float d = distance(p, uInnerPoints[i]);

        if (d < closestDistance)
        {
            closestDistance = d;
            closestIndex = i;
            closestType = 0;
        }
    }


    // ---------------------------------------------------------
    // Outer points
    // ---------------------------------------------------------

    for (int i = 0; i < MAX_OUTER_POINTS; i++)
    {
        if (i >= uOuterPointCount)
        break;

        float d = distance(p, uOuterPoints[i]);

        if (d < closestDistance)
        {
            closestDistance = d;
            closestIndex = i;
            closestType = 1;
        }
    }


    // ---------------------------------------------------------
    // Outer Voronoi cells are outside the rock.
    // ---------------------------------------------------------

    if (closestType != 0)
    discard;


    // ---------------------------------------------------------
    // The closest point is an inner point.
    //
    // Each inner point owns three palette colors.
    // ---------------------------------------------------------

    int colorOffset = closestIndex * 3;


    // ---------------------------------------------------------
    // Generate deterministic pseudo-random value for this pixel.
    //
    // This is deliberately based on pixel position AND the
    // Voronoi cell so adjacent cells don't share the exact
    // same random pattern.
    // ---------------------------------------------------------

    ivec2 pixel = ivec2(floor(vPosition));

    uint seed =
    uint(pixel.x) * 374761393u +
    uint(pixel.y) * 668265263u +
    uint(closestIndex) * 2147483647u;

    seed ^= seed >> 13;
    seed *= 1274126177u;
    seed ^= seed >> 16;

    float randomValue =
    float(seed) / 4294967295.0;


    // ---------------------------------------------------------
    // Pick one of the three colors for this cell.
    // ---------------------------------------------------------

    int colorIndex;

    if (randomValue < 0.3333333)
    {
        colorIndex = 0;
    }
    else if (randomValue < 0.6666666)
    {
        colorIndex = 1;
    }
    else
    {
        colorIndex = 2;
    }


    // ---------------------------------------------------------
    // Draw.
    // ---------------------------------------------------------

    FragColor = vec4(
            uCellColors[colorOffset + colorIndex],
            1.0
    );
}