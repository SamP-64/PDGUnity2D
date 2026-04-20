using UnityEngine;

using System.Collections;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public int level;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defence;

    PlayerController pc;

    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text hpText;

    public void IntializeStats(int level, int hp, int atk, int def)
    {
        this.level = level;
        this.maxHP = hp;
        this.currentHP = hp;
        this.attack = atk;
        this.defence = def;

        UpdateText();  
    }

    private void UpdateText()
    {
        levelText.text = "Lvl. " + level;
        hpText.text = "hp. " + currentHP + " / " + maxHP;
    }

    public void ApplyDamage(int damage) // Method to decrease player health
    {

        currentHP = currentHP - damage;
        UpdateText();
        TakeDamageEffect();
    }

    public void GainExp(int exp)
    {
        Debug.Log(exp);
    }
    public void RestoreHealth(int restoreValue) // Method to increase player health
    {
        currentHP = currentHP + restoreValue;

        if (currentHP > maxHP) // stop hp going over max
        {
            currentHP = maxHP;
        }

        UpdateText();
    }

    Renderer rend;
    Color originalColor;
    public Color hitColor = Color.red;
    public float flashTime = 0.1f;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        originalColor = rend.material.color;

    }
    public void TakeDamageEffect()
    {
        StopAllCoroutines();
        StartCoroutine(Flash());
    }
    IEnumerator Flash()
    {
        rend.material.color = hitColor;
        yield return new WaitForSeconds(flashTime);
        rend.material.color = originalColor;
    }
}
