using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UObject = UnityEngine.Object;

public class PlayerCarController : MonoBehaviour
{
    [Header("Lane тохиргоо")]
    public int totalLanes = 3;
    public float laneWidth = 3.5f;
    public float laneChangeSpeed = 10f;

    [Header("Хурдны тохиргоо")]
    public float forwardSpeed = 15f;
    public float speedIncreaseRate = 0.5f;
    public float maxSpeed = 35f;

    [Header("Үсрэх тохиргоо")]
    public float jumpForce = 7f;

    [Header("Бөхийх тохиргоо")]
    public float crouchScaleY = 0.4f;

    [Header("Swipe тохиргоо")]
    public float swipeThreshold = 50f;

    // Lane
    private int currentLane = 1;
    private float targetX;
    private bool canChangeLane = true;

    // Хурд
    private float currentSpeed;

    // Үсрэх
    private bool isGrounded = true;

    // Бөхийх
    private bool isCrouching = false;
    private Vector3 normalScale;

    // Touch
    private Vector2 touchStartPos;
    private bool isTouching = false;

    // Амьд эсэх
    private bool isAlive = true;

    // Компонентүүд
    private Rigidbody rb;
    private GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = UObject.FindFirstObjectByType<GameManager>();

        normalScale = transform.localScale;
        currentLane = 1;
        targetX = GetLaneXPosition(currentLane);
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        currentSpeed = forwardSpeed;
    }

    void Update()
    {
        if (!isAlive) return;

        currentSpeed = Mathf.Min(currentSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);

        HandleKeyboardInput();
        HandleTouchInput();

        // Lane хөдөлгөөн
        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        if (Mathf.Abs(transform.position.x - targetX) < 0.05f)
            canChangeLane = true;
    }

    void HandleKeyboardInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Зүүн баруун
        if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
            MoveLeft();
        else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
            MoveRight();

        // PgUp = үсрэх
        if (kb.pageUpKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
            Jump();

        // PgDn = бөхийх / суух
        if (kb.pageDownKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
            Crouch();

        if (kb.pageDownKey.wasReleasedThisFrame || kb.downArrowKey.wasReleasedThisFrame)
            StandUp();
    }

    void HandleTouchInput()
    {
        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                touchStartPos = mouse.position.ReadValue();
                isTouching = true;
            }
            else if (mouse.leftButton.wasReleasedThisFrame && isTouching)
            {
                Vector2 delta = mouse.position.ReadValue() - touchStartPos;
                ProcessSwipe(delta);
                isTouching = false;
            }
        }

        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            foreach (var touch in touchscreen.touches)
            {
                var phase = touch.phase.ReadValue();
                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    touchStartPos = touch.position.ReadValue();
                    isTouching = true;
                }
                else if (phase == UnityEngine.InputSystem.TouchPhase.Ended && isTouching)
                {
                    Vector2 delta = touch.position.ReadValue() - touchStartPos;
                    ProcessSwipe(delta);
                    isTouching = false;
                }
                break;
            }
        }
    }

    void ProcessSwipe(Vector2 delta)
    {
        if (delta.magnitude < swipeThreshold) return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x < 0) MoveLeft();
            else MoveRight();
        }
        else
        {
            if (delta.y > 0) Jump();
            else { Crouch(); Invoke("StandUp", 0.5f); }
        }
    }

    public void Jump()
    {
        if (!isGrounded || rb == null) return;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    public void Crouch()
    {
        if (isCrouching) return;
        isCrouching = true;
        transform.localScale = new Vector3(normalScale.x, crouchScaleY, normalScale.z);
        // Доошоо шилжих
        transform.position = new Vector3(transform.position.x, crouchScaleY / 2f, transform.position.z);
    }

    public void StandUp()
    {
        if (!isCrouching) return;
        isCrouching = false;
        transform.localScale = normalScale;
        transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
    }

    public void MoveLeft()
    {
        if (!canChangeLane || currentLane <= 0) return;
        currentLane--;
        targetX = GetLaneXPosition(currentLane);
        canChangeLane = false;
    }

    public void MoveRight()
    {
        if (!canChangeLane || currentLane >= totalLanes - 1) return;
        currentLane++;
        targetX = GetLaneXPosition(currentLane);
        canChangeLane = false;
    }

    float GetLaneXPosition(int lane)
    {
        return (lane - (totalLanes - 1) / 2f) * laneWidth;
    }

    void OnCollisionEnter(Collision col)
    {
        if (!col.gameObject.CompareTag("Obstacle"))
            isGrounded = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle")) Die();
        else if (other.CompareTag("Coin")) CollectCoin(other.gameObject);
    }

    void CollectCoin(GameObject coin)
    {
        if (gameManager != null) gameManager.AddCoin();
        coin.SetActive(false);
    }

    void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        if (gameManager != null) gameManager.GameOver();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(Vector3.up * 5f + Vector3.back * 3f, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);
        }
    }

    public float GetCurrentSpeed() => currentSpeed;
    public bool IsAlive() => isAlive;
}