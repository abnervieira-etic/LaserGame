using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float collisionOffset = 0.3f;
    [SerializeField] private LayerMask collisionLayers;
    private float pitch = 15f;
    private float yaw = 0f;
    private Vector3 currentVelocity;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Arraste o Player para o campo 'Target'.");
            enabled = false;
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        // Mouse
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -30f, 60f);

        // Zoom com scroll  
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * 2f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Posição desejada
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position - (rotation * Vector3.forward * distance);

        // Colisão
        Vector3 direction = desiredPosition - target.position;
        if (Physics.Raycast(target.position, direction.normalized, out RaycastHit hit, direction.magnitude, collisionLayers))
        {
            desiredPosition = hit.point + direction.normalized * collisionOffset;
        }

        // Suavização
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);
        transform.LookAt(target.position + Vector3.up * 1.5f);
        
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}