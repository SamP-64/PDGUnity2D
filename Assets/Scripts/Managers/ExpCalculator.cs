using UnityEngine;

public static class ExpCalculator
{
   public static int CalculateXPFast(int level)
    {
        if (level < 1) level = 1; // minimum level constraint
                                  // Formula: XP = 4 * level^3 / 5
        return (4 * level * level * level) / 5;
    }
}
