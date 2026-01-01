using UnityEngine;

public class CastleWallTerrain : MonoBehaviour
{
    [Header("Terrain")]
    [Range(129, 2049)] public int resolution = 513;
    public int sizeX = 900;
    public int sizeZ = 900;
    public float sizeY = 140f;

    [Header("Seed")]
    public int seed = 1337;

    [Header("Castle Hill (Subtle Center Elevation)")]
    [Range(0.05f, 0.35f)] public float castleRadius = 0.18f;
    [Range(0.00f, 0.20f)] public float castleTopRadius = 0.06f;
    [Range(0.00f, 0.50f)] public float castleHeight = 0.16f;      // subtle
    [Range(1.0f, 6.0f)] public float castleSlopeSharpness = 1.6f; // soft slope

    [Header("Rolling Hills (Middle Area)")]
    public float hillsScale = 260f;
    [Range(1, 8)] public int hillsOctaves = 4;
    [Range(0.1f, 0.9f)] public float hillsPersistence = 0.5f;
    [Range(1.5f, 3.5f)] public float hillsLacunarity = 2.0f;
    [Range(0.0f, 0.25f)] public float hillsAmplitude = 0.07f;

    [Header("Mountain Walls (4 Sides)")]
    [Range(0.02f, 0.35f)] public float wallThickness = 0.14f;  // fraction of map on each side
    [Range(0.00f, 1.00f)] public float wallHeight = 0.34f;
    public float wallScale = 140f;
    [Range(1, 8)] public int wallOctaves = 5;
    [Range(0.1f, 0.9f)] public float wallPersistence = 0.52f;
    [Range(1.5f, 3.5f)] public float wallLacunarity = 2.2f;

    [Header("Tiny Entrance (Centered on one side)")]
    public bool carveGate = true;
    public GateSide gateSide = GateSide.North;
    [Range(0.01f, 0.30f)] public float gateWidth = 0.06f;   // fraction of side length
    [Range(0.0f, 1.0f)] public float gateDepth = 0.92f;     // how deep the cut is (1 = full cut)
    [Range(0.05f, 0.80f)] public float gateSoftness = 0.18f; // how gradually it blends

    [Header("Global")]
    [Range(0.0f, 0.50f)] public float seaLevel = 0.08f;
    [Range(0.5f, 3.0f)] public float overallCurve = 1.10f;

    private Terrain _terrain;

    public enum GateSide { North, South, East, West }

    void Start() => Generate();

    [ContextMenu("Generate")]
    public void Generate()
    {
        Random.InitState(seed);

        var data = new TerrainData();
        data.heightmapResolution = resolution;
        data.size = new Vector3(sizeX, sizeY, sizeZ);

        float[,] heights = BuildHeights(resolution, resolution);
        data.SetHeights(0, 0, heights);

        if (_terrain != null) Destroy(_terrain.gameObject);
        var go = Terrain.CreateTerrainGameObject(data);
        go.name = "CastleWallTerrain";
        _terrain = go.GetComponent<Terrain>();
    }

    float[,] BuildHeights(int w, int h)
    {
        float[,] map = new float[w, h];

        Vector2 hillsOff = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        Vector2 wallOff = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));

        float minV = float.MaxValue, maxV = float.MinValue;

        for (int z = 0; z < h; z++)
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)(w - 1); // 0..1
                float v = z / (float)(h - 1); // 0..1

                // Radial distance only for the castle mound (center hill)
                float dx = u - 0.5f;
                float dz = v - 0.5f;
                float r = Mathf.Sqrt(dx * dx + dz * dz) / 0.70710678f;

                // --- 1) Subtle castle mound ---
                float castle = CastleMound(r);

                // --- 2) Rolling hills (kept off the castle plateau) ---
                float suppressCastleTop = SmoothStep(castleTopRadius, castleRadius, r);
                float hills = FBM(u, v, hillsScale, hillsOctaves, hillsPersistence, hillsLacunarity, hillsOff)
                              * hillsAmplitude * suppressCastleTop;

                // --- 3) 4-side mountain walls (RECT border mask, not radial) ---
                float wallMask = BorderWallMask(u, v, wallThickness); // 0 interior, 1 at edges
                float wallNoise = Ridged(u, v, wallScale, wallOctaves, wallPersistence, wallLacunarity, wallOff);
                float walls = wallNoise * wallHeight * wallMask;

                // --- 4) Carve one tiny centered gate on chosen side ---
                if (carveGate)
                {
                    float gate = GateMask(u, v, gateSide, gateWidth, wallThickness, gateSoftness);
                    // gate=1 => reduce walls strongly there
                    float cut = Mathf.Lerp(1f, 1f - gateDepth, gate);
                    walls *= cut;
                }

                float heightVal = 0f;
                heightVal += castleHeight * castle;
                heightVal += hills;
                heightVal += walls;

                heightVal = Mathf.Max(heightVal, seaLevel);
                heightVal = Mathf.Pow(Mathf.Clamp01(heightVal), overallCurve);

                minV = Mathf.Min(minV, heightVal);
                maxV = Mathf.Max(maxV, heightVal);

                map[x, z] = heightVal;
            }

        // Normalize to [0,1]
        for (int z = 0; z < h; z++)
            for (int x = 0; x < w; x++)
                map[x, z] = Mathf.InverseLerp(minV, maxV, map[x, z]);

        return map;
    }

    // This is the key fix:
    // Use distance-to-nearest-edge (square border), not distance from center (circle).
    float BorderWallMask(float u, float v, float thickness)
    {
        float dEdge = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v)); // 0 at edge, 0.5 at center
        // Convert to "how deep into the border we are":
        float t = 1f - Mathf.Clamp01(dEdge / Mathf.Max(0.0001f, thickness));
        // Smooth so it doesn't look like a hard cliff everywhere
        return t * t * (3f - 2f * t);
    }

    // Gate is centered on one side within the wall band, with a soft blend.
    float GateMask(float u, float v, GateSide side, float width, float thickness, float softness)
    {
        // width is fraction across the side (centered at 0.5)
        float across = 0f; // coordinate along the side
        float inBand = 0f; // are we inside the wall band on that side?

        switch (side)
        {
            case GateSide.North: // v near 1
                across = u;
                inBand = SmoothStep(1f - thickness, 1f, v);
                break;
            case GateSide.South: // v near 0
                across = u;
                inBand = 1f - SmoothStep(0f, thickness, v);
                break;
            case GateSide.East: // u near 1
                across = v;
                inBand = SmoothStep(1f - thickness, 1f, u);
                break;
            case GateSide.West: // u near 0
                across = v;
                inBand = 1f - SmoothStep(0f, thickness, u);
                break;
        }

        // Gate centered at 0.5 along the side
        float halfW = width * 0.5f;
        float dist = Mathf.Abs(across - 0.5f);

        // Soft corridor: 1 inside gate, 0 outside
        float gateCore = 1f - SmoothStep(halfW, halfW + softness, dist);

        // Only apply gate in the border band of that side
        return Mathf.Clamp01(gateCore * inBand);
    }

    float CastleMound(float r)
    {
        if (r <= castleTopRadius) return 1f;
        float t = Mathf.InverseLerp(castleTopRadius, castleRadius, r);
        t = Mathf.Clamp01(t);
        float slope = 1f - Mathf.Pow(t, castleSlopeSharpness);
        return (r >= castleRadius) ? 0f : slope;
    }

    float SmoothStep(float a, float b, float x)
    {
        float t = Mathf.InverseLerp(a, b, x);
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    float FBM(float u, float v, float scale, int oct, float pers, float lac, Vector2 off)
    {
        float amp = 1f, freq = 1f, sum = 0f, norm = 0f;
        for (int i = 0; i < oct; i++)
        {
            float x = (u * sizeX) / scale * freq + off.x;
            float z = (v * sizeZ) / scale * freq + off.y;
            float n = Mathf.PerlinNoise(x, z);
            sum += n * amp;
            norm += amp;
            amp *= pers;
            freq *= lac;
        }
        return sum / Mathf.Max(0.0001f, norm);
    }

    float Ridged(float u, float v, float scale, int oct, float pers, float lac, Vector2 off)
    {
        float amp = 1f, freq = 1f, sum = 0f, norm = 0f;
        float weight = 1f;

        for (int i = 0; i < oct; i++)
        {
            float x = (u * sizeX) / scale * freq + off.x;
            float z = (v * sizeZ) / scale * freq + off.y;

            float n = Mathf.PerlinNoise(x, z);
            n = 1f - Mathf.Abs(n * 2f - 1f);
            n *= n;
            n *= weight;
            weight = Mathf.Clamp01(n * 1.15f);

            sum += n * amp;
            norm += amp;

            amp *= pers;
            freq *= lac;
        }
        return sum / Mathf.Max(0.0001f, norm);
    }
}
