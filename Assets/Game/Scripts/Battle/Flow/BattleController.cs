using System.Collections.Generic;

public sealed class BattleController
{
    private readonly List<IWinCondition> conditions =
        new List<IWinCondition>();

    public bool Finished { get; private set; }

    public GameResult? Result { get; private set; }

    public void AddCondition(IWinCondition condition)
    {
        if (condition != null)
            conditions.Add(condition);
    }

    public GameResult? Evaluate(in BattleContext context)
    {
        if (Finished)
            return Result;

        for (int i = 0; i < conditions.Count; i++)
        {
            if (!conditions[i].IsSatisfied(context))
                continue;

            Finished = true;
            Result = conditions[i].Result;

            return Result;
        }

        return null;
    }
}
