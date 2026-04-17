using UnityEngine;
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
    public void ApplyDamage(int damage)
    {
        currentHP = currentHP - damage;
        UpdateText();
    }
    public void RestoreHealth(int restoreValue)
    {
        currentHP = currentHP + restoreValue;

        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }

        UpdateText();
    }

}
