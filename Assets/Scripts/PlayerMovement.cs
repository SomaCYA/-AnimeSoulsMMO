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
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Animator animator;
    private Transform cameraTransform;

    private Vector3 velocity;
    private bool isGrounded;

    private float currentVelocity;
    private float turnSmoothVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cameraTransform = Camera.main.transform;

        animator.SetFloat("InputY", 0f);
        animator.SetFloat("InputMagnitude", 0f);
        animator.SetBool("isSprinting", false);
    }

    void Update()
    {
        // --- 1. Boden prüfen ---
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- 2. Eingaben lesen ---
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(inputX, 0f, inputY).normalized;

        // --- 3. Bewegung berechnen ---
        if (inputDir.magnitude >= 0.1f)
        {
            // Kamera-relative Richtung
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);

            // Animation
            animator.SetFloat("InputY", 1f); // Vorwärts
            animator.SetFloat("InputMagnitude", 1f);
            animator.SetBool("isSprinting", isSprinting);
        }
        else
        {
            // Keine Bewegung
            animator.SetFloat("InputY", 0f);
            animator.SetFloat("InputMagnitude", 0f);
            animator.SetBool("isSprinting", false);
        }

        // --- 4. Gravity ---
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}
