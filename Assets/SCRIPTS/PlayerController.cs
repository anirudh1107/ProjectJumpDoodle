using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private Transform FreedomPoint;

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
        }
        else if (other.CompareTag("Breakable"))
        {
            Jump();
            other.gameObject.SetActive(false); // Deactivate the breakable platform
        }
    }

    private void Jump()
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            
    }

    
}
