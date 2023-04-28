using System.Collections.Generic;
using Core;
using DLS.Simulation;
using UnityEngine;

public class NotGate : BuiltinChip {

	public override void Init()
	{
		base.Init();
		ChipType = ChipType.Gate;
		PackageGraphicData = new PackageGraphicData()
		{
			PackageColour = new Color(140, 43, 36, 255)
		};
		inputPins = new List<Pin>(1);
		outputPins = new List<Pin>(1);
		chipName = "NOT";
	}

	protected override void ProcessOutput () {
		PinState outputSignal = 1 - inputPins[0].State;
		outputPins[0].ReceiveSignal (outputSignal);
	}
}