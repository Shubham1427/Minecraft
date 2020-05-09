using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Clouds : MonoBehaviour
{
    bool[,,] clouds;
    private List<Vector3> verticesCoords = new List<Vector3>();
    private List<int> trianglesVertices = new List<int>();
    private int verticesIndex = 0;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    Material mat;

    public void InitCloud(float seed, Material material)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        mat = material;
        int size = GeneralSettings.chunkWidth;
        clouds = new bool[size, 5, size];
        for (int x=0; x<size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < 5; y++)
                {
                    DecideCloud(x, y, z, seed, size);
                }
            }
        }
        for (int x=0; x<size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < 5; y++)
                {
                    if (clouds[x, y, z])
                        CreateCloudData(x, y, z, size);
                }
            }
            if (x == 150)
            {
                Debug.Log("A");
            }
        }
        CreateCloudMesh();
    }

    void DecideCloud(int _x, int _y, int _z, float seed, int size)
    {
        float x = (_x + transform.position.x) * GeneralSettings.cloudScale / (size * GeneralSettings.worldSizeInChunks);
        float y = _y / 4f;
        float z = (_z + transform.position.z) * GeneralSettings.cloudScale/ (size * GeneralSettings.worldSizeInChunks);

        if (GeneralSettings.GenerateNoise3D(x + seed + 12.375f, y + seed, z + seed) > 0.56f)
            clouds[_x, _y, _z] = true;
        else
            clouds[_x, _y, _z] = false;
    }

    void CreateCloudData(int x, int y, int z, int size)
    {
        Vector3Int coords = new Vector3Int(x, 132+y, z);
        for (int i = 0; i < 6; i++)
        {
            Vector3 adjacentBlockCoords = coords + BlockMeshData.faceIndex[i];


            if (CheckIndexInBounds((int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y-132, (int)adjacentBlockCoords.z, size))
            {
                if (clouds[(int)adjacentBlockCoords.x, (int)adjacentBlockCoords.y-132, (int)adjacentBlockCoords.z])
                    continue;
            }

            verticesCoords.Add(coords + BlockMeshData.verticesCoords[BlockMeshData.triangleVertices[i, 0]]);
            verticesCoords.Add(coords + BlockMeshData.verticesCoords[BlockMeshData.triangleVertices[i, 1]]);
            verticesCoords.Add(coords + BlockMeshData.verticesCoords[BlockMeshData.triangleVertices[i, 2]]);
            verticesCoords.Add(coords + BlockMeshData.verticesCoords[BlockMeshData.triangleVertices[i, 3]]);

            trianglesVertices.Add(verticesIndex);
            trianglesVertices.Add(verticesIndex + 1);
            trianglesVertices.Add(verticesIndex + 2);
            trianglesVertices.Add(verticesIndex + 2);
            trianglesVertices.Add(verticesIndex + 1);
            trianglesVertices.Add(verticesIndex + 3);
            verticesIndex += 4;
        }
    }

    bool CheckIndexInBounds(int x, int y, int z, int size)
    {
        return (x >= 0 && z >= 0 && x < size && z < size && y >= 0 && y<5);
    }

    private void CreateCloudMesh()
    {
        meshRenderer.material = mat;
        Mesh mesh = new Mesh
        {
            name = "CloudMesh",
            vertices = verticesCoords.ToArray(),
            triangles = trianglesVertices.ToArray(),
        };

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}
