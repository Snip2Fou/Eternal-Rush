using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting.APIUpdating;

public class Player : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    [SerializeField] private InputActionReference jumpActionReference;
    [SerializeField] private InputActionReference moveActionReference;

    [SerializeField] private float speed;
    [SerializeField] private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;
    private Vector3 playerVelocity;

    private bool isGrounded;
    private bool isJumping;

    private void Start()
    {
        animator.SetBool("RunningSlow", true);
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        jumpActionReference.action.performed += OnJump;
        jumpActionReference.action.Enable();

        moveActionReference.action.Enable();
    }

    void OnDisable()
    {
        jumpActionReference.action.performed -= OnJump;
        jumpActionReference.action.Disable();

        moveActionReference.action.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {
        Vector3 movement = new Vector3(moveActionReference.action.ReadValue<float>() * speed, 0, speed);
        rb.AddForce(movement * speed * Time.deltaTime, ForceMode.Force);
        if (rb.velocity.z >=  12)
        {
            animator.SetBool("RunningFast", true);
            animator.SetBool("RunningSlow", false);
        }
    }

    private void OnJump(InputAction.CallbackContext context) 
    {
        jumpActionReference.action.Disable();
        animator.SetTrigger("Jumping");
        rb.AddForce(Vector3.up * 300, ForceMode.Impulse);
    }
}
