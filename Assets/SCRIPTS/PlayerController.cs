using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private Transform FreedomPoint;

    [Header("Boost Settings")]
    public float boostVerticalVelocity = 20f;
    public float boostHorizontalMultiplier = 1.5f;
    private bool isBoosting = false;
    private Health health;

    private bool isMoving = false;
    private Vector2 moveDirection = Vector2.zero;
    private Rigidbody2D rb;

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase.CompareTo(InputActionPhase.Started) == 0)
        {
            isMoving = true;
            moveDirection = context.ReadValue<Vector2>();
        }
        else if (context.phase.CompareTo(InputActionPhase.Performed) == 0)
        {
            moveDirection = context.ReadValue<Vector2>();
        }
        else if (context.phase.CompareTo(InputActionPhase.Canceled) == 0)
        {
            isMoving = false;
        }

    }

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        HandleMovement();
       
        
        if(rb.linearVelocity.y < 0)
        {
            rb.gravityScale = 2f; // Increase gravity when falling
        }
        else
        {
            rb.gravityScale = 1f; // Normal gravity when rising or grounded
        }


        if (transform.position.y < FreedomPoint.position.y)
        {
            // Death is the only freedom!!!
            OnDeath();
        }
    }

    private void HandleMovement()
    {
        if (isMoving)
        {
            transform.Translate(new Vector3(moveDirection.x, 0, 0) * moveSpeed * Time.deltaTime);
        }

        if (transform.position.x < leftPoint.position.x)
        {
            transform.position = new Vector3(leftPoint.position.x, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > rightPoint.position.x)
        {
            transform.position = new Vector3(rightPoint.position.x, transform.position.y, transform.position.z);
        }
    }

    private void OnDeath()
    {
        // Handle player death logic here
        Debug.Log("Player has died!");
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("SpringFloor"))
        {
            Jump();
        }
        else if (other.CompareTag("Enemy"))
        {
            Debug.Log("Player hit an enemy!");
            // Handle player damage or game over logic here
            TakeDamage(20f);
        }
        else if (other.CompareTag("Breakable"))
        {
            Jump();
            other.gameObject.SetActive(false); // Deactivate the breakable platform
        }
        else if (other.CompareTag("Projectile"))
        {
            Debug.Log("Player hit by a projectile!");
            // Handle player damage or game over logic here
            TakeDamage(10f);
        }
        else if (other.CompareTag("Pickup"))
        {
            Debug.Log("Player picked up a JetPack!");

            if(other.GetComponent<JetPack>() != null)
            {
                other.GetComponent<JetPack>().OnPickedUp();
                StartCoroutine(BoostRoutine(5f)); // Example: Boost for 5 seconds
            }
        }
    }

    private void TakeDamage(float damage)
    {
        health.TakeDamage(damage);
    }

    private void Jump()
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            
    }

    private IEnumerator BoostRoutine(float duration)
    {
        isBoosting = true;
        
        // Store original speed to reset it later
        float originalHorizontalSpeed = moveSpeed;
        
        // Apply the buff
        moveSpeed *= boostHorizontalMultiplier;

        float elapsed = 0;
        while (elapsed < duration)
        {
            // Force constant upward velocity every frame
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, boostVerticalVelocity);
            
            elapsed += Time.deltaTime;
            yield return null; // Wait for next frame
        }

        // Reset stats
        moveSpeed = originalHorizontalSpeed;
        isBoosting = false;
    }



    
}
