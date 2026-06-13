using UnityEngine;
using Photon.Pun;

public class GladiatorHealth : MonoBehaviourPun
{
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Gladiator took damage. HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Gladiator died.");
        // Later: play death animation / game over screen
    }
}