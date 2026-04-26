using UnityEngine;

public static class ExpCalculator
{
    public static int CalculateEXP(int enemyLevel, int playerLevel, int baseExp)
    {
        // float levelFactor = Mathf.Pow((float)enemyLevel / playerLevel, 1.2f);

        float exp = baseExp * enemyLevel; //* levelFactor;

        return Mathf.RoundToInt(exp);
    }
}
