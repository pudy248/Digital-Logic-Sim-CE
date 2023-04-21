using System;
using UnityEngine;

namespace Interaction.Display
{
    public class SignalPinDisplay : MonoBehaviour
    {
        public PinPalette PinPalette;
        //Signals
        public MeshRenderer indicatorRenderer;
        public MeshRenderer pinRenderer;
        public MeshRenderer wireRenderer;

        private void Awake()
        {
            PinPalette = UIManager.instance.palette.pinPalette;
        }

        public void DrawSignals(bool interactable, Pin.WireType wireType = Pin.WireType.Simple, uint state = 0)
        {
            if (indicatorRenderer && interactable)
            {
                indicatorRenderer.material.color = wireType == Pin.WireType.Simple
                    ? (state == 1 ? PinPalette.onCol : PinPalette.offCol)
                    : (state > 0 ? PinPalette.busColor : PinPalette.offCol);
            }
            else
            {
                indicatorRenderer.material.color = PinPalette.nonInteractableCol;
                pinRenderer.material.color = PinPalette.nonInteractableCol;
                wireRenderer.material.color = PinPalette.nonInteractableCol;
            }
        }
    }
}