using UnityEngine;

public class JetPack : MonoBehaviour, IPickupable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
 
    public void OnPickedUp()
    {
        Debug.Log("JetPack picked up!");
        gameObject.SetActive(false);

        //trigger vfx and sfx
    }
}
