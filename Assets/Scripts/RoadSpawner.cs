using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class RoadSpawner : MonoBehaviour
{
    public GameObject roadTilePrefab;
    public GameObject[] obstaclePrefabs;
    public GameObject coinPrefab;

    public float tileLength = 20f;
    public int tilesAhead = 12;
    public float laneWidth = 2.5f;
    public int totalLanes = 3;
    public float scrollSpeed = 12f;
    public float speedIncreaseRate = 0.2f;
    public float maxSpeed = 25f;

    private float currentSpeed;
    private List<GameObject> activeTiles = new List<GameObject>();
    private List<GameObject> activeObjects = new List<GameObject>();

    // Tile-уудын Z байрлалыг тусдаа хадгалах
    private List<float> tileZPositions = new List<float>();
    private float nextSpawnZ = 0f;
    private float safeZone = 30f;
    private float roadHalfWidth;

    void Start()
    {
        currentSpeed = scrollSpeed;
        nextSpawnZ = 0f;
        roadHalfWidth = (totalLanes - 1) / 2f * laneWidth;

        // Эхлэлийн tile-уудыг spawn хийх
        for (int i = 0; i < tilesAhead; i++)
            SpawnTile();
    }

    void Update()
    {
        currentSpeed = Mathf.Min(currentSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);
        float move = currentSpeed * Time.deltaTime;

        // Tile хөдөлгөх
        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
            if (activeTiles[i] == null) { activeTiles.RemoveAt(i); tileZPositions.RemoveAt(i); continue; }
            activeTiles[i].transform.position += Vector3.back * move;
            tileZPositions[i] -= move;

            // Хойш хэт явсан tile устгаж шинэ spawn хийх
            if (tileZPositions[i] < -tileLength * 3)
            {
                Destroy(activeTiles[i]);
                activeTiles.RemoveAt(i);
                tileZPositions.RemoveAt(i);
                SpawnTile();
            }
        }

        // Obstacle/Coin хөдөлгөх
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            if (activeObjects[i] == null) { activeObjects.RemoveAt(i); continue; }
            activeObjects[i].transform.position += Vector3.back * move;
            if (activeObjects[i].transform.position.z < -tileLength * 2)
            {
                Destroy(activeObjects[i]);
                activeObjects.RemoveAt(i);
            }
        }

        // Tile тооцоолоход nextSpawnZ хөдлөхгүй тул тусад нь хасна
        nextSpawnZ -= move;
    }

    void SpawnTile()
    {
        if (roadTilePrefab == null) return;

        GameObject tile = Instantiate(roadTilePrefab, new Vector3(0, 0, nextSpawnZ), Quaternion.identity);
        activeTiles.Add(tile);
        tileZPositions.Add(nextSpawnZ);

        if (nextSpawnZ > safeZone)
        {
            SpawnObstacle(nextSpawnZ);
            SpawnCoins(nextSpawnZ);
        }

        nextSpawnZ += tileLength;
    }

    void SpawnObstacle(float z)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;
        if (Random.value > 0.45f) return;

        int lane = Random.Range(0, totalLanes);
        float x = GetLaneX(lane);
        float obsZ = z + tileLength * 0.5f;

        int idx = Random.Range(0, obstaclePrefabs.Length);
        GameObject prefab = obstaclePrefabs[idx];
        if (prefab == null) return;

        string n = prefab.name.ToLower();
        float yPos = 0.6f;
        Vector3 spawnPos;

        if (n.Contains("high"))
        {
            yPos = 2.0f;
            spawnPos = new Vector3(0, yPos, obsZ);
        }
        else if (n.Contains("low"))
        {
            yPos = 0.0f;
            spawnPos = new Vector3(0, yPos, obsZ);
        }
        else
        {
            spawnPos = new Vector3(x, yPos, obsZ);
        }

        GameObject obs = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Barrier өргөнийг замтай тэнцүүлэх
        if (n.Contains("high") || n.Contains("low"))
        {
            Vector3 s = obs.transform.localScale;
            obs.transform.localScale = new Vector3(totalLanes * laneWidth, s.y, s.z);
        }

        activeObjects.Add(obs);
    }

    void SpawnCoins(float z)
    {
        if (coinPrefab == null) return;
        if (Random.value > 0.55f) return;

        int lane = Random.Range(0, totalLanes);
        float x = GetLaneX(lane);

        for (int i = 0; i < 5; i++)
        {
            float coinZ = z + 2f + i * 2.5f;
            if (coinZ < z + tileLength - 2f)
            {
                GameObject coin = Instantiate(coinPrefab, new Vector3(x, 0.5f, coinZ), Quaternion.identity);
                activeObjects.Add(coin);
            }
        }
    }

    float GetLaneX(int lane)
    {
        return (lane - (totalLanes - 1) / 2f) * laneWidth;
    }

    public float GetCurrentSpeed() => currentSpeed;
}