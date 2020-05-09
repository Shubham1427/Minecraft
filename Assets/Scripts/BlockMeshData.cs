using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockMeshData
{
    public static readonly Vector3[] verticesCoords =
    {
        new Vector3 (0f, 0f, 0f),
        new Vector3 (0f, 0f, 1f),
        new Vector3 (0f, 1f, 1f),
        new Vector3 (0f, 1f, 0f),
        new Vector3 (1f, 0f, 0f),
        new Vector3 (1f, 0f, 1f),
        new Vector3 (1f, 1f, 1f),
        new Vector3 (1f, 1f, 0f),
    };

    public static readonly int[,] triangleVertices =
    {
        {1, 2, 0, 3},   //Right
        {4, 7, 5, 6},   //Left
        {5, 6, 1, 2},   //Front
        {0, 3, 4, 7},   //Back
        {6, 7, 2, 3},   //Top
        {1, 0, 5, 4},   //Bottom
    };

    public static readonly Vector3Int[] faceIndex =
    {
        new Vector3Int (-1, 0, 0),
        new Vector3Int (1, 0, 0),
        new Vector3Int (0, 0, 1),
        new Vector3Int (0, 0, -1),
        new Vector3Int (0, 1, 0),
        new Vector3Int (0, -1, 0),
    };

    public static readonly Vector3Int[] neighbourIndex =
    {
        new Vector3Int (-1, 0, 0),
        new Vector3Int (1, 0, 0),
        new Vector3Int (0, 0, 1),
        new Vector3Int (0, 0, -1),
        new Vector3Int (-1, 0, -1),
        new Vector3Int (1, 0, 1),
        new Vector3Int (1, 0, -1),
        new Vector3Int (-1, 0, 1),
    };

    public static readonly Vector3Int[] diagonalNeighboursSideBlocks =
    {
        new Vector3Int (0, 0, 1),
        new Vector3Int (1, 0, 0),
        new Vector3Int (0, 0, -1),
        new Vector3Int (-1, 0, 0),
        new Vector3Int (0, 0, 1),
        new Vector3Int (-1, 0, 0),
        new Vector3Int (0, 0, -1),
        new Vector3Int (1, 0, 0),
    };

    public static readonly int[] neighbourDistance =
    {
        10, 10, 10, 10, 14, 14, 14, 14,
    };
}