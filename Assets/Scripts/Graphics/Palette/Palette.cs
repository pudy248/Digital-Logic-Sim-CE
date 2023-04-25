using Interaction;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Palette")]
public class Palette : ScriptableObject
{
  [FormerlySerializedAs("pinPalette")] public SignalPalette signalPalette;
  public PinInteractionPalette PinInteractionPalette;
}