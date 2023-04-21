using System.Collections;
using System.Collections.Generic;
using Interaction.Display;
using UnityEngine;
using UnityEngine.Serialization;

// Base class for input and output signals
[RequireComponent(typeof(SignalPinDisplay))]
public class ChipSignal : Chip
{
    private SignalPinDisplay Display;
    public uint currentState;

    protected override void Awake()
    {
        Display = GetComponent<SignalPinDisplay>();
    }

    public TMPro.TextMeshProUGUI busReadout;

    public bool displayGroupDecimalValue { get; set; } = false;
    public bool useTwosComplement { get; set; } = true;
    public Pin.WireType wireType = Pin.WireType.Simple;
    public int GroupID { get; set; } = -1;

    [HideInInspector] public string signalName;

    protected bool interactable = true;

    public virtual void SetInteractable(bool interactable)
    {
        this.interactable = interactable;
        Display.DrawSignals(interactable);
    }

    public void SetDisplayState(uint state)
    {
        if (!interactable) return;

        Display.DrawSignals(interactable, wireType, state);
        busReadout.gameObject.SetActive(state != 0 && wireType != Pin.WireType.Simple);
        busReadout.text = state.ToString();
    }

    public static bool InSameGroup(ChipSignal signalA, ChipSignal signalB) =>
        (signalA.GroupID == signalB.GroupID) && (signalA.GroupID != -1);


    public virtual void UpdateSignalName(string newName) => signalName = newName;
}