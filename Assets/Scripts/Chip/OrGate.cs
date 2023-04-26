using DLS.Simulation;
using UnityEngine;

public class OrGate : BuiltinChip {

	protected override void Awake () {
		base.Awake ();
	}

	protected override void ProcessOutput () {
		PinState outputSignal = inputPins[0].State | inputPins[1].State;
		outputPins[0].ReceiveSignal (outputSignal);
	}

}