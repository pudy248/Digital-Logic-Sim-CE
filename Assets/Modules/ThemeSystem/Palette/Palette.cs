using System.Collections.ObjectModel;
using System.Linq;
using DLS.Simulation;
using Interaction;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Palette")]
public class Palette : ScriptableObject
{
    public PinInteractionPalette PinInteractionPalette;


    public Color nonInteractableCol;
    [SerializeField] int defaultIndex;


    [SerializeField] public VoltageColour[] voltageColours;
    public ReadOnlyCollection<VoltageColour> Colours => new(voltageColours);
    public VoltageColour GetDefaultTheme() => voltageColours[defaultIndex];

    public int DefaultIndex
    {
        get => defaultIndex;
        set
        {
            if (value >= voltageColours.Length)
            {
                defaultIndex = 0;
                return;
            }

            if (value < 0)
            {
                defaultIndex = voltageColours.Length - 1;
                return;
            }

            defaultIndex = value;
        }
    }


    public VoltageColour GetTheme(string themeName)
    {
        if (string.IsNullOrEmpty(themeName)) return GetDefaultTheme();

        return voltageColours.FirstOrDefault(x => x.Name.Equals(themeName)) ?? GetDefaultTheme();
    }


    [System.Serializable]
    public class VoltageColour
    {
        [FormerlySerializedAs("name")] public string Name;
        public Color High;
        public Color HighBus;
        public Color Low;
        public Color Tristate;
        public int DisplayPriority;

        public Color GetColour(PinState state, Pin.WireType wireType = Pin.WireType.Simple, bool useTriStateCol = true)
        {
            return state switch
            {
                PinState.HIGH => GetHigh(wireType),
                PinState.LOW => Low,
                _ => useTriStateCol ? Tristate : Low
            };
        }


        public Color GetHigh(Pin.WireType startPinWireType)
        {
            return startPinWireType != Pin.WireType.Simple ? HighBus : High;
        }
    }
}