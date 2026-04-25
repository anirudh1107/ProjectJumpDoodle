using UnityEngine;

public class MoveAcrossScreen : MonoBehaviour
{
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private float moveSpeed = 5f;

    private int moveDir = 1; // 1 for right, -1 for left

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * moveDir * moveSpeed * Time.deltaTime);
        if (transform.position.x < leftPoint.position.x)
        {
            moveDir = 1; // Move right
        }
        else if (transform.position.x > rightPoint.position.x)
        {
            moveDir = -1; // Move left
        }
    }
}
