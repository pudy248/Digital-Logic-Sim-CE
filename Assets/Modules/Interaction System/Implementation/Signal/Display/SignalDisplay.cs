using System;
using DLS.Simulation;
using UI.ThemeSystem;
using UnityEngine;
using UnityEngine.Serialization;
using static Pin;

namespace Interaction.Display
{
    [RequireComponent(typeof(ChipSignal))]
    public class SignalDisplay : MonoBehaviour, IThemeSettable
    {
        public Palette signalPalette;

        public Palette.VoltageColour CurrentTheme;

        //Signals
        public MeshRenderer indicatorRenderer;
        public Transform indicator;
        [FormerlySerializedAs("pin")] public Transform PinDisplay;
        public Transform Connection;


        private bool Interactable;

        private void Awake()
        {
            signalPalette = ThemeManager.Palette;
            CurrentTheme = signalPalette.GetDefaultTheme();

            var e = GetComponent<ChipSignal>();
            e.OnStateChange += (wireType, state) => DrawSignals(state, wireType);

            ScalingManager.i.OnScaleChange += UpdateScale;
        }

        private void Start()
        {
            UpdateScale();
        }

        private void OnDestroy()
        {
            ScalingManager.i.OnScaleChange -= UpdateScale;
        }

        [Header("Scaling")] public float IndicatoMultiplayer = 2.8f;
        public float Pinfactor = 6.5f;
        public float PinOffset = -2.75f;
        public float Connectfactor = 1f;
        public float ConnectOffset = 0f;

        private void UpdateScale()
        {
            Connection.transform.localScale = new Vector3(ScalingManager.PinSize, ScalingManager.WireThickness / 10, 1);
            PinDisplay.transform.localPosition = new Vector3(ScalingManager.PinSize, 0, -0.1f);
            PinDisplay.localPosition = new Vector3(ScalingManager.PinSize * Pinfactor + PinOffset, PinDisplay.localPosition.y,
                PinDisplay.localPosition.z);
            Connection.localPosition = new Vector3(ScalingManager.PinSize * Connectfactor + ConnectOffset,
                Connection.localPosition.y, Connection.localPosition.z);

            indicator.transform.localScale = new Vector3(ScalingManager.PinSize * IndicatoMultiplayer,
                ScalingManager.PinSize * IndicatoMultiplayer, 1);
            
        }
        

        private void OnValidate()
        {
            if (ScalingManager.i != null)
                UpdateScale();
        }


        private WireType WireType;
        private PinStates State;

        private void DrawSignals(PinStates state, WireType wireType = WireType.Simple)
        {
            WireType = wireType;
            
            State = state ?? PinStates.Zero;
            
            if (!indicatorRenderer) return;

            indicatorRenderer.material.color = CurrentTheme.GetColour(State, wireType)[0];
        }

        public void SetTheme(Palette.VoltageColour voltageColour)
        {
            CurrentTheme = voltageColour;
            DrawSignals(State, WireType);
        }
    }
}