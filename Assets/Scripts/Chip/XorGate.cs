using DLS.Simulation;
using UnityEngine;

public class XorGate : BuiltinChip
{

    protected override void ProcessOutput()
    {
        PinState outputSignal = inputPins[0].State ^ inputPins[1].State;
        outputPins[0].ReceiveSignal(outputSignal);
    }

}