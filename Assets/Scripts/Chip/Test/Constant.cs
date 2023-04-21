using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Constant : Chip {
	public bool high;
	public MeshRenderer meshRenderer;
	[FormerlySerializedAs("wirePalette")] [FormerlySerializedAs("palette")] public PinPalette pinPalette;
	
	public void SendSignal () {
		outputPins[0].ReceiveSignal ((high) ? 1U : 0);
		//Debug.Log ("Send const signal to " + outputPins[0].childPins[0].pinName + " " + outputPins[0].childPins[0].chip.chipName);
	}

	void Update () {
		meshRenderer.material.color = (high) ? pinPalette.onCol : pinPalette.offCol;
	}
}