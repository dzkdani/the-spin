public interface IAiBehavior
{
    string Id { get; }

    float Score(in BattleSnapshot snapshot, AiProfile profile);

    AiCommand Decide(in BattleSnapshot snapshot, AiProfile profile);
}
