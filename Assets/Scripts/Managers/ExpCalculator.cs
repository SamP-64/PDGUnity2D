using UnityEngine;

public static class ExpCalculator
{
   public static int CalculateXPFast(int level)
    {
        return (4 * level * level * level) / 5;
    }
}
