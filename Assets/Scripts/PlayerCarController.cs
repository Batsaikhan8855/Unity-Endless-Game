using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UObject = UnityEngine.Object;

public class PlayerCarController : MonoBehaviour
{
    [Header("Lane тохиргоо")]
    public int totalLanes = 3;
    public float laneWidth = 2.5f;
    public float laneChangeSpeed = 10f;

    [Header("Хурдны тохиргоо")]
    public float forwardSpeed = 12f;
    public float speedIncreaseRate = 0.2f;
    public float maxSpeed = 25f;

    [Header("Үсрэх тохиргоо")]
    public float jumpForce = 6f;

    [Header("Бөхийх тохиргоо")]
    public float crouchScaleY = 0.4f;

    [Header("Swipe тохиргоо")]
    public float swipeThreshold = 50f;

    private int currentLane = 1;
    private float targetX;
    private bool canChangeLane = true;
    private float currentSpeed;
    private bool isGrounded = true;
    private bool isCrouching = false;
    private Vector3 normalScale;
    private Vector2 touchStartPos;
    private bool isTouching = false;
    private bool isAlive = true;
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

        // Rigidbody тохиргоо
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionZ |
                           RigidbodyConstraints.FreezeRotationX |
                           RigidbodyConstraints.FreezeRotationY |
                           RigidbodyConstraints.FreezeRotationZ;
            rb.linearDamping = 0f;
            rb.angularDamping = 10f;
        }
    }

    void Update()
    {
        if (!isAlive) return;
        currentSpeed = Mathf.Min(currentSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);
        HandleKeyboardInput();
        HandleTouchInput();

        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        if (Mathf.Abs(transform.position.x - targetX) < 0.05f)
            canChangeLane = true;

        // Газарт буусан эсэхийг Y velocity-р шалгах
        if (rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.05f && transform.position.y <= 1.2f)
            isGrounded = true;
    }

    void HandleKeyboardInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
            MoveLeft();
        else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
            MoveRight();

        if (kb.upArrowKey.wasPressedThisFrame || kb.pageUpKey.wasPressedThisFrame)
            Jump();

        if (kb.downArrowKey.wasPressedThisFrame || kb.pageDownKey.wasPressedThisFrame)
            Crouch();
        if (kb.downArrowKey.wasReleasedThisFrame || kb.pageDownKey.wasReleasedThisFrame)
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
                ProcessSwipe(mouse.position.ReadValue() - touchStartPos);
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
                    ProcessSwipe(touch.position.ReadValue() - touchStartPos);
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
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }

    public void Crouch()
    {
        if (isCrouching) return;
        isCrouching = true;
        transform.localScale = new Vector3(normalScale.x, crouchScaleY, normalScale.z);
    }

    public void StandUp()
    {
        if (!isCrouching) return;
        isCrouching = false;
        transform.localScale = normalScale;
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
        if (col.gameObject.CompareTag("Obstacle")) return;
        foreach (ContactPoint contact in col.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
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