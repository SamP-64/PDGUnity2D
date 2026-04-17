using UnityEngine;

public static class DamageCalculator
{
    public static int CalculateDamage(
        int userLevel,
        int userAttack,
        int baseAttackPower,
        int targetDefence
    )
    {
        float baseDamage =
            (((2f * userLevel / 5f) + 2f) * userAttack * baseAttackPower / targetDefence) / 50f + 2f;

        return Mathf.Max(1, Mathf.FloorToInt(baseDamage));
    }
}