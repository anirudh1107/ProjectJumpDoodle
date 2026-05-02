using UnityEngine;

public class MoveAcrossScreen : MonoBehaviour
{
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private float moveSpeed = 5f;

    private int moveDir = 1; // 1 for right, -1 for left

    void Update()
    {
        if (leftPoint == null || rightPoint == null)
        {
            Debug.LogWarning("Left and Right points not set for MoveAcrossScreen script on " + gameObject.name);
            return;
        }
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

    public void SetLeftAndRightPoints(Transform left, Transform right)
    {
        leftPoint = left;
        rightPoint = right;
    }
}
