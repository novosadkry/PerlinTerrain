using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMesh
{
    public enum Face
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,
        FRONT,
        BACK
    }

    public const int textureAtlasSize = 4;

    private int triangleIndex = 0;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    private Mesh mesh = new Mesh();

    public void AddMeshFace(Face face, Vector3 pos)
    {
        for (int i = 0; i < 6; i++)
        {
            int vertIndex = cubeTriangles[(int)face, i];

            vertices.Add(cubeVerts[vertIndex] + pos);
            triangles.Add(triangleIndex++);
        }
    }

    public Mesh ConstructMesh()
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        return mesh;
    }

    public void AddTexture(int index)
    {
        for (int i = 0; i < 6; i++)
        {
            int x = index % textureAtlasSize;
            int y = index / textureAtlasSize;

            Vector2 normalizedUV = cubeUVs[i] / textureAtlasSize;
            Vector2 margin = new Vector2(1.0f / textureAtlasSize * x, 1.0f / textureAtlasSize * y);

            uvs.Add(normalizedUV + margin);
        }
    }

    public static Vector3[] cubeVerts = new Vector3[8] {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1)
    };

    public static readonly int[,] cubeTriangles = new int[6, 6] {
        { 3, 7, 6, 3, 6, 2 }, // TOP
        { 0, 5, 4, 0, 1, 5 }, // BOTTOM
        { 0, 4, 7, 0, 7, 3 }, // LEFT
        { 5, 1, 2, 5, 2, 6 }, // RIGHT
        { 1, 0, 3, 1, 3, 2 }, // FRONT
        { 4, 5, 6, 4, 6, 7 }  // BACK
    };

    public static Vector2[] cubeUVs = new Vector2[6] {
        new Vector2(1, 0),
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    };

    public static Vector3[] facesDirections = new Vector3[6] {
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0),
        new Vector3(-1, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, 0, -1),
        new Vector3(0, 0, 1)
    };
}
