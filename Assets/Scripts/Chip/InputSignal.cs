using System;
using System.Linq;
using DLS.Simulation;
using Interaction.Display;
using UnityEngine;

// Provides input signal (0 or 1) to a chip.
// When designing a chip, this input signal can be manually set to 0 or 1 by the player.
[RequireComponent(typeof(SignalDisplay))]
public class InputSignal : ChipSignal
{
	public TMPro.TMP_InputField busInput;

	public void ToggleActive()
	{
		currentState = 1 - currentState;
		NotifyStateChange();
	}

	public void SetState(PinState pinState)
	{
		currentState = pinState;
		NotifyStateChange();
	}

	public void SendSignal(PinState signal)
	{
		currentState = signal;
		Debug.Log("aiuto"+name,this);
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

	void OnMouseDown()
	{
		// Allow only to click on single wires, not on bus wires
		if (outputPins.All(x => x.wireType == Pin.WireType.Simple))
			ToggleActive();
		else
		{
			busInput.gameObject.SetActive(true);
			busInput.Select();
		}
	}

	public void OnEndEdit()
	{
		uint enteredState;
		try
		{
			enteredState = uint.Parse(busInput.text == string.Empty ? "0" : busInput.text);
		}
		catch(Exception e) {
			if (e is OverflowException)
				enteredState = int.MaxValue;
			else throw e;
		}
		enteredState = wireType != Pin.WireType.Bus32 ? Math.Min(enteredState, (uint)(1 << Pin.NumBits(wireType)) - 1) : enteredState;
		currentState = (PinState)Math.Max(enteredState, 0);
		busInput.text = currentState.ToString();
		busInput.gameObject.SetActive(false);
		NotifyStateChange();
	}
}