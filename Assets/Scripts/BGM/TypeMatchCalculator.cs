public static class TypeMatchCalculator
{
    public const float MatchMultiplier = 1.5f;
    public const float NeutralMultiplier = 1f;
    public const float MismatchMultiplier = 0.75f;

    public static float Calculate(AttackType attackType, BGMType bgmType)
    {
        if (attackType == AttackType.Neutral || bgmType == BGMType.None)
            return NeutralMultiplier;

        if ((int)attackType == (int)bgmType)
            return MatchMultiplier;

        return MismatchMultiplier;
    }
}
