using System.Collections.Generic;

public sealed class ElementStack
{
    private readonly List<ElementClaim> claims =
        new List<ElementClaim>();

    public ElementDefinition Current
    {
        get
        {
            ElementClaim? best = null;

            for (int i = 0; i < claims.Count; i++)
            {
                ElementClaim claim = claims[i];

                if (best == null || claim.Priority > best.Value.Priority)
                {
                    best = claim;
                    continue;
                }

                if (claim.Priority == best.Value.Priority &&
                    claim.Sequence > best.Value.Sequence)
                {
                    best = claim;
                }
            }

            return best?.Element;
        }
    }

    public void Add(ElementClaim claim)
    {
        claims.Add(claim);
    }

    public void RemoveBySource(string sourceId)
    {
        claims.RemoveAll(claim => claim.SourceId == sourceId);
    }

    public void Tick(float now)
    {
        claims.RemoveAll(claim => claim.IsExpired(now));
    }

    public IEnumerable<ElementClaim> Explain()
    {
        List<ElementClaim> live = new List<ElementClaim>(claims);

        live.Sort((a, b) =>
        {
            int byPriority = b.Priority.CompareTo(a.Priority);

            return byPriority != 0
                ? byPriority
                : b.Sequence.CompareTo(a.Sequence);
        });

        return live;
    }
}
