using UnityEngine;

public static class TurnManager 
{
    public static void NextTurn()
    {
        Enemy[] enemies = Object.FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            // enemy.RandomMove();

            enemy.MoveTowardsPlayer();
        }
    }
}
