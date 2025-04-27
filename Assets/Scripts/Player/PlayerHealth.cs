using benjohnson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth;
    int currentHealth;

    // Components
    [SerializeField] Counter cCounter;
    [SerializeField] Counter mCounter;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateCurrent();
        UpdateMax();
    }

    public void Damage(int value)
    {
        PlayerStats.instance.playerStatsDic["damageTaken"]++;
        ArtifactManager.instance.TriggerTakeDamage();
        SoundManager.instance.PlaySound("Player Hurt");
        DamagePopup.CreatePopup(transform.position, value);
        currentHealth -= value;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Kill();
        }
        UpdateCurrent();
    }

    public void IncreaseHealth(int increase)
    {
        maxHealth += increase;
        if (increase > 0)
            currentHealth += increase;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        if (maxHealth < 0)
        {
            maxHealth = 0;
            Kill();
        }
        UpdateCurrent();
        UpdateMax();
    }

    public void Heal(int value)
    {
        PlayerStats.instance.playerStatsDic["healthHealed"] += value;
        SoundManager.instance.PlaySound("Heal");
        DamagePopup.CreatePopup(transform.position, value, true);
        currentHealth += value;
        if (currentHealth < 0)
            currentHealth = 0;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        UpdateCurrent();
    }

    void UpdateCurrent()
    {
        if (cCounter == null) return;
        cCounter.SetText(currentHealth.ToString(), 0);
    }

    void UpdateMax()
    {
        if (mCounter == null) return;
        mCounter.SetText(maxHealth.ToString(), 0);
    }

    public void Kill()
    {
        GameManager.instance.PlayerKilled();
    }
}
