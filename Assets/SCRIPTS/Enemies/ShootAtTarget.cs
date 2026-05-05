using UnityEngine;
using UnityEngine.Pool;

public class ShootAtTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float shootInterval = 2f;
    [SerializeField] private float projectileSpeed = 10f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating(nameof(Shoot), 1f, shootInterval);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Shoot()
    {
        if (target == null)
        {
            return;
        }
        Vector3 direction = (target.position - transform.position).normalized;
        // Instantiate and shoot projectile in the direction of the target
        AudioManager._instance.PlayShootingSound();
        GameObject projectile = EnemyAmmoBox.Instance.getAmmo(1)[0];
        projectile.transform.position = transform.position;
        projectile.SetActive(true);
        projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileSpeed; // Example speed of 10 units per second
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
