using UnityEngine;

public class HealthPickup : MonoBehaviour, IPickupable
{
    public void OnPickedUp()
    {
        Debug.Log("Health pickup collected!");
        gameObject.SetActive(false);

        //trigger vfx and sfx
    }
}
