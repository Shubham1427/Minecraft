using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public float globalLightLevel;

    public BlockTypes[] blockTypes;
    public Material material, cloudMaterial;
    public Chunk[,] chunks;
    public float seed;
    public BiomeData biomesData;
    public Color daySky, nightSky;
    public Camera playerCam;
    public GameObject zombiePrefab;

    private float timeDayNight = 0f;
    private bool isDay = true;
    int zombieCountInWorld = 0;

    // Start is called before the first frame update
    private void Start()
    {
        seed = Random.Range(0f, 1000f);
        //seed = 995.2866f;
        chunks = new Chunk[GeneralSettings.worldSizeInChunks, GeneralSettings.worldSizeInChunks];
        GenerateWorld();
        CreateClouds();
    }

    void CreateClouds()
    {
        for (int x = 0; x < GeneralSettings.worldSizeInChunks; x++)
        {
            for (int z = 0; z < GeneralSettings.worldSizeInChunks; z++)
            {
                GameObject clouds = new GameObject("Cloud (" + x + ", " + z + ")");
                clouds.AddComponent<Clouds>();
                clouds.transform.parent = transform;
                clouds.transform.position = new Vector3(x * GeneralSettings.chunkWidth, 0, z * GeneralSettings.chunkWidth);
                clouds.GetComponent<Clouds>().InitCloud(seed, cloudMaterial);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        timeDayNight += Time.deltaTime;
        if (isDay)
        {
            globalLightLevel = Mathf.Lerp(GeneralSettings.dayMaxLight, GeneralSettings.NightMinLight, timeDayNight / GeneralSettings.dayNightTimePeriod);
            playerCam.backgroundColor = Color.Lerp(daySky, nightSky, timeDayNight / GeneralSettings.dayNightTimePeriod);
        }
        else
        {
            globalLightLevel = Mathf.Lerp(GeneralSettings.NightMinLight, GeneralSettings.dayMaxLight, timeDayNight / GeneralSettings.dayNightTimePeriod);
            playerCam.backgroundColor = Color.Lerp(nightSky, daySky, timeDayNight / GeneralSettings.dayNightTimePeriod);
        }
        if (timeDayNight > GeneralSettings.dayNightTimePeriod)
        {
            timeDayNight = 0f;
            isDay = !isDay;
        }
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        ZombieSpawner();
    }

    Chunk GetChunkFromWorldCoords(int x, int z)
    {
        x = x / GeneralSettings.chunkWidth;
        z = z / GeneralSettings.chunkWidth;
        return chunks[x, z];
    }

    void ZombieSpawner()
    {
        if (zombieCountInWorld < 5 && Random.Range (0f, 1f) < 0.01f)
        {
            int x = Random.Range(0, GeneralSettings.worldSizeInChunks * GeneralSettings.chunkWidth);
            int z = Random.Range(0, GeneralSettings.worldSizeInChunks * GeneralSettings.chunkWidth);
            //int x = 154;
            //int z = 150;
            Chunk currentChunk = GetChunkFromWorldCoords(x, z);
            int x1 = x-(int)currentChunk.myPosition.x;
            int z1 = z-(int)currentChunk.myPosition.z;
            for (int y = 0; y < GeneralSettings.chunkHeight-2; y++)
            {
                if (!blockTypes[currentChunk.myBlocks[x1, y+1, z1].id].isVisible && !blockTypes[currentChunk.myBlocks[x1, y + 2, z1].id].isVisible && blockTypes[currentChunk.myBlocks[x1, y, z1].id].isSolid && currentChunk.myBlocks[x1, y, z1].lightLevel*globalLightLevel <= 1f)
                {
                    GameObject zombie = Instantiate(zombiePrefab, new Vector3(x+0.5f, y+2, z+0.5f), Quaternion.identity);
                    zombie.name = "Zombie " + (zombieCountInWorld + 1);
                    zombieCountInWorld++;
                    return;
                }
            }
        }
    }

    private void GenerateWorld()
    {
        for (int x = 0; x < GeneralSettings.worldSizeInChunks; x++)
        {
            for (int z = 0; z < GeneralSettings.worldSizeInChunks; z++)
            {
                chunks[x, z] = CreateChunk(new Vector2(x * GeneralSettings.chunkWidth, z * GeneralSettings.chunkWidth));
            }
        }
        for (int x = 0; x < GeneralSettings.worldSizeInChunks; x++)
        {
            for (int z = 0; z < GeneralSettings.worldSizeInChunks; z++)
            {
                chunks[x, z].CreateChunk();
            }
        }
    }

    public bool IsPRCInChunk(Vector3 pos)
    {
        return (pos.x >= 0 && pos.x < GeneralSettings.chunkWidth && pos.z >= 0 && pos.z < GeneralSettings.chunkWidth && pos.y >= 0 && pos.y < GeneralSettings.chunkHeight);
    }

    private Chunk CreateChunk(Vector2 pos)
    {
        GameObject newChunk = new GameObject("Chunk (" + (int)(pos.x / GeneralSettings.chunkWidth) + ", " + (int)(pos.y / GeneralSettings.chunkWidth) + ")");
        newChunk.AddComponent<Chunk>();
        newChunk.transform.parent = transform;
        newChunk.GetComponent<Chunk>().InitChunk(new Vector3(pos.x, 0, pos.y), material, seed);
        return newChunk.GetComponent<Chunk>();
    }
}

[System.Serializable]
public class BlockTypes
{
    public string blockName;
    public bool isVisible;
    public bool isTransparent;
    public bool isSolid;
    public float transparencyValue;

    public int[] texures = new int[6];      //Order -> Right, Left, Front, Back, Top, Bottom
}