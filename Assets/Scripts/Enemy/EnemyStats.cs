using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int level;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defence;


    public void IntializeStats(int level, int hp, int atk, int def)
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
