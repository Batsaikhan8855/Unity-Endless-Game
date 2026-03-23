using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class RoadSpawner : MonoBehaviour
{
    public GameObject roadTilePrefab;
    public GameObject[] obstaclePrefabs;
    public GameObject coinPrefab;

    public float tileLength = 20f;
    public int tilesAhead = 8;
    public float laneWidth = 3.5f;
    public int totalLanes = 3;
    public float scrollSpeed = 15f;
    public float speedIncreaseRate = 0.3f;
    public float maxSpeed = 30f;

    private float currentSpeed;
    private List<GameObject> activeTiles = new List<GameObject>();
    private float spawnZ = -40f;
    private float safeZone = 40f;
    private Transform player;

    void Start()
    {
        currentSpeed = scrollSpeed;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        for (int i = 0; i < tilesAhead; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {
        currentSpeed = Mathf.Min(currentSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);

        // Бүх tile-уудыг хөдөлгөх
        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
            if (activeTiles[i] == null) { activeTiles.RemoveAt(i); continue; }
            activeTiles[i].transform.position += Vector3.back * currentSpeed * Time.deltaTime;

            // Хойш хэт явсан tile устгах
            if (activeTiles[i].transform.position.z < -tileLength * 2)
            {
                Destroy(activeTiles[i]);
                activeTiles.RemoveAt(i);
                SpawnTile();
            }
        }
    }

    void SpawnTile()
    {
        if (roadTilePrefab == null) return;

        GameObject tile = Instantiate(roadTilePrefab, new Vector3(0, 0.01f, spawnZ), Quaternion.identity);
        activeTiles.Add(tile);

        // Safe zone дээр obstacle/coin гаргахгүй
        if (spawnZ > safeZone)
        {
            SpawnObstacle(spawnZ);
            SpawnCoins(spawnZ);
        }

        spawnZ += tileLength;
    }

    void SpawnObstacle(float z)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;
        if (Random.value > 0.4f) return;

        int blockedLane = Random.Range(0, totalLanes);
        float x = GetLaneX(blockedLane);
        float obsZ = z + Random.Range(tileLength * 0.3f, tileLength * 0.7f);

        // Санамсаргүй obstacle сонгох
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        if (prefab == null) return;

        GameObject obs = Instantiate(prefab, new Vector3(x, 0.5f, obsZ), Quaternion.identity);
        activeTiles.Add(obs);
    }

    void SpawnCoins(float z)
    {
        if (coinPrefab == null) return;
        if (Random.value > 0.6f) return;

        int lane = Random.Range(0, totalLanes);
        float x = GetLaneX(lane);

        for (int i = 0; i < 5; i++)
        {
            float coinZ = z + 2f + i * 2f;
            GameObject coin = Instantiate(coinPrefab, new Vector3(x, 0.5f, coinZ), Quaternion.identity);
            activeTiles.Add(coin);
        }
    }

    float GetLaneX(int lane)
    {
        return (lane - (totalLanes - 1) / 2f) * laneWidth;
    }

    public float GetCurrentSpeed() => currentSpeed;
}