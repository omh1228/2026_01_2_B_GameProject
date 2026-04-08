using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterStats : MonoBehaviour
{

    public string characterName;
    public int maxHealth = 100;
    public int currentHealth;

    public Slider healthBar;
    public TextMeshProUGUI healthText;


    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
    }    
}
