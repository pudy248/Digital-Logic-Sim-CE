using DLS.Simulation;
using UnityEngine;

public class TriStateBuffer : BuiltinChip
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void ProcessOutput()
    {
        var data = inputPins[0].State;
        var enable = inputPins[1].State;

        outputPins[0].ReceiveSignal(enable == PinState.HIGH ? data : PinState.FLOATING);
    }
}