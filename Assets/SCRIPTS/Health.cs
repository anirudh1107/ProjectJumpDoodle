using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float defaultDamage = 1f;
    private float maxHealth = 3f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        MainUIManager.Instance.UpdateHealthDisplay(currentHealth);
    }


    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        MainUIManager.Instance.UpdateHealthDisplay(currentHealth);
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    public void TakeDamage()
    {
        currentHealth -= defaultDamage;
        MainUIManager.Instance.UpdateHealthDisplay(currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle death logic here
        Debug.Log("Entity has died!");
        Destroy(gameObject);
    }
}
