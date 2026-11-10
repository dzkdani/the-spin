using System;

[Serializable]
public sealed class StatModifierSpec
{
    public StatId stat;
    public ModOp op;
    public float value;
    public string scope;
}
