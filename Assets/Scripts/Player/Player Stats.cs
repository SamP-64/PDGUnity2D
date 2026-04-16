using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    int level;
    int maxHP;
    int currentHP;
    int attack;
    int defence;

    PlayerController pc;

    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text hpText;
    public void Awake()
    {
        pc = gameObject.GetComponent<PlayerController>();
    }
    public void IntializeStats(int level, int hp, int atk, int def)
    {
        this.maxHP = hp;
        this.currentHP = hp;
        this.attack = atk;
        this.defence = def;

        levelText.text = "Lvl. " + level;
        hpText.text = "hp. " + currentHP + " / " + maxHP;
    }

    public void Update()
    {

    }
}
