using UnityEngine;

[CreateAssetMenu(
    fileName = "BeyPaletteDatabase",
    menuName = "Beyblade/Palette Database")]
public class BeyPaletteDatabase : ScriptableObject
{
    public BeyColorPalette[] palettes;
}