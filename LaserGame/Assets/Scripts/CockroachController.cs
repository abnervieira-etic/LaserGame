using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CockroachController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float topExitForce = 5f;

    [Header("Visual Mesh")]
    [SerializeField] private Transform meshTransform;
    [SerializeField] private float modelRotationSpeed = 10f;

    [Header("Wall Detection")]
    [SerializeField] private float wallCheckDistance = 1.0f;
    [SerializeField] private float raycastDis = 1.0f;
    [SerializeField] private float leftRaycast = 1.0f;
    [SerializeField] private float rightRaycast = 1.0f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Ground & Physics")]
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jumping")]
    [SerializeField] private float minJumpForce = 10f;
    [SerializeField] private float maxJumpForce = 20f;
    [SerializeField] private float maxJumpChargeTime = 1.5f;
    private float jumpChargeTime;
    private float requestedJumpForce;
    private bool isChargingJump;
    private bool jumpButtonHeld;
    private bool jumpRequested;

    private Rigidbody rb;
    private bool isClimbing = false;
    private Vector3 wallNormal;
    private Vector3 wallUp;
    private Vector3 wallRight;
    private Vector2 inputBuffer;
    private Vector3 moveDirection;
    private Quaternion targetRotation;
    private Quaternion meshTargetRotation;
    private bool isGrounded;
    private bool wasClimbing = false;
    private Vector3 lastMeshForward;

    public static CockroachController instance;
    void Awake()
    {
        instance = this;
        lastMeshForward = transform.forward;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        inputBuffer.x = Input.GetAxisRaw("Horizontal");
        inputBuffer.y = Input.GetAxisRaw("Vertical");

        Vector3 rayOrigin = transform.position + Vector3.up * raycastDis;
        bool wallAhead = Raycasts(rayOrigin, out RaycastHit hit);

        isGrounded = GroundedCheck();
        HandleStateTransition(wallAhead, hit);
        Jump();
    }
    void FixedUpdate()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, modelRotationSpeed * Time.fixedDeltaTime);
        CalculateMovementAndRotation();
        
        if (isClimbing)
        {
            rb.linearVelocity = moveDirection;
        }
        else if (!isClimbing && jumpRequested && isGrounded)
        {
            rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);
        }
        else if (!isClimbing && wasClimbing)
        {
            rb.linearVelocity = new Vector3(moveDirection.x, 0f, moveDirection.z); 
            wasClimbing = false;
        }
        

        if (jumpRequested)
        {
            Debug.Log("Jump requested with force: " + requestedJumpForce);
            rb.AddForce(Vector3.up * requestedJumpForce, ForceMode.Impulse);
            jumpRequested = false;
        }

        if (jumpButtonHeld && jumpChargeTime < maxJumpChargeTime)
        {
            float additionalForce = maxJumpChargeTime > 0f
                ? (maxJumpForce - minJumpForce) / maxJumpChargeTime
                : 0f;

            rb.AddForce(Vector3.up * additionalForce * Time.fixedDeltaTime, ForceMode.Impulse);
        }

        // Rotação da mesh (visual apenas)
        if (meshTransform != null)
        {
            meshTransform.rotation = Quaternion.Slerp(meshTransform.rotation, meshTargetRotation, modelRotationSpeed * Time.fixedDeltaTime);
        }
    }
    bool Raycasts(Vector3 origin, out RaycastHit hit)
    {
        Vector3 forward = transform.forward;
        Vector3 left = Quaternion.AngleAxis(leftRaycast, Vector3.up) * forward;
        Vector3 right = Quaternion.AngleAxis(rightRaycast, Vector3.up) * forward;
        Vector3 back = Quaternion.AngleAxis(0f, Vector3.up) * -forward;

        Vector3 groundedOrigin = transform.position + Vector3.down * raycastDis;
        
        if (Physics.Raycast(groundedOrigin, Vector3.down, out hit, groundCheckDistance, wallLayer))
            return true;

        if (Physics.Raycast(origin, forward, out hit, wallCheckDistance, wallLayer))
            return true;

        if (Physics.Raycast(origin, left, out hit, wallCheckDistance, wallLayer))
            return true;

        if (Physics.Raycast(origin, right, out hit, wallCheckDistance, wallLayer))
            return true;

        if (Physics.Raycast(origin, back, out hit, wallCheckDistance, wallLayer))
            return true;        
        return false;
    }

    bool GroundedCheck()
    {
        if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer))
            return true;
        return false;
    }
    void HandleStateTransition(bool wallAhead, RaycastHit hit)
    {
        if (wallAhead && !isClimbing )
        {
            wallNormal = hit.normal;
            isClimbing = true;
        }
        else if (isClimbing && !wallAhead)
        {
            isClimbing = false;
            wasClimbing = true;
        }
        else if (isClimbing && wallAhead && !isGrounded)
        {
            wallNormal = hit.normal;
            wallRight = Vector3.Cross(wallNormal, Vector3.up).normalized;
            wallUp = Vector3.Cross(wallRight, wallNormal).normalized;
        }
    }

    void CalculateMovementAndRotation()
    {
    if (isClimbing)
    {
        // Movimento no plano da parede
        Vector3 inputOnWall = (wallRight * inputBuffer.x) + (wallUp * inputBuffer.y);
        moveDirection = Vector3.ClampMagnitude(inputOnWall, 1f) * climbSpeed;
 
        // Gravidade customizada: empurra contra a parede para grudar
        rb.useGravity = false;
        rb.AddForce(-wallNormal * gravity * 5, ForceMode.Acceleration);
 
        // --- ROTAÇÃO DA RAIZ (isso estava faltando) ---
        // Trava a raiz de frente pra parede SEMPRE, independente de input.
        // Isso é o que mantém o raycast de detecção (transform.forward) grudado
        // na parede enquanto você desliza pros lados/baixo.
        meshTargetRotation = Quaternion.LookRotation(-wallNormal, wallUp);
 
        // --- ROTAÇÃO DA MESH ---
        // Mesmo eixo "up" (wallNormal) em AMBOS os casos, com ou sem input.
        // Só o "forward" muda. Isso elimina o pulo/giro brusco ao soltar o input,
        // porque antes o caso parado usava um up diferente (wallUp) do caso
        // andando (wallNormal) — troca de eixo = rotação visualmente errada.
        Vector3 meshForwardDir = inputOnWall.sqrMagnitude > 0.001f
            ? inputOnWall.normalized
            : lastMeshForward; // parado: mantém a última direção, não reseta
 
        // Achata no plano da parede por segurança (evita drift se wallRight/wallUp
        // não estiverem perfeitamente ortogonais a wallNormal)
        meshForwardDir = Vector3.ProjectOnPlane(meshForwardDir, wallNormal).normalized;
        if (meshForwardDir.sqrMagnitude < 0.0001f) meshForwardDir = wallUp;
 
        lastMeshForward = meshForwardDir;
        meshTargetRotation = Quaternion.LookRotation(meshForwardDir, wallNormal);
 

    }
    else
    {
        // Movimento no chão relativo à câmera
        Vector3 camForward = UnityEngine.Camera.main.transform.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = UnityEngine.Camera.main.transform.right;
        camRight.y = 0;
        camRight.Normalize();
 
        Vector3 inputGround = (camRight * inputBuffer.x) + (camForward * inputBuffer.y);
        moveDirection = Vector3.ClampMagnitude(inputGround, 1f) * moveSpeed;
 
        rb.useGravity = true;
 
        if (inputGround.sqrMagnitude > 0.001f)
        {
            targetRotation = Quaternion.LookRotation(inputGround);
            meshTargetRotation = targetRotation;
            lastMeshForward = inputGround.normalized; // guarda pra quando voltar a escalar
        }
    }
    }
 
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {

            jumpButtonHeld = true;
            jumpChargeTime = 0f;
            requestedJumpForce = minJumpForce;
            jumpRequested = true;
        }

        if (jumpButtonHeld && Input.GetKey(KeyCode.Space))
        {
            jumpChargeTime = Mathf.Min(jumpChargeTime + Time.deltaTime, maxJumpChargeTime);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpButtonHeld = false;

        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = isClimbing ? Color.green : Color.red;

        Vector3 forward = transform.forward;
        Vector3 left = Quaternion.AngleAxis(leftRaycast, Vector3.up) * forward;
        Vector3 right = Quaternion.AngleAxis(rightRaycast, Vector3.up) * forward;
        Vector3 back = Quaternion.AngleAxis(0f, Vector3.up) * -forward;

        Gizmos.DrawRay(transform.position + Vector3.up * raycastDis, forward * wallCheckDistance);
        Gizmos.DrawRay(transform.position + Vector3.up * raycastDis, left * wallCheckDistance);
        Gizmos.DrawRay(transform.position + Vector3.up * raycastDis, right * wallCheckDistance);
        Gizmos.DrawRay(transform.position + Vector3.up * raycastDis, back * wallCheckDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + Vector3.up * raycastDis, Vector3.down * groundCheckDistance);
    }
}