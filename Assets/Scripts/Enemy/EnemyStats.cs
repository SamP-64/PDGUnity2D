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

    #region TakeDamage
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
    #endregion 
    #region Damage Effect
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
        int exp = ExpCalculator.CalculateXPFast(level);
        enemy.pc.playerStats.GainExp(exp);
    }


    Renderer renderer;
    Color originalColour;
    Color hitColour = Color.red;
    float flashTime = 0.1f;

    void Awake()
    {
        renderer = GetComponentInChildren<Renderer>();
        originalColour = renderer.material.color;

    }
    public void TakeDamageEffect()
    {
        StopAllCoroutines();
        StartCoroutine(Flash());
    }
    IEnumerator Flash()
    {
        renderer.material.color = hitColour;
        yield return new WaitForSeconds(flashTime);
        renderer.material.color = originalColour;
    }

    public IEnumerator DieEffect()
    {
        Transform t = transform;
        Renderer rend = GetComponentInChildren<Renderer>();

        Color original = rend.material.color;

        rend.material.color = Color.white;

        yield return new WaitForSeconds(0.05f);

        rend.material.color = Color.red; // Flash Sprite Red

        float duration = 0.7f;
        float time = 0f;

        Vector3 startScale = t.localScale;

        while (time < duration) // Shrink for duration
        {
            float tLerp = time / duration;
            t.localScale = Vector3.Lerp(startScale, Vector3.zero, tLerp);  // shrink enemy
            time += Time.deltaTime;
            yield return null;
        }

        t.localScale = Vector3.zero;
    }
    #endregion
}
