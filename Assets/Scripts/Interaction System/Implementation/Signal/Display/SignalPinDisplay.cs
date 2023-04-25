using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interaction.Display
{
    public class SignalPinDisplay : MonoBehaviour
    {
        [FormerlySerializedAs("PinPalette")] public SignalPalette signalPalette;
        //Signals
        public MeshRenderer indicatorRenderer;
        public MeshRenderer pinRenderer;
        public MeshRenderer wireRenderer;

        private void Awake()
        {
            signalPalette = UIThemeManager.Palette.signalPalette;
        }

        public void DrawSignals(bool interactable, Pin.WireType wireType = Pin.WireType.Simple, uint state = 0)
        {
            if (indicatorRenderer && interactable)
            {
                indicatorRenderer.material.color = wireType == Pin.WireType.Simple
                    ? (state == 1 ? signalPalette.onCol : signalPalette.offCol)
                    : (state > 0 ? signalPalette.busColor : signalPalette.offCol);
            }
            else
            {
                indicatorRenderer.material.color = signalPalette.nonInteractableCol;
                pinRenderer.material.color = signalPalette.nonInteractableCol;
                wireRenderer.material.color = signalPalette.nonInteractableCol;
            }
        }
    }
}