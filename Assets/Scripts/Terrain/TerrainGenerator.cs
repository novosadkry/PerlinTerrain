using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(GameObjectPool))]
public class TerrainGenerator : MonoBehaviour
{
    public GameObject player;

    [Header("Render")]
    public int renderDistance;
    public float spawnTick = .05f;
    public float despawnTick = .05f;

    [Header("Chunk Settings")]
    public int seed;
    public int width;
    public int height;
    public int depth;

    [Header("Material Settings")]
    public Material grass;

    [Header("Generator Settings")]
    public int maxHeight = 10;
    public float freq = 10f;

    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    private Queue<Vector2Int> chunkQueue = new Queue<Vector2Int>();
    private GameObjectPool chunkObjectPool;

    public Vector2Int CurrentChunk
    {
        get
        {
            return new Vector2Int(
                Mathf.FloorToInt(player.transform.position.x / width),
                Mathf.FloorToInt(player.transform.position.z / depth)
            );
        }
    }

    void Awake()
    {
        chunkObjectPool = GetComponent<GameObjectPool>();
        chunkObjectPool.CanGrow = true;
        chunkObjectPool.CanShrink = true;

        StartCoroutine(DespawnTerrain());
        StartCoroutine(SpawnQueuedTerrain());

        StartCoroutine(DestroyUnusedChunkObjects());
    }

    void Update()
    {
        EnqueueTerrain();
    }

    void EnqueueTerrain()
    {
        int columns = (renderDistance * 2) + 1;
        int rows = (renderDistance * 2) + 1;

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector2Int diff = new Vector2Int(i - ((columns - 1) / 2), j - ((rows - 1) / 2));
                Vector2Int chunkPos = CurrentChunk + diff;

                if (!chunks.ContainsKey(chunkPos) && !chunkQueue.Contains(chunkPos))
                    chunkQueue.Enqueue(chunkPos);
            }
        }
    }

    IEnumerator SpawnQueuedTerrain()
    {
        while (true)
        {
            if (chunkQueue.Count > 0)
            {
                Vector2Int chunkPos = chunkQueue.Dequeue();

                if (isDistant(chunkPos, CurrentChunk, renderDistance))
                    continue;

                chunks.Add(chunkPos, CreateChunk(chunkPos));
            }

            yield return new WaitForSeconds(spawnTick);
        }
    }

    IEnumerator DespawnTerrain()
    {
        while (true)
        {
            List<Chunk> toRemove = new List<Chunk>();

            foreach (Chunk chunk in chunks.Values)
            {
                if (isDistant(chunk.ChunkPos, CurrentChunk, renderDistance))
                {
                    if (chunk != null)
                        chunk.gameObject.SetActive(false);

                    toRemove.Add(chunk);
                }
            }

            foreach (Chunk chunk in toRemove)
                chunks.Remove(chunk.ChunkPos);

            yield return new WaitForSeconds(despawnTick);
        }
    }

    IEnumerator DestroyUnusedChunkObjects()
    {
        while (true)
        {
            chunkObjectPool.RemoveUnused();
            yield return new WaitForSeconds(60);
        }
    }

    Chunk CreateChunk(Vector2Int chunkPos)
    {
        GameObject chunkObject = chunkObjectPool.GetPooled();
        chunkObject.name = $"Chunk-{chunkPos.x}-{chunkPos.y}";
        chunkObject.SetActive(true);

        Vector3 chunkWorldPos = new Vector3(chunkPos.x * width, 0, chunkPos.y * depth);

        chunkObject.transform.position = chunkWorldPos;

        Chunk chunk = chunkObject.GetComponent<Chunk>();
        chunk.ChunkPos = chunkPos;

        chunk.Width = width;
        chunk.Height = height;
        chunk.Depth = depth;

        chunk.grass = grass;

        chunk.MaxHeight = maxHeight;
        chunk.Freq = freq;

        chunk.Seed = seed;

        chunk.GenerateChunkMap();
        chunk.GenerateChunkMesh();

        return chunk;
    }

    bool isDistant(Vector2Int a, Vector2Int b, float distance)
    {
        return 
            Mathf.Abs(a.x - b.x) > distance ||
            Mathf.Abs(a.y - b.y) > distance;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(25, 25, 150, 50), $"Current chunk: {CurrentChunk.x}, {CurrentChunk.y}");
    }

    void OnValidate()
    {
        if (maxHeight >= height)
            Debug.LogWarning("Invalid maxHeight. Cannot be higher than or equal to chunk height");
    }
}
