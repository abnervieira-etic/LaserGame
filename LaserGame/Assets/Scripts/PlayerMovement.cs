using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 8f;
    
    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 currentVelocity;
    private Transform camTransform;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        camTransform = UnityEngine.Camera.main.transform;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 camForward = camTransform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = camTransform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 input = (camRight * h) + (camForward * v);
        moveInput = Vector3.ClampMagnitude(input, 1f);
    }

    void FixedUpdate()
    {
        Vector3 targetVelocity = new Vector3(moveInput.x, 0f, moveInput.z) * speed;

        // Inércia: acelera rápido, desacelera suave
        float smoothTime = moveInput.sqrMagnitude > 0.001f ? 1f / acceleration : 1f / deceleration;
        Vector3 smoothedVelocity = Vector3.SmoothDamp(
            new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z),
            targetVelocity,
            ref currentVelocity,
            smoothTime
        );

        // Aplica mantendo Y da física (gravidade, pulo)
        rb.linearVelocity = new Vector3(smoothedVelocity.x, rb.linearVelocity.y, smoothedVelocity.z);

        // Rotação
        if (moveInput.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        //Debug.Log($"Input: {moveInput} | Velocity: {rb.linearVelocity} | Pos: {transform.position}");
    }
}