using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JumpAnimationEvent : MonoBehaviour
{
    [SerializeField] private InputActionReference jumpActionReference;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Animator animator;

    public void OnJumpAnimationEnd()
    {
        jumpActionReference.action.Enable();
        /*if (Physics.Raycast(transform.position, Vector3.down, 1f, groundLayer))
        {
            jumpActionReference.action.Enable();
        }
        else
        {
            Debug.Log("Jump2"); 
        }*/
    }
}
