using UnityEngine;

public static class WaveReset
{
    public static void Prepare(BeyController bey, Vector3 position)
    {
        if (bey == null || bey.Data == null)
            return;

        bey.Data.Stats.RemoveBySourcePrefix("element:");
        bey.transform.position = position;

        if (bey.Physics != null && bey.Physics.Rigidbody != null)
            bey.Physics.Rigidbody.linearVelocity = Vector2.zero;
    }
}
