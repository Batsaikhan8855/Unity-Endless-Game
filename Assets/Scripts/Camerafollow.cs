using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 positionOffset = new Vector3(0f, 8f, -10f);
    public Vector3 rotationAngle = new Vector3(45f, 0f, 0f);
    public float followSmoothness = 10f;

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        transform.rotation = Quaternion.Euler(rotationAngle);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + positionOffset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref velocity,
            1f / followSmoothness
        );

        // Rotation тогтмол байна - LookAt хэрэглэхгүй
        transform.rotation = Quaternion.Euler(rotationAngle);
    }
}