using UnityEngine;

/// <summary>
/// RoadScroller - Бүх зам tile-уудыг тоглогч руу хөдөлгөх
/// Энэ script нь RoadSpawner-тай хамт ажиллана
/// RoadSpawner GameObject дээр нэмнэ
/// </summary>
public class RoadScroller : MonoBehaviour
{
    public float scrollSpeed = 15f;
    public float speedIncreaseRate = 0.3f;
    public float maxSpeed = 30f;

    private float currentSpeed;

    void Start()
    {
        currentSpeed = scrollSpeed;
    }

    void Update()
    {
        currentSpeed = Mathf.Min(currentSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);

        // Hierarchy дотор байгаа бүх RoadTile(Clone)-уудыг хөдөлгөх
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("RoadTile");
        foreach (GameObject tile in tiles)
        {
            tile.transform.position += Vector3.back * currentSpeed * Time.deltaTime;
        }

        // Obstacle-уудыг хөдөлгөх
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject obs in obstacles)
        {
            obs.transform.position += Vector3.back * currentSpeed * Time.deltaTime;
        }

        // Coin-уудыг хөдөлгөх
        GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject coin in coins)
        {
            coin.transform.position += Vector3.back * currentSpeed * Time.deltaTime;
        }
    }

    public float GetCurrentSpeed() => currentSpeed;
}