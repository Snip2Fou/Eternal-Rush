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
    }
}
