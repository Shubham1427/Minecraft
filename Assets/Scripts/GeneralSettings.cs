using UnityEngine;

public static class GeneralSettings
{
    public static int worldSizeInChunks = 8;
    public static int chunkHeight = 128;
    public static int chunkWidth = 16;
    public static int TextureSize = 16;
    public static float lightOffset = 0.08f;
    public static float cloudScale = 8.8f;

    //Time in seconds of how long day/night will sustain. Default = 600 (10 minutes)
    public static float dayNightTimePeriod = 600f;

    //IMPORTANT These two values should be between 0 and 1 only

    public static float dayMaxLight = 1f;
    public static float NightMinLight = 0.1f;

    public static float GenerateNoise3D(float x, float y, float z)
    {
        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);
        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        return (ab + bc + ac + ba + cb + ca) / 6f;
    }
}