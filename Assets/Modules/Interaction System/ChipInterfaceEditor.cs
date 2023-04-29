using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interaction;
using Interaction.Builder;
using UnityEngine;
using UnityEngine.Serialization;

// Allows player to add/remove/move/rename inputs or outputs of a chip.
public class ChipInterfaceEditor : MonoBehaviour
{
    const int maxGroupSize = 16;

    [SerializeField] private SignalInteraction SignalInteractablePref;

    public event Action<Chip> OnDeleteChip;
    public event Action OnChipsAddedOrDeleted;


    public enum EditorInterfaceType
    {
        Input,
        Output
    }

    public EditorInterfaceType editorInterfaceType;

    [Header("References")] public Transform chipContainer;


    public Transform signalHolder;
    public Transform barGraphic;
    public ChipInterfaceEditor otherEditor;

    public bool showPreviewSignal;

    string currentEditorName;

    public ChipEditor CurrentEditor
    {
        set => currentEditorName = value.Data.name;
    }

    public SignalInteraction selectedSignals { get; private set; }

    // Grouping
    private int DesiredGroupSize
    {
        get => _desiredGroupSize;
        set => _desiredGroupSize = Mathf.Clamp(value, 1, maxGroupSize);
    }

    private int _desiredGroupSize = 1;


    private Dictionary<int, SignalInteraction> SignalsByID= new Dictionary<int, SignalInteraction>();
    private SignalInteractionBuilder SignalBuilder;


    void Awake()
    {
        
        FindObjectOfType<CreateGroup>().onGroupSizeSettingPressed += (x) => DesiredGroupSize = x;
       
        float BoundsTop = transform.position.y + (transform.localScale.y / 2);
        float BoundsBottom = transform.position.y - transform.localScale.y / 2f;
        // Handles spawning if user clicks, otherwise displays preview
        float containerX = chipContainer.position.x +
                           chipContainer.localScale.x / 2 *
                           ((editorInterfaceType == EditorInterfaceType.Input) ? -1 : 1);

        SignalBuilder = new SignalInteractionBuilder(SignalInteractablePref, signalHolder, OnDeleteChip, BoundsBottom,
            BoundsTop, containerX, chipContainer.position.z, editorInterfaceType);
    }

    private void Start()
    {
        ScalingManager.i.OnScaleChange += UpdateScale;
    }

    private void OnDestroy()
    {
        ScalingManager.i.OnScaleChange -= UpdateScale;
    }



    public Chip LoadSignal(InputSignal signal, float y)
    {
        return AddSignal(y,1).Signals[0];
    }

    public Chip LoadSignal(OutputSignal signal, float y)
    {
        return AddSignal(y,1).Signals[0];
    }


    private void OnMouseDown()
    {
        if (InputHelper.MouseOverUIObject()) return;

        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Plus, KeyCode.KeypadPlus, KeyCode.Equals))
            DesiredGroupSize++;
        else if (InputHelper.AnyOfTheseKeysDown(KeyCode.Minus, KeyCode.KeypadMinus, KeyCode.Underscore))
            DesiredGroupSize--;

        HandleSpawning();
    }


    public ChipSignal[][] GetGroups()
    {
        var keys = SignalsByID.Keys;
        ChipSignal[][] groups = new ChipSignal[keys.Count][];
        int i = 0;
        foreach (var key in keys)
        {
            groups[i] = SignalsByID[key].Signals.ToArray();
            i++;
        }

        return groups;
    }


    void HandleSpawning()
    {
        if (InputHelper.MouseOverUIObject())
            return;


        // Spawn on mouse down
        if (!Input.GetMouseButtonDown(0)) return;

        if (InputHelper.CompereTagObjectUnderMouse2D(ProjectTags.InterfaceMask, ProjectLayer.Default)) return;


        AddSignal(InputHelper.MouseWorldPos.y, DesiredGroupSize);
        DesiredGroupSize = 1;


        OnChipsAddedOrDeleted?.Invoke();
    }

    private SignalInteraction AddSignal(float yPos, int groupSize)
    {
        var Interactable = SignalBuilder.Build(yPos, groupSize);
        SignalsByID.Add(Interactable.id, Interactable.obj);
        return  Interactable.obj;
    }

    public void UpdateScale()
    {
        transform.localPosition =
            new Vector3(ScalingManager.IoBarDistance * (editorInterfaceType == EditorInterfaceType.Input ? -1f : 1f),
                transform.localPosition.y, transform.localPosition.z);
        barGraphic.localScale = new Vector3(ScalingManager.IoBarGraphicWidth, 1, 1);
        GetComponent<BoxCollider2D>().size = new Vector2(ScalingManager.IoBarGraphicWidth, 1);


    }


    public List<ChipSignal> GetAllSignals()
    {
        var res = new List<ChipSignal>();
        foreach (var e in SignalsByID.Values)
        {
            res.AddRange(e.Signals);
        }

        return res;
    }

    public List<Pin> GetAllPin()
    {
        var e = GetAllSignals();
        var res = new List<Pin>();
        foreach (var signal in GetAllSignals())
        {
            res.AddRange(signal.inputPins);
            res.AddRange(signal.outputPins);
        }

        return res;
    }


}