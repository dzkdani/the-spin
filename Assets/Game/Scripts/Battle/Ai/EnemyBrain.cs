using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyBrain
{
    private readonly AiProfile profile;
    private readonly IRandom random;
    private readonly List<IAiBehavior> behaviors;

    private AiCommand command;
    private float decisionTimer;
    private float commitTimer;
    private string currentBehavior = "none";

    public string CurrentBehavior => currentBehavior;

    public EnemyBrain(AiProfile profile, IRandom random)
    {
        this.profile = profile;
        this.random = random;

        behaviors = new List<IAiBehavior>
        {
            new PressureBehavior(),
            new ExploitElementBehavior(),
            new BaitBehavior(),
            new StrikeBehavior(),
            new RetreatBehavior()
        };
    }

    public AiCommand Tick(in BattleSnapshot snapshot, float deltaTime)
    {
        commitTimer -= deltaTime;
        decisionTimer -= deltaTime;

        if (decisionTimer > 0f)
            return command;

        decisionTimer = profile.decisionInterval;

        if (commitTimer > 0f)
            return command;

        IAiBehavior best = null;
        float bestScore = 0f;

        for (int i = 0; i < behaviors.Count; i++)
        {
            float score = behaviors[i].Score(snapshot, profile);

            if (score > bestScore)
            {
                bestScore = score;
                best = behaviors[i];
            }
        }

        if (best == null)
        {
            currentBehavior = "none";
            command = new AiCommand(
                snapshot.OpponentPosition,
                false,
                -1);

            return command;
        }

        currentBehavior = best.Id;

        AiCommand decided = best.Decide(snapshot, profile);

        Vector2 jitter =
            random.InsideUnitCircle() * profile.jitterRadius;

        command = new AiCommand(
            decided.MoveTarget + jitter,
            decided.RequestDash,
            decided.AbilitySlot);

        commitTimer = profile.commitTime;

        return command;
    }
}
