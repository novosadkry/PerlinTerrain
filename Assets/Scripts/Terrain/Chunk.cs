using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    [Header("Material Settings")]
    public Material grass;

    public Vector2Int ChunkPos { get; set; }

    public int Seed { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }
    public int Depth { get; set; }

    private int maxHeight;
    public int MaxHeight {
        get => maxHeight; 
        
        set
        {
            if (value >= Height)
            {
                Debug.LogError("Max height can't be higher than or equal to chunk height!");
                maxHeight = 0;
            }

            else
                maxHeight = value;
        }
    }

    public float Freq { get; set; }

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    private ChunkMesh chunkMesh;

    public byte[,,] blocks;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void GenerateChunkMap()
    {
        blocks = new byte[Width + 2, Height, Depth + 2];

        for (int x = 0; x < Width + 2; x++)
        {
            for (int z = 0; z < Depth + 2; z++)
            {
                float perlin = Mathf.PerlinNoise((ChunkPos.x * Width + x + Seed) / Freq, (ChunkPos.y * Depth + z + Seed) / Freq);
                perlin += Mathf.PerlinNoise((ChunkPos.x * Width + x + Seed) / Freq * 0.2f, (ChunkPos.y * Depth + z + Seed) / Freq * 0.2f);
                perlin -= Mathf.PerlinNoise((ChunkPos.x * Width + x + Seed) / Freq * 0.5f, (ChunkPos.y * Depth + z + Seed) / Freq * 0.5f) * 1.5f;
                perlin = Mathf.Clamp01(perlin);
                
                int y = Mathf.RoundToInt(perlin * MaxHeight);

                blocks[x, y, z] = 1;

                for (int i = 0; i < y; i++)
                    blocks[x, i, z] = 1;
            }
        }
    }

    public void GenerateChunkMesh()
    {
        chunkMesh = new ChunkMesh();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    Vector3Int relatPos = new Vector3Int(x + 1, y, z + 1);

                    if (blocks[relatPos.x, relatPos.y, relatPos.z] != 0)
                    {
                        foreach (ChunkMesh.Face face in Enum.GetValues(typeof(ChunkMesh.Face)))
                        {
                            Vector3Int targetBlock = Vector3Int.FloorToInt(relatPos + ChunkMesh.facesDirections[(int)face]);

                            if (IsAir(targetBlock))
                            {
                                chunkMesh.AddMeshFace(face, relatPos);

                                if (relatPos.y > maxHeight * 0.75f)
                                    chunkMesh.AddTexture(1);
                                else if (relatPos.y == 0)
                                    chunkMesh.AddTexture(2);
                                else
                                    chunkMesh.AddTexture(0);
                            }
                        }
                    }
                }
            }
        }

        Mesh mesh = chunkMesh.ConstructMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        meshRenderer.material = grass;
    }

    public bool IsAir(Vector3Int pos)
    {
        return IsValidPos(pos) && blocks[pos.x, pos.y, pos.z] == 0;
    }

    public bool IsValidPos(Vector3Int pos)
    {
        return pos.x > -1 && pos.x < Width + 2 &&
            pos.y > -1 && pos.y < Height &&
            pos.z > -1 && pos.z < Depth + 2;
    }
}
