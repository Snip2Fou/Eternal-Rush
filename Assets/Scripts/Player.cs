using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    [SerializeField] private Animator animator;

    [SerializeField] private InputActionReference inputActionReference;

    // Start is called before the first frame update
    void Start()
    {
        inputActionReference.action.Enable();
        inputActionReference.action.performed += OnJump;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnJump(InputAction.CallbackContext context) 
    {
        animator.SetTrigger("Jumping");
    }
}
