using System;
using DLS.Simulation;
using UnityEngine;
using UnityEngine.Serialization;
using static Pin;

namespace Interaction.Display
{
    [RequireComponent(typeof(ChipSignal))]
    public class SignalDisplay : MonoBehaviour
    {
        public Palette signalPalette;

        public Palette.VoltageColour CurrentTheme;

        //Signals
        public MeshRenderer indicatorRenderer;
        public MeshRenderer pinRenderer;
        public MeshRenderer wireRenderer;

        private bool Interactable;

        private void Awake()
        {
            signalPalette = UIThemeManager.Palette;
            CurrentTheme = signalPalette.GetDefaultTheme();

            var e = GetComponent<ChipSignal>();
            e.OnStateChange += DrawSignals;
            e.OnInteractableSet += SetInteractable;
        }

        private void SetInteractable(bool interactable)
        {
            Interactable = interactable;
        }

        private void DrawSignals(WireType wireType = WireType.Simple, PinState state = PinState.LOW)
        {
            if (indicatorRenderer && Interactable)
            {
                indicatorRenderer.material.color = CurrentTheme.GetColour(state, wireType); 
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