using System.Collections.Generic;
using Core;
using DLS.Simulation;
using UnityEngine;

public class TriStateBuffer : BuiltinChip
{
    
    public override void Init()
    {
        base.Init();
        ChipType = ChipType.Miscellaneous;
        PackageGraphicData = new PackageGraphicData()
        {
            PackageColour = new Color(0,0,0, 255)
        };
        inputPins = new List<Pin>(2);
        outputPins = new List<Pin>(1);
        chipName = "TRI-STATE BUFFER";
    }
    
    protected override void ProcessOutput()
    {
        var data = inputPins[0].State;
        var enable = inputPins[1].State;

        outputPins[0].ReceiveSignal(enable == PinState.HIGH ? data : PinState.FLOATING);
    }
}