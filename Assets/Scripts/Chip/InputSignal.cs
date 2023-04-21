﻿using System;
using System.Linq;
using Interaction.Display;
using UnityEngine;

// Provides input signal (0 or 1) to a chip.
// When designing a chip, this input signal can be manually set to 0 or 1 by the player.
[RequireComponent(typeof(SignalPinDisplay))]
public class InputSignal : ChipSignal
{
	public TMPro.TMP_InputField busInput;

	protected override void Start()
	{
		base.Start();
		SetCol();
	}


	public void ToggleActive()
	{
		currentState = 1 - currentState;
		SetCol();
	}

	public void SetState(uint state)
	{
		currentState = state >= 1 ? 1U : 0U;
		SetCol();
	}

	public void SendSignal(uint signal)
	{
		currentState = signal;
		outputPins[0].ReceiveSignal(signal);
		SetCol();
	}

	public void SendOffSignal()
	{
		outputPins[0].ReceiveSignal(0);
		SetCol();
	}

	public void SendSignal()
	{
		outputPins[0].ReceiveSignal(currentState);
	}

	void SetCol()
	{
		SetDisplayState(currentState);
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
			busReadout.gameObject.SetActive(false);
			busInput.gameObject.SetActive(true);
			busInput.Select();
			//indicatorRenderer.material.color = palette.editColor;
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
		currentState = Math.Max(enteredState, 0);
		busInput.text = currentState.ToString();
		busInput.gameObject.SetActive(false);
		SetCol();
	}
}