using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Palette/PinInteractionPalette")]
public class PinInteractionPalette : ScriptableObject
{
    public Color handleCol;
    public Color highlightedHandleCol;
    public Color selectedHandleCol;
    public Color selectedAndFocusedHandleCol;

    public Color PinHighlighte;
    public Color Pindefault;
    
    
    public Color WireHighlighte;

}