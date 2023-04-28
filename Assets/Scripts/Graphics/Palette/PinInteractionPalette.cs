using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Palette/PinInteractionPalette")]
public class PinInteractionPalette : ScriptableObject
{
    [Header("Signal Handler")]
    public Color handleCol;
    public Color highlightedHandleCol;
    public Color FocusedHandleCol;

    
    [Header("PIN")]
    public Color PinHighlighte;
    public Color Pindefault;
    
    
    [Header("Wire")]
    public Color WireHighlighte;

}