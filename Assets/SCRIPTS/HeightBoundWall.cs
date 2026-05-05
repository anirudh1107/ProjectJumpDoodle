using UnityEngine;

public class HeightBoundWall : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float wallSideOffset; // e.g., -5 for left wall, 5 for right wall
    [Range(0, 1)] public float scrollResistance = 1f; // 1 = scrolls 1:1 with player

    private float startWallY;
    private float startPlayerY;

    void Start()
    {
        startWallY = transform.position.y;
        startPlayerY = player.position.y;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Keep the wall at a fixed horizontal distance from the player's start X or a fixed X
        // But scroll the Y based on player height
        float heightShift = (player.position.y - startPlayerY) * scrollResistance;
        
        transform.position = new Vector3(wallSideOffset, startWallY + heightShift, transform.position.z);
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        startPlayerY = player.position.y; // Reset starting Y when player is set
    }
}