using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class Player : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    [SerializeField] private InputActionReference inputActionReference;

    [SerializeField] private int speed;

    private void Start()
    {
        animator.SetBool("RunningSlow", true);
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        inputActionReference.action.performed += OnJump;
        inputActionReference.action.Enable();
    }

    void OnDisable()
    {
        inputActionReference.action.performed -= OnJump;
        inputActionReference.action.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.forward * speed, ForceMode.Force);
    }

    private void Move()
    {
        if (rb.velocity.z >=  12)
        {
            animator.SetBool("RunningFast", true);
            animator.SetBool("RunningSlow", false);
        }
    }

    private void OnJump(InputAction.CallbackContext context) 
    {
        inputActionReference.action.Disable();
        animator.SetTrigger("Jumping");
        rb.AddForce(Vector3.up * 80, ForceMode.Impulse);
    }
}
