using UnityEngine;

[CreateAssetMenu(menuName = "Palette/SignalPalette")]
public class SignalPalette : ScriptableObject {
    public Color onCol;
    public Color offCol;
    public Color highZCol;
    public Color busColor;
    public Color selectedColor;
    public Color nonInteractableCol;

}