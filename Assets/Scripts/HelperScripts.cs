using UnityEngine;
using Random = UnityEngine.Random;

public class CoinRotate : MonoBehaviour
{
    public float rotationSpeed = 180f;
    public Vector3 rotationAxis = Vector3.up;
    public bool enableFloating = true;
    public float floatAmplitude = 0.15f;
    public float floatFrequency = 2f;

    private Vector3 startPosition;
    private float randomOffset;

    void OnEnable()
    {
        startPosition = transform.position;
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
        if (enableFloating)
        {
            float yOffset = Mathf.Sin(Time.time * floatFrequency + randomOffset) * floatAmplitude;
            transform.position = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);
        }
    }
}

public class ObstacleScroll : MonoBehaviour
{
    private PlayerCarController player;
    private bool hasPassedPlayer = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.GetComponent<PlayerCarController>();
    }

    void Update()
    {
        if (player == null || !player.IsAlive()) return;
        float speed = player.GetCurrentSpeed();
        transform.position += Vector3.back * speed * Time.deltaTime;
        if (transform.position.z < -20f && !hasPassedPlayer)
        {
            hasPassedPlayer = true;
            gameObject.SetActive(false);
        }
    }
}