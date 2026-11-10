using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ContentManifest",
    menuName = "Bey/Content Manifest")]
public class ContentManifest : ScriptableObject
{
    [Header("Content")]
    public List<ContentDefinition> assets = new List<ContentDefinition>();

    [Header("Rules")]
    public CombatTuning combatTuning;

    public MatchupTable matchupTable;
}
