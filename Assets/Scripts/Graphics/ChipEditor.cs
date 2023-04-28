using UnityEngine;

public class ChipEditor : MonoBehaviour
{
    public Transform chipImplementationHolder;
    public Transform wireHolder;

    public ChipInterfaceEditor inputsEditor;
    public ChipInterfaceEditor outputsEditor;
    public ChipInteraction chipInteraction;
    public PinAndWireInteraction pinAndWireInteraction;

    public PinNameDisplayManager pinNameDisplayManager ;

    public ChipData Data;

    void Awake()
    {
        Data = new ChipData()
        {
            FolderIndex = 0,
            scale = 1
        };


        pinAndWireInteraction.Init(chipInteraction, inputsEditor, outputsEditor);
        pinAndWireInteraction.onConnectionChanged += OnChipNetworkModified;
     
        ScalingManager.OnScaleChange +=inputsEditor.UpdateScale;
        ScalingManager.OnScaleChange +=outputsEditor.UpdateScale;
        ScalingManager.OnScaleChange += () => pinNameDisplayManager.UpdateTextSize(ScalingManager.PinDisplayFontSize);
    }

    void LateUpdate()
    {
        pinAndWireInteraction.OrderedUpdate();
        chipInteraction.OrderedUpdate();
    }

    void OnChipNetworkModified() { CycleDetector.MarkAllCycles(this); }

    public void LoadFromSaveData(ChipSaveData saveData)
    {
        Data = saveData.Data;
        ScalingManager.i.SetScale(Data.scale);

        // Load component chips
        foreach (Chip componentChip in saveData.componentChips)
        {
            switch (componentChip)
            {
                case InputSignal inp:
                    inp.wireType = inp.outputPins[0].wireType;
                    inputsEditor.LoadSignal(inp);
                    break;
                case OutputSignal outp:
                    outp.wireType = outp.inputPins[0].wireType;
                    outputsEditor.LoadSignal(outp);
                    break;
                default:
                    chipInteraction.LoadChip(componentChip);
                    break;
            }
        }

        // Load wires
        if (saveData.wires != null)
        {
            foreach (Wire wire in saveData.wires)
            {
                pinAndWireInteraction.LoadWire(wire);
            }
        }

        ChipEditorOptions.instance.SetUIValues(this);
    }

}
