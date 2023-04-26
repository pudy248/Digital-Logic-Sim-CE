using System;
using System.Collections;
using System.Collections.Generic;
using DLS.Simulation;
using Interaction.Display;
using UnityEngine;
using UnityEngine.Serialization;
using static Pin;

// Base class for input and output signals
[RequireComponent(typeof(SignalDisplay))]
public class ChipSignal : Chip
{
    
    
    public event Action<WireType,PinState> OnStateChange; 
    public event Action<bool> OnInteractableSet;
    
    public PinState currentState;
    public WireType wireType = WireType.Simple;

    protected bool interactable = true;
    public bool displayGroupDecimalValue { get; set; } = false;
    public bool useTwosComplement { get; set; } = true;

    [HideInInspector] public string signalName;


    protected override void Start () {
        base.Start();
        NotifyStateChange();
    }
    
    
    public virtual void SetInteractable(bool interactable)
    {
        this.interactable = interactable;
        OnInteractableSet?.Invoke(interactable);
    }
    public virtual void UpdateSignalName(string newName) => signalName = newName;

    public void NotifyStateChange()
    {
        if (!interactable) return;
        OnStateChange?.Invoke(wireType,currentState);
    }


}