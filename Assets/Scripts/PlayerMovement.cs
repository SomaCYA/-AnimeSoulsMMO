using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 120f; // Grad pro Sekunde

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Animator animator;

    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

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
        float inputX = Input.GetAxis("Horizontal"); // A/D = drehen
        float inputY = Input.GetAxis("Vertical");   // W/S = laufen

        // --- 3. Sprint ---
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // --- 4. Bewegung ---
        Vector3 move = transform.forward * Mathf.Clamp(inputY, -1f, 1f);
        controller.Move(move * currentSpeed * Time.deltaTime);

        // --- 5. Rotation mit A/D ---
        if (Mathf.Abs(inputX) > 0.1f)
        {
            transform.Rotate(Vector3.up, inputX * rotationSpeed * Time.deltaTime);
        }

        // --- 6. Animation ---
        float inputMagnitude = Mathf.Abs(inputY);
        animator.SetFloat("InputY", inputY);
        animator.SetFloat("InputMagnitude", inputMagnitude);
        animator.SetBool("isSprinting", isSprinting);

        // --- 7. Gravity ---
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
