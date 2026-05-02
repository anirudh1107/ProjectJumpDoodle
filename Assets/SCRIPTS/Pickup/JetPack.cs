using UnityEngine;

public class JetPack : MonoBehaviour, IPickupable
{
    public void OnPickedUp()
    {
        Debug.Log("JetPack picked up!");
        gameObject.SetActive(false);

        //trigger vfx and sfx
    }
}
