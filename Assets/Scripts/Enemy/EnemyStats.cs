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
    public bool dead = false;
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
        if (currentHP <= 0)
        {
            dead = true;
            StartCoroutine(DieRoutine());
        }
    }

    IEnumerator DieRoutine()
    {
        yield return StartCoroutine(DieEffect());

        Dungeon.Grid[enemy.gridPos.x, enemy.gridPos.y].cellType = CellType.Floor;

        if (enemy.collectedItem != null)
        {
            
            Dungeon.Grid[enemy.gridPos.x, enemy.gridPos.y].cellType =
            enemy.collectedItem.GetComponent<Spawnable>().CellType;

            enemy.collectedItem.transform.position = transform.position;
            enemy.collectedItem.SetActive(true);
        }

        Destroy(gameObject);
        textLog.AddMessage("Player Defeated Enemy!");
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

    public IEnumerator DieEffect()
    {
        Transform t = transform;
        Renderer rend = GetComponentInChildren<Renderer>();

        Color original = rend.material.color;

        // flash white/red
        rend.material.color = Color.white;

        yield return new WaitForSeconds(0.05f);

        rend.material.color = Color.red;

        // shrink
        float duration = 0.7f;
        float time = 0f;

        Vector3 startScale = t.localScale;

        while (time < duration)
        {
            float tLerp = time / duration;
            t.localScale = Vector3.Lerp(startScale, Vector3.zero, tLerp);
            time += Time.deltaTime;
            yield return null;
        }

        t.localScale = Vector3.zero;
    }
}
