using UnityEngine;

[CreateAssetMenu(menuName = "Palette/PinInteractionPalette")]
public class PinInteractionPalette : ScriptableObject
{
    public Color handleCol;
    public Color highlightedHandleCol;
    public Color selectedHandleCol;
    public Color selectedAndFocusedHandleCol;
}