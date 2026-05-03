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
    [SerializeField]private float boostVerticalVelocity = 20f;
    [SerializeField] private float boostHorizontalMultiplier = 1.5f;
    [SerializeField] private float boostDuration = 5f;

    private bool isBoosting = false;
    private Health health;

    private bool isMoving = false;
    private Vector2 moveDirection = Vector2.zero;
    private Rigidbody2D rb;
    private Animator playerAnimator;

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
        playerAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
        if(rb.linearVelocity.y < 0)
        {
            rb.gravityScale = 2f; 
            playerAnimator.SetBool("Landed", true);
        }
        else
        {
            rb.gravityScale = 1f; 
            playerAnimator.SetBool("Landed", false);
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
        LevelManager.Instance.DestroyPlayer();
        LevelManager.Instance.DestroyAllActiveObjects();
        GameManager.Instance.ChangeState(GameState.GameOver);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("SpringFloor"))
        {
            PlayJumpSound();
            playerAnimator.SetBool("Falling", false);
            Jump(other);
        }
        else if (other.CompareTag("Enemy"))
        {
            Debug.Log("Player hit an enemy!");
            TakeDamage(20f);
        }
        else if (other.CompareTag("Breakable"))
        {
            PlayJumpSound();
            playerAnimator.SetBool("Falling", false);
            Jump(other);
            other.gameObject.SetActive(false); // Deactivate the breakable platform
        }
        else if (other.CompareTag("Projectile"))
        {
            Debug.Log("Player hit by a projectile!");
            TakeDamage(10f);
        }
        else if (other.CompareTag("Pickup"))
        {
            Debug.Log("Player picked up a JetPack!");

            if(other.GetComponent<JetPack>() != null)
            {
                other.GetComponent<JetPack>().OnPickedUp();
                if((!isBoosting))
                    StartCoroutine(BoostRoutine(boostDuration));
            }
        }
    }

    private void PlayJumpSound() {
        if(AudioManager._instance != null) {
            AudioManager._instance.PlayJumpSound();
        }
    }

    private void TakeDamage(float damage)
    {
        if ((health.GetCurrentHealth() - damage) <= 0)
        {
            OnDeath();
        }
        health.TakeDamage(damage);
    }

    private void Jump(Collider2D other)
    {
        if (rb.linearVelocity.y <= 0.1f)
        {
            // This prevents "side-triggering"
            if (other.transform.position.y < transform.position.y)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            }
        }
        
            
    }

    private IEnumerator BoostRoutine(float duration)
    {
        isBoosting = true;
        float originalHorizontalSpeed = moveSpeed;
        moveSpeed *= boostHorizontalMultiplier;

        float elapsed = 0;
        while (elapsed < duration)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, boostVerticalVelocity);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        moveSpeed = originalHorizontalSpeed;
        isBoosting = false;
    }

    public void SetBoundaries(Transform leftBoundary, Transform rightBoundary, Transform freedomPoint)
    {
        leftPoint = leftBoundary;
        rightPoint = rightBoundary;
        FreedomPoint = freedomPoint;
    }
    
}
