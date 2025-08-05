using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;

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

        animator.SetFloat("InputX", 0f);
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

        // --- 3. Sprint-Check ---
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        // --- 4. Bewegung berechnen ---
        Vector3 move = transform.right * inputX + transform.forward * inputY;
        float inputMagnitude = new Vector2(inputX, inputY).magnitude;
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // --- 5. Animator füttern ---
        animator.SetFloat("InputX", inputX);
        animator.SetFloat("InputY", inputY);
        animator.SetFloat("InputMagnitude", inputMagnitude);
        animator.SetBool("isSprinting", isSprinting);

        // --- 6. Gravity anwenden ---
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
