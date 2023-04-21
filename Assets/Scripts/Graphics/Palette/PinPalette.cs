using UnityEngine;

[CreateAssetMenu(menuName = "Palette/PinPalette")]
public class PinPalette : ScriptableObject {
    public Color onCol;
    public Color offCol;
    public Color highZCol;
    public Color busColor;
    public Color selectedColor;
    public Color nonInteractableCol;

}