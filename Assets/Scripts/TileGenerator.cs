using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileGenerator : MonoBehaviour
{
    public GameObject tileContainer;
    public GameObject fillTile;
    public GameObject pathTile;
    public GameObject rampStartTile;
    public GameObject rampEndTile;

    public int width = 20;
    public int height = 30;

    [Range(0, 30)]
    public int extraDepth = 15;

    public float tileSize = 1f;

    [Header("Generation Settings")]
    [Range(0.1f, 0.4f)]
    public float rampFrequency = 0.2f;

    [Range(2, 8)]
    public int minFlatLength = 3;

    [Range(5, 15)]
    public int maxFlatLength = 8;

    [Header("Spawners")]
    public GameObject enemySpawner;
    public GameObject playerSpawn;
    public float enemyChance = 0.1f;

    [Header("Optimization")]
    public int tilesPerFrame = 150;

    private enum TileType { Empty, Fill, Path, RampStart, RampEnd, EnemySpawner }
    private TileType[,] tileTypes;

    private List<GameObject> tilePool = new List<GameObject>();
    private List<Vector2Int> spawnerPositions = new List<Vector2Int>();

    void Start()
    {
        StartCoroutine(GenerateMapAsync());
    }

    IEnumerator GenerateMapAsync()
    {
        int totalHeight = height + extraDepth;
        tileTypes = new TileType[width, totalHeight];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < totalHeight; y++)
            {
                tileTypes[x, y] = TileType.Empty;
            }
        }

        GenerateContinuousPath();

        FillBelowPath();
        MarkEnemySpawners();

        yield return StartCoroutine(InstantiateTilesAsync());
        SetPlayerSpawn();
    }

    void GenerateContinuousPath()
    {
        int currentX = 0;
        int currentY = extraDepth + 2; 

        while (currentX < width)
        {
            bool placeRamp = Random.value < rampFrequency && currentX < width - 1;

            if (placeRamp && currentY < height - 2)
            {
                tileTypes[currentX, currentY + 1] = TileType.RampStart;
                tileTypes[currentX + 1, currentY + 1] = TileType.RampEnd;
                currentX += 2;
                currentY += 1;
            }
            else
            {
                int flatLength = Random.Range(minFlatLength, maxFlatLength);

                for (int i = 0; i < flatLength && currentX < width; i++)
                {
                    tileTypes[currentX, currentY] = TileType.Path;
                    currentX++;
                }

                if(currentY > 1 && Random.value < 0.3f && currentX < width)
                {
                    currentY -= Random.Range(1, 3);
                    currentY = Mathf.Max(1, currentY);
                }
            }
        }
    }

    void FillBelowPath()
    {
        int totalHeight = height + extraDepth;
        for (int x = 0; x < width; x++)
        {
            for (int y = totalHeight - 1; y >= 0; y--)
            {
                if (tileTypes[x, y] != TileType.Empty)
                {
                    for (int fillY = y - 1; fillY >= 0; fillY--)
                    {
                        if (tileTypes[x, fillY] == TileType.Empty)
                        {
                            tileTypes[x, fillY] = TileType.Fill;
                        }
                    }
                    break;
                }
            }
        }
    }

    IEnumerator InstantiateTilesAsync()
    {
        int totalHeight = height + extraDepth;

        if (tileContainer != null)
        {
            foreach (var tile in tilePool)
            {
                tile.SetActive(false);
            }

            int tilesCreated = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < totalHeight; y++)
                {
                    if (tileTypes[x, y] == TileType.Empty) continue;

                    GameObject prefab = GetTilePrefab(x, y);
                    GameObject newTile = null;

                    foreach (var t in tilePool)
                    {
                        if (!t.activeInHierarchy && t.name.Contains(prefab.name))
                        {
                            newTile = t;
                            break;
                        }
                    }

                    if (newTile == null)
                    {
                        newTile = Instantiate(prefab, tileContainer.transform);
                        tilePool.Add(newTile);
                    }

                    newTile.transform.position = new Vector3(x * tileSize, y * tileSize, 1);
                    newTile.SetActive(true);

                    tilesCreated++;
                    if (tilesCreated >= tilesPerFrame)
                    {
                        tilesCreated = 0;
                        yield return null;
                    }
                }
            }

            foreach (var pos in spawnerPositions)
            {
                if (enemySpawner != null)
                {
                    GameObject spawner = Instantiate(enemySpawner, tileContainer.transform);
                    spawner.transform.position = new Vector3(pos.x * tileSize, pos.y * tileSize + 2f * tileSize, 1);
                }
            }
        }
    }

    void SetPlayerSpawn()
    {
        int spawnX = 8;
        int spawnY = FindSurfaceAtX(spawnX);

        if(playerSpawn != null)
        {
            Vector3 playerSpawnPosition = new Vector3(
                spawnX * tileSize,
                spawnY * tileSize + 2f,
                1
            );
            
            Instantiate(playerSpawn, playerSpawnPosition, Quaternion.identity);
        }
    }

   void MarkEnemySpawners()
   {
        spawnerPositions.Clear();
        for (int x = 20; x < width; x++)
        {
             for (int y = 0; y < height + extraDepth; y++)
            {
                 if (tileTypes[x, y] == TileType.Path && Random.value < enemyChance)
                 {
                     spawnerPositions.Add(new Vector2Int(x, y));
                }
            }
        }
   }

    int FindSurfaceAtX(int x)
    {
        for(int y = tileTypes.GetLength(1) - 1; y >= 0; y--)
        {
            if (tileTypes[x, y] == TileType.Path || 
                tileTypes[x, y] == TileType.RampStart || 
                tileTypes[x, y] == TileType.RampEnd)
            {
                return y;
            }
        }
        return -1;
    }

    GameObject GetTilePrefab(int x, int y)
    {
        switch (tileTypes[x, y])
        {
            case TileType.Path: return pathTile;
            case TileType.RampStart: return rampStartTile;
            case TileType.RampEnd: return rampEndTile;
            case TileType.Fill: return fillTile;
            case TileType.EnemySpawner: return fillTile;
            default: return fillTile;
        }
    }

    [ContextMenu("Regenerate Map")]
    public void RegenerateMap()
    {
        StopAllCoroutines();
        StartCoroutine(GenerateMapAsync());
    }
}