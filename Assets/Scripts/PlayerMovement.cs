using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float rotationSmoothTime = 0.1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("Dodgeroll Settings")]
    public float rollSpeed = 8f;
    public float rollCooldown = 1f;
    public AnimationCurve rollSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private CharacterController controller;
    private Animator animator;
    private Transform cam;

    private Vector3 velocity;                // nur Y für Grav
    private float turnSmoothVelocity;

    private bool isRolling = false;
    private float lastRollTime = -999f;
    private Vector3 rollDir = Vector3.zero;
    private float rollT = 0f;
    private int rollHash;

    private readonly float gravity = -9.81f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cam = Camera.main.transform;

        animator.SetFloat("InputY", 0f);
        animator.SetFloat("InputMagnitude", 0f);
        animator.SetBool("isSprinting", false);

        rollHash = Animator.StringToHash("Roll");
    }

    void Update()
    {
        // --- 1) Grounding ---
        bool grounded = Physics.CheckSphere(
            groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore);

        // Wenn geerdet und nach unten fiel, Y-Velocity neutralisieren (kein zusätzlicher Down-Push)
        if (grounded && velocity.y < 0f)
            velocity.y = 0f;

        // --- 2) Input ---
        float ix = Input.GetAxisRaw("Horizontal");
        float iz = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(ix, 0f, iz).normalized;
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift);

        Vector3 moveDirWorld = Vector3.zero;
        if (!isRolling && inputDir.sqrMagnitude > 0.001f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            moveDirWorld = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        // --- 3) Rolle starten ---
        if (!isRolling && Time.time > lastRollTime + rollCooldown)
        {
            if (Input.GetKeyDown(KeyCode.Space) && inputDir.sqrMagnitude > 0.001f)
            {
                float rollAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                rollDir = (Quaternion.Euler(0f, rollAngle, 0f) * Vector3.forward).normalized;

                animator.ResetTrigger("RollTrigger");
                animator.SetTrigger("RollTrigger");

                isRolling = true;
                rollT = 0f;
                lastRollTime = Time.time;

                // KEIN velocity.y-Down-Push mehr – du regelst die Höhe manuell
            }
        }

        // --- 4) Bewegung (eine Move() später) ---
        Vector3 horizontal = Vector3.zero;

        if (isRolling)
        {
            AnimatorStateInfo st = animator.GetCurrentAnimatorStateInfo(0);
            if (st.shortNameHash == rollHash)
                rollT = Mathf.Clamp01(st.normalizedTime);

            float rollMult = rollSpeedCurve.Evaluate(rollT);
            horizontal = rollDir * (rollSpeed * rollMult);

            animator.SetFloat("InputY", 0f);
            animator.SetFloat("InputMagnitude", 0f);
            animator.SetBool("isSprinting", false);

            if (st.shortNameHash != rollHash || st.normalizedTime >= 1f)
                isRolling = false;
        }
        else
        {
            float speed = wantsSprint ? sprintSpeed : walkSpeed;
            horizontal = moveDirWorld * speed;

            float mag = Mathf.Clamp01(inputDir.magnitude);
            animator.SetFloat("InputY", mag);
            animator.SetFloat("InputMagnitude", mag);
            animator.SetBool("isSprinting", wantsSprint);
        }

        // --- 5) Gravity ---
        velocity.y += gravity * Time.deltaTime;

        // --- 6) Eine einzige Move()-Ausführung ---
        Vector3 motion = (horizontal * Time.deltaTime) + (Vector3.up * velocity.y * Time.deltaTime);
        controller.Move(motion);
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
