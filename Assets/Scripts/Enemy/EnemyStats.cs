using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    public int level;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defence;
    public bool dead = false;

    Enemy enemy;

    public void IntializeStats(int level, int hp, int atk, int def)
    {
       
        this.maxHP = hp;
        this.currentHP = hp;
        this.attack = atk;
        this.defence = def;
    }

    private void Start()
    {
        enemy = gameObject.GetComponent<Enemy>();
    }

    #region TakeDamage
 
    public IEnumerator ApplyDamage(int damage)
    {
        currentHP = currentHP - damage;

        GameManager.Instance.textLog.AddMessage("Enemy took " + damage + " damage!");
        TakeDamageEffect();

        if (currentHP <= 0 && !dead)
        {
            dead = true;
            yield return StartCoroutine(DieRoutine());
        }
    }

    #endregion
    #region Damage Effect
    IEnumerator DieRoutine()
    {

        Dungeon.Grid[enemy.gridPos.x, enemy.gridPos.y].cellType = CellType.Floor;
        yield return StartCoroutine(DieEffect());

        if (enemy.collectedItem != null)
        {
            
            Dungeon.Grid[enemy.gridPos.x, enemy.gridPos.y].cellType =
            enemy.collectedItem.GetComponent<Spawnable>().CellType;

            enemy.collectedItem.transform.position = transform.position;
            enemy.collectedItem.SetActive(true);
        }

        Destroy(gameObject);
        GameManager.Instance.textLog.AddMessage("Enemy Defeated!");
        int exp = ExpCalculator.CalculateXPFast(level);
        enemy.pc.playerStats.GainExp(exp);
    }

    Renderer rend;
    Color originalColour;
    Color hitColour = Color.red;
    float flashTime = 0.1f;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        originalColour = rend.material.color;

    }
    public void TakeDamageEffect()
    {
        StopAllCoroutines();
        StartCoroutine(Flash());
    }
    IEnumerator Flash()
    {
        rend.material.color = hitColour;
        yield return new WaitForSeconds(flashTime);
        rend.material.color = originalColour;
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

        yield return new WaitForSeconds(0.3f);
        t.localScale = Vector3.zero;
    }
    #endregion
}
