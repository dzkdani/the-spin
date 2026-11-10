using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MatchupTable",
    menuName = "Bey/Matchup Table")]
public class MatchupTable : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public ElementDefinition attacker;

        public ElementDefinition defender;

        public float multiplier = 1f;
    }

    public List<Entry> entries = new List<Entry>();
}
