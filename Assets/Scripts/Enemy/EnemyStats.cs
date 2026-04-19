using UnityEngine;

using System.Collections;
using static UnityEditor.Progress;
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
        TakeDamageEffect();
        if (currentHP < 0)
        {
            Die();
        }
    }

    void Die()
    {
        Dungeon.Grid[enemy.gridPos.x, enemy.gridPos.y].cellType = CellType.Floor;

        if (enemy.collectedItem != null)
        {
            Debug.Log(enemy.collectedItem.name);
            textLog.AddMessage("Player Defeated Enemy!");
            Dungeon.Grid[enemy.gridPos.x, enemy.gridPos.y].cellType = enemy.collectedItem.GetComponent<Spawnable>().CellType;
            enemy.collectedItem.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0f);
            enemy.collectedItem.gameObject.SetActive(true);
        }

        Destroy(this.gameObject);
        
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
