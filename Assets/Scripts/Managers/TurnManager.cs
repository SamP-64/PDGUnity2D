using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour   
{

    [SerializeField] public MiniMap MiniMap;
    public static TurnManager Instance;

    bool isProcessingTurn = false;

    private List<Enemy> enemies = new List<Enemy>();

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator NextTurn(float waitTime)
    {
        if (isProcessingTurn) { yield break; }
    

        isProcessingTurn = true;

        yield return new WaitForSeconds(waitTime);

        var currentEnemies = enemies.ToArray(); // set copy of enemy list as main list may be midified during the for each loop

        foreach (Enemy enemy in currentEnemies)
        {
            if (enemy.enemyStats.dead == true)
            {
                continue;
            }

            if (enemy.IsNextToPlayer())
            {
                yield return enemy.Attack();
            }
            else if (enemy.GetDistanceFromPlayer() < enemy.sightRange)
            {
                yield return enemy.MoveTowardsPlayer();
            }
            else if (enemy.collectedItem == null)
            {
                yield return enemy.MoveTowardsItem();
            }
            else
            {
                yield return enemy.RandomMove();
            }
        }

        isProcessingTurn = false;
    }

    public bool IsTurnRunning()
    {
        return isProcessingTurn;
    }

    public void StartTurn()
    {
        isProcessingTurn = true;
    }

    public void EndTurn()
    {
        isProcessingTurn = false;
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        enemies.Remove(enemy);
    }
    public void ClearEnemies()
    {
        enemies.Clear();
    }
}