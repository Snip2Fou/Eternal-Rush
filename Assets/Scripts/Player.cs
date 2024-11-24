using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Video;

public class Player : MonoBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    [SerializeField] private InputActionReference jumpActionReference;
    [SerializeField] private InputActionReference moveActionReference;

    [SerializeField] private float speed;

    [SerializeField] private ProceduralGeneration proceduralGeneration;
    [SerializeField] private GameObject scoreObj;
    [SerializeField] private GameObject resultObj;
    [SerializeField] private TextMeshProUGUI scoreResultText;

    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip hitClip;
    private AudioSource audioSource;

    private void Start()
    {
        animator.SetBool("RunningSlow", true);
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = runClip;
        audioSource.loop = true;
        audioSource.Play();   
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

    private void FixedUpdate()
    {
        speed += Time.deltaTime * 10;
    }

    private void Move()
    {
        Vector3 velocity = new Vector3(moveActionReference.action.ReadValue<float>() * Time.deltaTime * 1000, Mathf.Clamp(rb.velocity.y, rb.velocity.y -1, 5), speed * Time.deltaTime);
        if (transform.position.x > 31.5 && velocity.x > 0)
        {
            transform.SetPositionAndRotation(new Vector3(31.5f, transform.position.y, transform.position.z), transform.rotation);
            velocity.x = 0; 
        }
        else if (transform.position.x < 18.5 && velocity.x < 0)
        {
            transform.SetPositionAndRotation(new Vector3(18.5f, transform.position.y, transform.position.z), transform.rotation);
            velocity.x = 0;
        }
        rb.velocity = velocity; 
        if (rb.velocity.z >=  30)
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
        audioSource.Stop();
    }

    private void Lose()
    {
        Score score_script = scoreObj.GetComponent<Score>();
        scoreResultText.text = score_script.score.ToString();
        score_script.CheckNewBestScore();
        speed = 750;
        resultObj.SetActive(true);
        scoreObj.SetActive(false);
        animator.SetBool("RunningFast", false);
        animator.SetBool("RunningSlow", false);
        animator.SetTrigger("Idle");
        rb.velocity = Vector3.zero;
        transform.SetPositionAndRotation(new Vector3(25, 0.7f, 5), Quaternion.identity);
        proceduralGeneration.ResetMap();
        enabled = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Vector3 contactNormal = collision.GetContact(0).normal;
            Vector3 playerForward = transform.forward;

            float impactAngle = Vector3.Dot(playerForward, -contactNormal);

            if (impactAngle > 0.75f)
            {
                audioSource.clip = hitClip;
                audioSource.loop = false;
                audioSource.Play();
                Lose();
            }
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            if(audioSource != null)
            {
                audioSource.Play();
            }
        }
    }
}
