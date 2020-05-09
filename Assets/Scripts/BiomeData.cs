using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome", menuName = "Biome", order = 1)]
public class BiomeData : ScriptableObject
{
    public Biome[] biomes;
}

[System.Serializable]
public class Biome
{
    public string biomeName;
    public int terrainHeight;
    public int solidGroundHeight;
    public int minTreeHeight;
    public int maxTreeHeight;
    public float terrainScale;
    public float minNoiseForTerrain;
    public float maxNoiseForTerrain;
    public float TreePlacementScale;
    public float TreePlacementThreshold;
}