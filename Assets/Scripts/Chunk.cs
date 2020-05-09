using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private List<Vector3> verticesCoords = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> trianglesVertices = new List<int>();
    private List<Color> colors = new List<Color>();
    private int verticesIndex = 0;
    public Vector3 myPosition;
    public BlockData[,,] myBlocks;
    private World world;
    private Material material;
    private float seed;

    private bool IsPRCInWorld(Vector3 pos)
    {
        pos += myPosition;
        int temp = GeneralSettings.chunkWidth * GeneralSettings.worldSizeInChunks;
        return (pos.x >= 0 && pos.x < temp && pos.z >= 0 && pos.z < temp && pos.y >= 0 && pos.y < GeneralSettings.chunkHeight);
    }

    public void InitChunk(Vector3 pos, Material m, float s)
    {
        seed = s;
        world = GameObject.Find("World").GetComponent<World>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        myPosition = pos;
        transform.position = myPosition;
        material = m;

        myBlocks = new BlockData[GeneralSettings.chunkWidth, GeneralSettings.chunkHeight, GeneralSettings.chunkWidth];
        for (int x = 0; x < GeneralSettings.chunkWidth; x++)
        {
            for (int z = 0; z < GeneralSettings.chunkWidth; z++)
            {
                for (int y = 0; y < GeneralSettings.chunkHeight; y++)
                {
                    myBlocks[x, y, z] = new BlockData(0);
                    NoiseMapGeneration(x, y, z);
                }
            }
        }
        for (int x = 0; x < GeneralSettings.chunkWidth; x++)
        {
            for (int z = 0; z < GeneralSettings.chunkWidth; z++)
            {
                WorldPainting(x, GeneralSettings.chunkHeight - 1, z);
            }
        }
        CalculateLight();
    }


    private void CreateChunkMesh()
    {
        meshRenderer.material = material;

        Mesh mesh = new Mesh
        {
            name = "ChunkMesh",
            vertices = verticesCoords.ToArray(),
            triangles = trianglesVertices.ToArray(),
            uv = uvs.ToArray(),
            colors = colors.ToArray(),
        };

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    public void CreateChunk()
    {
        CalculateLight();
        for (int x = 0; x < GeneralSettings.chunkWidth; x++)
        {
            for (int z = 0; z < GeneralSettings.chunkWidth; z++)
            {
                for (int y = GeneralSettings.chunkHeight - 1; y >= 0; y--)
                {
                    CreateBlockData(new Vector3(x, y, z));
                }
            }
        }
        for (int x = 0; x < GeneralSettings.chunkWidth; x++)
        {
            for (int z = 0; z < GeneralSettings.chunkWidth; z++)
            {
                for (int y = 0; y < GeneralSettings.chunkHeight - 2; y++)
                {
                    CalculateNeighborLinks(new Vector3Int(x, y, z));
                }
            }
        }
        CreateChunkMesh();
    }

    private BlockData GetBlock(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        int chunkX = x / GeneralSettings.chunkWidth;
        int chunkZ = z / GeneralSettings.chunkWidth;

        x -= chunkX * GeneralSettings.chunkWidth;
        z -= chunkZ * GeneralSettings.chunkWidth;

        if (!world.IsPRCInChunk(new Vector3(x, y, z)))
            return null;

        return world.chunks[chunkX, chunkZ].myBlocks[x, y, z];
    }

    void CalculateNeighborLinks(Vector3Int blockPos)
    {
        for (int i = 0; i < 8; i++)
        {
            myBlocks[blockPos.x, blockPos.y, blockPos.z].neighbors[i] = false;
        }
        if (!world.blockTypes[myBlocks[blockPos.x, blockPos.y, blockPos.z].id].isVisible)
            return;
        if (blockPos.y + 2 < GeneralSettings.chunkHeight && (world.blockTypes[myBlocks[blockPos.x, blockPos.y + 1, blockPos.z].id].isVisible || world.blockTypes[myBlocks[blockPos.x, blockPos.y + 2, blockPos.z].id].isVisible))
            return;
        for (int i = 0; i < 8; i++)
        {
            Vector3Int adjacentBlockCoords = blockPos + BlockMeshData.neighbourIndex[i];
            if (world.IsPRCInChunk(adjacentBlockCoords))
            {
                if (adjacentBlockCoords.y + 3 < GeneralSettings.chunkHeight && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + 1, adjacentBlockCoords.z].id].isVisible && world.blockTypes[myBlocks[blockPos.x, blockPos.y + 3, blockPos.z].id].isVisible)
                    continue;
                if (adjacentBlockCoords.y+2 < GeneralSettings.chunkHeight && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + 2, adjacentBlockCoords.z].id].isVisible)
                    continue;
                if (adjacentBlockCoords.y + 3 < GeneralSettings.chunkHeight && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + 1, adjacentBlockCoords.z].id].isVisible && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + 3, adjacentBlockCoords.z].id].isVisible)
                    continue;
                if (adjacentBlockCoords.y - 3 >= 0 && adjacentBlockCoords.y + 1 < GeneralSettings.chunkHeight && !world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y+1, adjacentBlockCoords.z].id].isVisible  && !world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y, adjacentBlockCoords.z].id].isVisible && !world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y - 1, adjacentBlockCoords.z].id].isVisible && !world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y - 2, adjacentBlockCoords.z].id].isVisible && !world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y - 3, adjacentBlockCoords.z].id].isVisible)
                    continue;
                if (i > 3)
                {
                    bool skipIteration = false;
                    for (int j = 1; j >= -3; j--)
                    {
                        if (adjacentBlockCoords.y + j < GeneralSettings.chunkHeight && adjacentBlockCoords.y + j >= 0)
                        {
                            Vector3Int neighboursSideBlock1;
                            Vector3Int neighboursSideBlock2;
                            if (j < 1)
                            {
                                neighboursSideBlock1 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4)] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 1, (int)myPosition.z);
                                neighboursSideBlock2 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4) + 1] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 1, (int)myPosition.z);

                                if (neighboursSideBlock1.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock1) != null && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + j, adjacentBlockCoords.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock1).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                                if (neighboursSideBlock2.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock2) != null && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + j, adjacentBlockCoords.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock2).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }

                                neighboursSideBlock1 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4)] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 2, (int)myPosition.z);
                                neighboursSideBlock2 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4) + 1] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 2, (int)myPosition.z);

                                if (neighboursSideBlock1.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock1) != null && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + j, adjacentBlockCoords.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock1).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                                if (neighboursSideBlock2.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock2) != null && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + j, adjacentBlockCoords.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock2).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                            }
                            else
                            {
                                neighboursSideBlock1 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4)] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 2, (int)myPosition.z);
                                neighboursSideBlock2 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4) + 1] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 2, (int)myPosition.z);

                                if (neighboursSideBlock1.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock1) != null && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + j, adjacentBlockCoords.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock1).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                                if (neighboursSideBlock2.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock2) != null && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + j, adjacentBlockCoords.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock2).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }

                                neighboursSideBlock1 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4)] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 3, (int)myPosition.z);
                                neighboursSideBlock2 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4) + 1] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 3, (int)myPosition.z);

                                if (neighboursSideBlock1.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock1) != null && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + j, adjacentBlockCoords.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock1).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                                if (neighboursSideBlock2.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock2) != null && world.blockTypes[myBlocks[adjacentBlockCoords.x, adjacentBlockCoords.y + j, adjacentBlockCoords.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock2).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (skipIteration)
                        continue;
                }
                myBlocks[blockPos.x, blockPos.y, blockPos.z].neighbors[i] = true;
                continue;
            }
            else if (IsPRCInWorld(adjacentBlockCoords))
            {
                Vector3 temp = (myPosition + adjacentBlockCoords) / GeneralSettings.chunkWidth;
                Vector3 temp2 = myPosition + adjacentBlockCoords;
                temp2 -= world.chunks[(int)temp.x, (int)temp.z].myPosition;
                if (adjacentBlockCoords.y + 3 < GeneralSettings.chunkHeight && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 1, (int)temp2.z].id].isVisible && world.blockTypes[myBlocks[blockPos.x, blockPos.y + 3, blockPos.z].id].isVisible)
                    continue;
                if (temp2.y + 2 < GeneralSettings.chunkHeight && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 2, (int)temp2.z].id].isVisible)
                    continue;
                if (temp2.y + 3 < GeneralSettings.chunkHeight && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 1, (int)temp2.z].id].isVisible && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 3, (int)temp2.z].id].isVisible)
                    continue;
                if (temp2.y - 3 >= 0 && temp2.y+1<GeneralSettings.chunkHeight && !world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y+1, (int)temp2.z].id].isVisible && !world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y, (int)temp2.z].id].isVisible && !world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y-1, (int)temp2.z].id].isVisible && !world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y-2, (int)temp2.z].id].isVisible && !world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y-3, (int)temp2.z].id].isVisible)
                    continue;
                if (i > 3)
                {
                    bool skipIteration = false;
                    for (int j = 1; j >= -3; j--)
                    {
                        if (adjacentBlockCoords.y + j < GeneralSettings.chunkHeight && adjacentBlockCoords.y + j >= 0)
                        {
                            Vector3Int neighboursSideBlock1;
                            Vector3Int neighboursSideBlock2;
                            if (j < 1)
                            {
                                neighboursSideBlock1 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4)] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 1, (int)myPosition.z);
                                neighboursSideBlock2 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4) + 1] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 1, (int)myPosition.z);

                                if (neighboursSideBlock1.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock1) != null && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 2, (int)temp2.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock1).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                                if (neighboursSideBlock2.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock2) != null && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 2, (int)temp2.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock2).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }

                                neighboursSideBlock1 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4)] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 2, (int)myPosition.z);
                                neighboursSideBlock2 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4) + 1] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 2, (int)myPosition.z);

                                if (neighboursSideBlock1.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock1) != null && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 2, (int)temp2.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock1).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                                if (neighboursSideBlock2.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock2) != null && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 2, (int)temp2.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock2).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                            }
                            else
                            {
                                neighboursSideBlock1 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4)] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 2, (int)myPosition.z);
                                neighboursSideBlock2 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4) + 1] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 2, (int)myPosition.z);

                                if (neighboursSideBlock1.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock1) != null && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 2, (int)temp2.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock1).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                                if (neighboursSideBlock2.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock2) != null && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 2, (int)temp2.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock2).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }

                                neighboursSideBlock1 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4)] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 3, (int)myPosition.z);
                                neighboursSideBlock2 = BlockMeshData.diagonalNeighboursSideBlocks[2 * (i - 4) + 1] + adjacentBlockCoords + new Vector3Int((int)myPosition.x, 3, (int)myPosition.z);

                                if (neighboursSideBlock1.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock1) != null && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 2, (int)temp2.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock1).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                                if (neighboursSideBlock2.y < GeneralSettings.chunkHeight && GetBlock(neighboursSideBlock2) != null && world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y + 2, (int)temp2.z].id].isVisible && world.blockTypes[GetBlock(neighboursSideBlock2).id].isVisible)
                                {
                                    skipIteration = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (skipIteration)
                        continue;
                }
                myBlocks[blockPos.x, blockPos.y, blockPos.z].neighbors[i] = true;
                continue;
            }
        }
    }

    private void CreateBlockData(Vector3 coords)
    {
        if (!world.blockTypes[myBlocks[(int)coords.x, (int)coords.y, (int)coords.z].id].isVisible)
            return;

        for (int i = 0; i < 6; i++)
        {
            Vector3 adjacentBlockCoords = coords + BlockMeshData.faceIndex[i];
            float lightLevel = 0f;
            if (world.IsPRCInChunk(adjacentBlockCoords))
            {
                if (!world.blockTypes[myBlocks[(int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y, (int)adjacentBlockCoords.z].id].isTransparent)
                    continue;
                if (world.blockTypes[myBlocks[(int)coords.x, (int)coords.y, (int)coords.z].id].blockName == "Water")
                {
                    if (world.blockTypes[myBlocks[(int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y, (int)adjacentBlockCoords.z].id].isVisible)
                        continue;
                }
                lightLevel = myBlocks[(int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y, (int)adjacentBlockCoords.z].lightLevel;
            }
            else if (IsPRCInWorld(adjacentBlockCoords))
            {
                Vector3 temp = (myPosition + adjacentBlockCoords) / GeneralSettings.chunkWidth;
                Vector3 temp2 = myPosition + adjacentBlockCoords;
                temp2 -= world.chunks[(int)temp.x, (int)temp.z].myPosition;              

                if (!world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y, (int)temp2.z].id].isTransparent)
                    continue;

                if (myBlocks[(int)coords.x, (int)coords.y, (int)coords.z].id == 5)
                {
                    if (world.blockTypes[world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y, (int)temp2.z].id].isVisible)
                        continue;
                }
                lightLevel = world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y, (int)temp2.z].lightLevel;
            }

            verticesCoords.Add(coords + BlockMeshData.verticesCoords[BlockMeshData.triangleVertices[i, 0]]);
            verticesCoords.Add(coords + BlockMeshData.verticesCoords[BlockMeshData.triangleVertices[i, 1]]);
            verticesCoords.Add(coords + BlockMeshData.verticesCoords[BlockMeshData.triangleVertices[i, 2]]);
            verticesCoords.Add(coords + BlockMeshData.verticesCoords[BlockMeshData.triangleVertices[i, 3]]);

            AddTexture(world.blockTypes[myBlocks[(int)coords.x, (int)coords.y, (int)coords.z].id].texures[i]);

            colors.Add(new Color(0f, 0f, 0f, lightLevel));
            colors.Add(new Color(0f, 0f, 0f, lightLevel));
            colors.Add(new Color(0f, 0f, 0f, lightLevel));
            colors.Add(new Color(0f, 0f, 0f, lightLevel));

            trianglesVertices.Add(verticesIndex);
            trianglesVertices.Add(verticesIndex + 1);
            trianglesVertices.Add(verticesIndex + 2);
            trianglesVertices.Add(verticesIndex + 2);
            trianglesVertices.Add(verticesIndex + 1);
            trianglesVertices.Add(verticesIndex + 3);
            verticesIndex += 4;
        }
    }

    void CalculateLight()
    {
        Queue<Vector3Int> LightedBlocks = new Queue<Vector3Int>();
        for (int x = 0; x < GeneralSettings.chunkWidth; x++)
        {
            for (int z = 0; z < GeneralSettings.chunkWidth; z++)
            {
                float lightRay = 1f;
                myBlocks[x, GeneralSettings.chunkHeight - 1, z].lightLevel = lightRay;
                for (int y = GeneralSettings.chunkHeight - 1; y >= 0; y--)
                {
                    if (world.blockTypes[myBlocks[x, y, z].id].transparencyValue < lightRay)
                        lightRay = world.blockTypes[myBlocks[x, y, z].id].transparencyValue;
                    myBlocks[x, y, z].lightLevel = lightRay;
                    if (lightRay > GeneralSettings.lightOffset)
                        LightedBlocks.Enqueue(new Vector3Int(x, y, z));
                }
            }
        }

        while (LightedBlocks.Count > 0)
        {
            Vector3Int coords = LightedBlocks.Dequeue();
            for (int i = 0; i < 6; i++)
            {
                Vector3 adjacentBlockCoords = coords + BlockMeshData.faceIndex[i];
                if (world.IsPRCInChunk(adjacentBlockCoords))
                {
                    if (!world.blockTypes[myBlocks[coords.x, coords.y, coords.z].id].isTransparent && !world.blockTypes[myBlocks[(int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y, (int)adjacentBlockCoords.z].id].isVisible)
                        continue;
                    if (myBlocks[(int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y, (int)adjacentBlockCoords.z].lightLevel < myBlocks[coords.x, coords.y, coords.z].lightLevel - GeneralSettings.lightOffset)
                    {
                        myBlocks[(int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y, (int)adjacentBlockCoords.z].lightLevel = myBlocks[coords.x, coords.y, coords.z].lightLevel - GeneralSettings.lightOffset;
                        if (myBlocks[(int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y, (int)adjacentBlockCoords.z].lightLevel > GeneralSettings.lightOffset)
                            LightedBlocks.Enqueue(new Vector3Int((int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y, (int)adjacentBlockCoords.z));
                    }
                }
                //else if (IsPRCInWorld(adjacentBlockCoords))
                //{
                //    Vector3 temp = (myPosition + adjacentBlockCoords) / GeneralSettings.chunkWidth;
                //    Vector3 temp2 = myPosition + adjacentBlockCoords;
                //    temp2 -= world.chunks[(int)temp.x, (int)temp.z].myPosition;

                //    if (world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y, (int)temp2.z].lightLevel < myBlocks[coords.x, coords.y, coords.z].lightLevel - GeneralSettings.lightOffset)
                //    {
                //        world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y, (int)temp2.z].lightLevel = myBlocks[coords.x, coords.y, coords.z].lightLevel - GeneralSettings.lightOffset;
                //        if (world.chunks[(int)temp.x, (int)temp.z].myBlocks[(int)temp2.x, (int)temp2.y, (int)temp2.z].lightLevel > GeneralSettings.lightOffset)
                //            LightedBlocks.Enqueue(new Vector3Int((int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y, (int)adjacentBlockCoords.z));
                //    }
                //}
            }
        }
    }

    public void ClearChunkData ()
    {
        verticesCoords.Clear();
        colors.Clear();
        trianglesVertices.Clear();
        verticesIndex = 0;
        uvs.Clear();
    }

    private void AddTexture(int ID)
    {
        float y = (int)(ID / GeneralSettings.TextureSize);
        float x = ID - (int)(y * GeneralSettings.TextureSize);

        y = y / GeneralSettings.TextureSize;
        y = 1f - y - 1f / GeneralSettings.TextureSize;
        x = x / GeneralSettings.TextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + 1f / GeneralSettings.TextureSize));
        uvs.Add(new Vector2(x + 1f / GeneralSettings.TextureSize, y));
        uvs.Add(new Vector2(x + 1f / GeneralSettings.TextureSize, y + 1f / GeneralSettings.TextureSize));
    }

    private void WorldPainting(int x, int y, int z)
    {
        if (y < 0)
            return;
        if (myBlocks[x, y, z].id == 7)
        {
            WorldPainting(x, y - 1, z);
            return;
        }
        if (myBlocks[x, y, z].id == 0 && y != 0)
        {
            if (y <= 95)
                myBlocks[x, y, z].id = 5;        //Water
            WorldPainting(x, y - 1, z);
            return;
        }

        GenerateTree(x, y, z);

        //Deciding grass, dirt, or stone
        if (y + 1 >= GeneralSettings.chunkHeight || myBlocks[x, y + 1, z].id == 0 || myBlocks[x, y + 1, z].id == 6 || myBlocks[x, y + 1, z].id == 5)
        {
            if (y + 1 < GeneralSettings.chunkHeight && myBlocks[x, y + 1, z].id == 6)
                myBlocks[x, y, z].id = 3;       //Dirt
            else
                myBlocks[x, y, z].id = 4;       //Grass

            float worldPosx = (myPosition.x + x) / 150f;
            float worldPosz = (myPosition.z + z) / 150f;

            int dirtHeight = 4 + (int)(Mathf.PerlinNoise(worldPosx + seed, worldPosz + seed) * 4f);

            for (int i = 1; i < dirtHeight; i++)
            {
                myBlocks[x, y - i, z].id = 3;      //Dirt
            }
            WorldPainting(x, y - dirtHeight, z);
            return;
        }
        WorldPainting(x, y - 1, z);
    }

    private void GenerateTree(int x, int y, int z)
    {
        Biome biome;
        int biomeIndex = 0;
        float worldPosx = (myPosition.x + x) * 1.2f / 0.25f;
        float worldPosz = (myPosition.z + z) * 1.2f / 0.25f;
        float worldPosy = (myPosition.y + y) / 128f;

        worldPosx *= 1.1f / 100f;
        worldPosz *= 1.1f / 100f;

        float biomeNoise = Mathf.PerlinNoise(worldPosx + seed + 35.8461f, worldPosz + seed + 35.8461f);
        for (int i = 0; i < world.biomesData.biomes.Length; i++)
        {
            biome = world.biomesData.biomes[i];
            if (biomeNoise < biome.maxNoiseForTerrain && biomeNoise >= biome.minNoiseForTerrain)
            {
                biomeIndex = i;
                break;
            }
        }

        worldPosx *= 1.1f * 100f;
        worldPosz *= 1.1f * 100f;

        biome = world.biomesData.biomes[biomeIndex];

        worldPosx = (myPosition.x + x) * biome.TreePlacementScale;
        worldPosz = (myPosition.z + z) * biome.TreePlacementScale;
        worldPosy *= 40f;

        int treeHeight = biome.minTreeHeight + (int)(Mathf.PerlinNoise(worldPosx, worldPosz) * (biome.maxTreeHeight - biome.minTreeHeight));

        if (GeneralSettings.GenerateNoise3D(worldPosx + 43.43856f, worldPosy, worldPosz) >= biome.TreePlacementThreshold && myBlocks[x, y, z].id == 2 && y + treeHeight + 2 < GeneralSettings.chunkHeight)
        {
            bool flag = false;
            for (int i = 1; i <= treeHeight; i++)
            {
                if (myBlocks[x, y + i, z].id != 0)
                {
                    flag = true;
                    break;
                }
            }
            for (int i=-3; i<= 3; i++)
            {
                for (int j=-3; j<=3; j++)
                {
                    if (!world.IsPRCInChunk(new Vector3(x+i, y, z+j)))
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (!flag)
            {
                for (int i = 1; i <= treeHeight; i++)
                {
                    myBlocks[x, y + i, z].id = 6;       //Oak Log
                }
                for (int i = -3; i <= 3; i++)
                {
                    for (int j = -3; j <= 3; j++)
                    {
                       for (int k = treeHeight - 2; k < treeHeight; k++)
                       {
                            if ((i == 0 && j == 0) || myBlocks[x+i, y+k, z+j].id != 0)
                                continue;
                            else
                                myBlocks[x+i, y+k, z+j].id = 7;       //Leaves
                       }
                    }
                }
                for (int i = -2; i <= 2; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        for (int k = treeHeight; k <= treeHeight + 1; k++)
                        {
                            if (i == 0 && j == 0 && k == treeHeight)
                                continue;
                            else
                                myBlocks[x + i, y + k, z + j].id = 7;       //Leaves
                        }
                    }
                }
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        myBlocks[x + i, y + treeHeight + 2, z + j].id = 7;       //Leaves
                    }
                }
            }
        }
    }

    private void NoiseMapGeneration(int x, int y, int z)
    {
        float worldPosx = (myPosition.x + x);
        float worldPosy = (myPosition.y + y);
        float worldPosz = (myPosition.z + z);

        int biomeIndex = 0;
        Biome biome;

        worldPosx *= 1.1f / 100f;
        worldPosz *= 1.1f / 100f;

        float biomeNoise = Mathf.PerlinNoise(worldPosx + seed + 35.8461f, worldPosz + seed + 35.8461f);
        for (int i = 0; i < world.biomesData.biomes.Length; i++)
        {
            biome = world.biomesData.biomes[i];
            if (biomeNoise < biome.maxNoiseForTerrain && biomeNoise >= biome.minNoiseForTerrain)
            {
                biomeIndex = i;
                break;
            }
        }

        worldPosx *= 1.1f * 100f;
        worldPosz *= 1.1f * 100f;

        biome = world.biomesData.biomes[biomeIndex];

        worldPosx *= 1.1f / biome.terrainScale;
        worldPosz *= 1.1f / biome.terrainScale;
        worldPosy /= 128f;

        float noise = GeneralSettings.GenerateNoise3D(worldPosx + seed, worldPosy + seed, worldPosz + seed);
        float minVal = Mathf.Min(Mathf.Abs(biomeNoise - biome.minNoiseForTerrain), Mathf.Abs(biomeNoise - biome.maxNoiseForTerrain));

        noise += ((float)biome.terrainHeight - y) * 0.5f / (biome.terrainHeight - (float)biome.solidGroundHeight);

        //if (minVal < 0.3f)
        //{
        //    if (biome.biomeName == "Mountains")
        //        noise -= 0.3f - minVal;
        //    else if (biome.biomeName == "Plains")
        //        noise += 0.3f - minVal;
        //if (biome.biomeName == "Mountains")
        //{
        //    noise += 1f;
        //}
        //else if (biome.biomeName == "Plains")
        //{
        //    noise += 1f;
        //}
        //}

        //Basic noisemap generation
        if (noise > 0.25f || y <= biome.solidGroundHeight)
            myBlocks[x, y, z].id = 2;       //Stone
        else
            myBlocks[x, y, z].id = 0;       //Air
        if (y == 0)
            myBlocks[x, y, z].id = 1;       //BedRock
    }
}

public class BlockData
{
    public byte id;
    public float lightLevel;
    public bool[] neighbors;

    public BlockData (byte id)
    {
        this.id = id;
        lightLevel = 0f;
        neighbors = new bool[8];
        for (int i = 0; i < 8; i++)
        {
            neighbors[i] = false;
        }
    }
}