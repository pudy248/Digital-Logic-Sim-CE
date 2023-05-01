using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using DLS.Simulation;
using Interaction.Display;
using SebInput.Internal;
using UnityEngine;
using UnityEngine.Serialization;

// Provides input signal (0 or 1) to a chip.
// When designing a chip, this input signal can be manually set to 0 or 1 by the player.
[RequireComponent(typeof(SignalDisplay))]
public class InputSignal : ChipSignal
{
    public void ToggleActive()
    {
        if (currentState[0] == PinState.HIGH)
            currentState[0] = PinState.LOW;
        else
            currentState[0] = PinState.HIGH;
        NotifyStateChange();
    }

    public void SetState(PinStates pinState)
    {
        currentState = pinState;
        NotifyStateChange();
    }

    public void SendSignal(PinStates signal)
    {
        currentState = signal;
        outputPins[0].ReceiveSignal(signal);
        NotifyStateChange();
    }

    public void SendSignal()
    {
        outputPins[0].ReceiveSignal(currentState);
    }


    public override void UpdateSignalName(string newName)
    {
        base.UpdateSignalName(newName);
        outputPins[0].pinName = newName;
    }

    protected override void Start()
    {
        base.Start();
        GetComponentInChildren<SignalEvent>().MouseInteraction.LeftMouseDown += LeftClickHandler;
    }


    void LeftClickHandler()
    {
        // Allow only to click on single wires, not on bus wires
        if (outputPins.All(x => x.wireType == Pin.WireType.Simple))
            ToggleActive();
    }

    public void SetBusStatus(uint state)
    {
        currentState = new PinStates(state);
        NotifyStateChange();
    }
}