using Interaction;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Palette")]
public class Palette : ScriptableObject
{
  public PinPalette pinPalette;
  public PinInteractionPalette PinInteractionPalette;
}