using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float rotationSmoothTime = 0.1f;

    [Header("Dodgeroll Settings")]
    public float rollSpeed = 8f;
    public float rollCooldown = 1f;
    public AnimationCurve rollSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Visuals")]
    public Transform modelRoot;                 // Child mit Animator (z.B. "character")
    public bool autoApplySkinWidthOffset = true;

    private CharacterController controller;
    private Animator animator;
    private Transform cam;

    private Vector3 velocity;                   // nur Y
    private float turnSmoothVelocity;
    private bool isRolling;
    private float lastRollTime = -999f;
    private Vector3 rollDir;
    private float rollT;
    private int rollHash;

    private readonly float gravity = -9.81f;

    void OnValidate()
    {
        controller = GetComponent<CharacterController>();
        if (controller)
            controller.center = new Vector3(0f, controller.height * 0.5f, 0f);
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam        = Camera.main ? Camera.main.transform : null;

        // Center = Height/2 sicherstellen
        controller.center = new Vector3(0f, controller.height * 0.5f, 0f);

        // Animator vom Child (modelRoot) holen
        if (!modelRoot && transform.childCount > 0)
            modelRoot = transform.GetChild(0); // Fallback

        if (modelRoot)
        {
            animator = modelRoot.GetComponentInChildren<Animator>(true);
            if (animator) animator.applyRootMotion = false; // Root Motion aus

            // Sichtbares Mesh optisch auf Boden ziehen (kompensiert SkinWidth-Spalt)
            if (autoApplySkinWidthOffset)
            {
                var lp = modelRoot.localPosition;
                lp.y = -controller.skinWidth;     // i.d.R. -0.08
                modelRoot.localPosition = lp;
            }
        }

        // Start: exakt auf Boden snappen (physisch)
        StartSnapToGround();

        if (animator)
        {
            animator.SetFloat("InputY", 0f);
            animator.SetFloat("InputMagnitude", 0f);
            animator.SetBool("isSprinting", false);
            rollHash = Animator.StringToHash("Roll"); // Animator-State-Name "Roll"
        }
        else
        {
            Debug.LogWarning("[PlayerMovement] Kein Animator gefunden. Weisen Sie 'modelRoot' zu.");
        }
    }

    void Update()
    {
        bool grounded = controller.isGrounded;

        // Input
        float ix = Input.GetAxisRaw("Horizontal");
        float iz = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(ix, 0f, iz).normalized;
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift);

        Vector3 moveDirWorld = Vector3.zero;
        if (!isRolling && inputDir.sqrMagnitude > 0.001f && cam)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            moveDirWorld = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        // Rolle starten
        if (!isRolling && Time.time > lastRollTime + rollCooldown && animator)
        {
            if (Input.GetKeyDown(KeyCode.Space) && inputDir.sqrMagnitude > 0.001f)
            {
                float rollAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + (cam ? cam.eulerAngles.y : 0f);
                rollDir = (Quaternion.Euler(0f, rollAngle, 0f) * Vector3.forward).normalized;

                // In Rollrichtung ausrichten
                transform.rotation = Quaternion.LookRotation(new Vector3(rollDir.x, 0f, rollDir.z));

                animator.ResetTrigger("RollTrigger");
                animator.SetTrigger("RollTrigger");

                isRolling = true;
                rollT = 0f;
                lastRollTime = Time.time;
            }
        }

        // Horizontalbewegung
        Vector3 horizontal = Vector3.zero;
        if (isRolling && animator)
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);
            if (st.shortNameHash == rollHash) rollT = Mathf.Clamp01(st.normalizedTime);

            float rollMult = rollSpeedCurve.Evaluate(rollT);
            horizontal = rollDir * (rollSpeed * rollMult);

            // Am Boden „kleben“
            velocity.y = -2f;

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
            if (animator)
            {
                animator.SetFloat("InputY", mag);
                animator.SetFloat("InputMagnitude", mag);
                animator.SetBool("isSprinting", wantsSprint);
            }
        }

        // Gravitation / Boden-Stick
        if (grounded)
        {
            if (velocity.y < -2f) velocity.y = -2f; // kleiner Down-Bias gegen optischen Spalt
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Move
        Vector3 motion = (horizontal * Time.deltaTime) + Vector3.up * (velocity.y * Time.deltaTime);
        controller.Move(motion);
    }

    private void StartSnapToGround()
    {
        float r = Mathf.Max(0.01f, controller.radius - 0.02f);
        Vector3 origin = transform.position + controller.center + Vector3.up * 0.05f;

        if (Physics.SphereCast(origin, r, Vector3.down, out RaycastHit hit, 5f, ~0, QueryTriggerInteraction.Ignore))
        {
            float bottom = transform.position.y + controller.center.y - controller.height * 0.5f;
            float gap = (bottom - hit.point.y);
            if (gap > 0.001f)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - gap, transform.position.z);
            }
        }
    }
}
