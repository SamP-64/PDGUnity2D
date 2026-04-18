using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour   
{

    [SerializeField] public MiniMap MiniMap;
    public static TurnManager Instance;

    bool isProcessingTurn = false;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator NextTurn(float waitTime)
    {
        if (isProcessingTurn) { yield break; }

        isProcessingTurn = true;

        yield return new WaitForSeconds(waitTime); 

        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (enemy.IsNextToPlayer())
            {
                enemy.Attack();
            }
            else if (enemy.GetDistanceFromPlayer() < enemy.sightRange)
            {
                enemy.MoveTowardsPlayer();
            }
            else if (enemy.collectedItem == null)
            {
                enemy.MoveTowardsItem();
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
}