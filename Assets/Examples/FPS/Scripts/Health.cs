using Netick;
using Netick.Unity;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [Networked] public float CurrentHealth { get; set; }
    public float maxHealth = 100f;
        
    public override void NetworkStart()
    {
        CurrentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
            
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
        
    private void Die()
    {
        Sandbox.Kick(InputSource);
        Sandbox.Destroy(Object);
  
    }
}
