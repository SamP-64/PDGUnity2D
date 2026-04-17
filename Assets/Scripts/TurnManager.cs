using UnityEngine;
using System.Collections;

public class TurnManager : MonoBehaviour   
{

    [SerializeField] public MiniMap MiniMap;
    public static TurnManager Instance;
    private void Awake()
    {
        Instance = this;

    }

    public IEnumerator NextTurn(float waitTime)
    {

        yield return new WaitForSeconds(waitTime); 


        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (enemy.IsNextToPlayer())
            {
                enemy.Attack();
              
            }
            else
            {
                Debug.Log("movetoplayere");
                enemy.MoveTowardsPlayer();
            }

        }


    }


}
