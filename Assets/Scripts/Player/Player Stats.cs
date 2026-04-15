using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    int maxHP;
    int currentHP;
    int attack;
    int defence;

    PlayerController pc;

    public void Awake()
    {
        pc = gameObject.GetComponent<PlayerController>();
    }
    public void IntializeStats(int hp, int atk, int def)
    {
        this.maxHP = hp;
        this.currentHP = hp;
        this.attack = atk;
        this.defence = def;
    }

    public void Update()
    {

    }
}
