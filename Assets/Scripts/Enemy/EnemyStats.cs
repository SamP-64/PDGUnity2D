using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int level;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defence;

    Enemy enemy;
    TextLog textLog;
    public void IntializeStats(int level, int hp, int atk, int def)
    {
       
        this.maxHP = hp;
        this.currentHP = hp;
        this.attack = atk;
        this.defence = def;
  
    }
    private void Start()
    {
        textLog = FindObjectOfType<TextLog>();
        enemy = gameObject.GetComponent<Enemy>();
    }
    public void ApplyDamage(int damage)
    {
        currentHP = currentHP - damage;
        textLog.AddMessage("Enemy took " + damage + " damage!");
        if (currentHP < 0)
        {
            Die();
        }
    }

    void Die()
    {
      textLog.AddMessage("Player Defeated Enemy!");
        Dungeon.Grid[enemy.gridPos.x, enemy.gridPos.y].cellType = CellType.Floor;
        Destroy(this.gameObject);
    }

}
