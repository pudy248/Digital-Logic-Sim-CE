using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interaction;
using Interaction.Builder;
using UnityEngine;
using UnityEngine.Serialization;

// Allows player to add/remove/move/rename inputs or outputs of a chip.
public class ChipInterfaceEditor : Interactable
{
    const int maxGroupSize = 16;

    [SerializeField] private SignalInteraction SignalInteractablePref;

    public event Action<Chip> OnDeleteChip;
    public event Action OnChipsAddedOrDeleted;

    public enum EditorType
    {
        Input,
        Output
    }


    public EditorType editorType;

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

    SignalInteraction highlightedSignal;
    public SignalInteraction selectedSignals { get; private set; }

    BoxCollider2D inputBounds;


    bool mouseInInputBounds;

    // Dragging
    bool isDragging;
    float dragHandleStartY;
    float dragMouseStartY;

    // Grouping
    private int DesiredGroupSize
    {
        get => _desiredGroupSize;
        set => _desiredGroupSize = Mathf.Clamp(value, 1, maxGroupSize);
    }

    private int _desiredGroupSize = 1;


    public Dictionary<int, SignalInteraction> SignalsByID;
    private SignalInteractionBuilder SignalBuilder;


    void Awake()
    {
        inputBounds = GetComponent<BoxCollider2D>();
        FindObjectOfType<CreateGroup>().onGroupSizeSettingPressed += (x) => DesiredGroupSize = x;
    }

    private void Start()
    {
        SignalsByID = new Dictionary<int, SignalInteraction>();
        SignalBuilder = new SignalInteractionBuilder(SignalInteractablePref,signalHolder,OnDeleteChip,BoundsBottom,BoundsTop);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !InputHelper.MouseOverUIObject())
            ReleaseFocus();
    }

    public override void FocusLostHandler()
    {
        highlightedSignal = null;
        ClearSelectedSignals();
    }


    // Event handler when changed input or output pin wire type
    public void ChangeWireType(int mode)
    {
        if (IsSomethingSelected)
            selectedSignals.ChangeWireType(mode);
    }

    public override void OrderedUpdate()
    {
        if (!InputHelper.MouseOverUIObject())
            HandleInput();
        else if (HasFocus)
            ReleaseFocusNotHandled();

        DrawSignalHandles();
    }


    public void LoadSignal(InputSignal signal)
    {
        signal.transform.parent = signalHolder;
        signal.signalName = signal.outputPins[0].pinName;
    }

    public void LoadSignal(OutputSignal signal)
    {
        signal.transform.parent = signalHolder;
        signal.signalName = signal.inputPins[0].pinName;
    }

    void HandleInput()
    {
        Vector2 mousePos = InputHelper.MouseWorldPos;

        mouseInInputBounds = inputBounds.OverlapPoint(mousePos);


        highlightedSignal = GetSignalUnderMouse();

        if (mouseInInputBounds && highlightedSignal != null && Input.GetMouseButtonDown(0))
            RequestFocus();
        else if (!IsSomethingSelected)
        {
            ReleaseFocusNotHandled();
            isDragging = false;
        }

        if (HasFocus)
        {
            otherEditor.ClearSelectedSignals();

            if (Input.GetMouseButtonDown(0))
                SelectSignal(highlightedSignal);

            // If a signal is selected, handle movement/renaming/deletion
            if (IsSomethingSelected)
            {
                if (isDragging)
                {
                    float handleNewY = mousePos.y + (dragHandleStartY - dragMouseStartY);
                    bool cancel = Input.GetKeyDown(KeyCode.Escape);

                    if (cancel) handleNewY = dragHandleStartY;

                    selectedSignals.SetUpPosition(handleNewY);

                    if (Input.GetMouseButtonUp(0)) isDragging = false;

                    // Cancel drag and deselect
                    if (cancel) FocusLostHandler();
                }

                UpdatePropertyUIPosition();

                // Finished with selected signal, so deselect it
                if (Input.GetKeyDown(KeyCode.Return)) FocusLostHandler();
            }
        }


        if (highlightedSignal != null || isDragging) return;
        if (!mouseInInputBounds || InputHelper.MouseOverUIObject()) return;

        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Plus, KeyCode.KeypadPlus, KeyCode.Equals))
            DesiredGroupSize++;
        else if (InputHelper.AnyOfTheseKeysDown(KeyCode.Minus, KeyCode.KeypadMinus, KeyCode.Underscore))
            DesiredGroupSize--;

        HandleSpawning();
    }


    void ClearSelectedSignals()
    {
        selectedSignals = null;
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

    float ClampYBetweenBorder(float y)
    {
        return Mathf.Clamp(y, BoundsBottom + ScalingManager.handleSizeY / 2f,
            BoundsTop - ScalingManager.handleSizeY / 2f);
    }

    // Handles spawning if user clicks, otherwise displays preview
    void HandleSpawning()
    {
        if (InputHelper.MouseOverUIObject())
            return;

        float containerX = chipContainer.position.x +
                           chipContainer.localScale.x / 2 *
                           ((editorType == EditorType.Input) ? -1 : 1);

        // Spawn on mouse down
        if (Input.GetMouseButtonDown(0))
        {
            var vec = new Vector3(containerX, InputHelper.MouseWorldPos.y, chipContainer.position.z);
            var Interactable = SignalBuilder.Build(vec,DesiredGroupSize);
            SignalsByID.Add(Interactable.id, Interactable.obj);
            DesiredGroupSize = 1;
            SelectSignal(Interactable.obj);
            OnChipsAddedOrDeleted?.Invoke();
        }
    }

    float BoundsTop => transform.position.y + (transform.localScale.y / 2);
    float BoundsBottom => transform.position.y - transform.localScale.y / 2f;

    bool IsSomethingSelected => selectedSignals != null;


    public override bool CanReleaseFocus() => !isDragging && !mouseInInputBounds;


    void UpdatePropertyUIPosition()
    {
        if (IsSomethingSelected)
        {
            UIManager.instance.PinPropertiesMenu.SetPosition(selectedSignals.GetGroupCenter(), editorType);
        }
    }

    public void UpdateGroupProperty(string newName, bool twosComplementToggle)
    {
        selectedSignals.UpdateProperty(newName, twosComplementToggle);
    }

    void DrawSignalHandles()
    {
        foreach (SignalInteraction singnal in SignalsByID.Values)
            singnal.DrawSignalsHandle();
    }

    SignalInteraction GetSignalUnderMouse()
    {
        // ChipSignal signalUnderMouse = null;
        // float nearestDst = float.MaxValue;
        //
        // for (int i = 0; i < signals.Count; i++)
        // {
        //     ChipSignal currentSignal = signals[i];
        //     float handleY = currentSignal.transform.position.y;
        //
        //     Vector2 handleCentre = new Vector2(transform.position.x, handleY);
        //     Vector2 mousePos = InputHelper.MouseWorldPos;
        //
        //     const float selectionBufferY = 0.1f;
        //
        //     float halfSizeX = handleSizeX;
        //     float halfSizeY = (ScalingManager.handleSizeY + selectionBufferY) / 2f;
        //     bool insideX = mousePos.x >= handleCentre.x - halfSizeX &&
        //                    mousePos.x <= handleCentre.x + halfSizeX;
        //     bool insideY = mousePos.y >= handleCentre.y - halfSizeY &&
        //                    mousePos.y <= handleCentre.y + halfSizeY;
        //
        //     if (insideX && insideY)
        //     {
        //         float dst = Mathf.Abs(mousePos.y - handleY);
        //         if (dst < nearestDst)
        //         {
        //             nearestDst = dst;
        //             signalUnderMouse = currentSignal;
        //         }
        //     }
        // }

        var e = InputHelper.GetUIObjectsUnderMouse();
        foreach (var obj in e)
        {
            var p = obj.GetComponent<ChipSignal>();
            if (p is not null)
                return p.GetComponentInParent<SignalInteraction>();
        }

        return null;
    }

    // Select signal (starts dragging, shows rename field)
    void SelectSignal(SignalInteraction signalToDrag)
    {
        if (signalToDrag == null) return;

        isDragging = true;

        dragMouseStartY = InputHelper.MouseWorldPos.y;

        signalToDrag.SelectSignal();

        UIManager.instance.PinPropertiesMenu.EnableUI(this,
            signalToDrag.Signals[0].signalName, signalToDrag.Signals.Count > 1,
            signalToDrag.Signals[0].useTwosComplement, currentEditorName,
            signalToDrag.Signals[0].signalName, (int)signalToDrag.Signals[0].wireType);

        UpdatePropertyUIPosition();
    }


    public void UpdateScale()
    {
        transform.localPosition =
            new Vector3(ScalingManager.ioBarDistance * (editorType == EditorType.Input ? -1f : 1f),
                transform.localPosition.y, transform.localPosition.z);
        barGraphic.localScale = new Vector3(ScalingManager.ioBarGraphicWidth, 1, 1);
        GetComponent<BoxCollider2D>().size = new Vector2(ScalingManager.ioBarGraphicWidth, 1);

        foreach (var sig in SignalsByID.Values)
            sig.UpdateScale();

        UpdatePropertyUIPosition();
    }


    public override void DeleteCommand()
    {
        if (!Input.GetKeyDown(KeyCode.Backspace))
            DeleteSelected();
    }

    private void DeleteSelected()
    {
        OnChipsAddedOrDeleted?.Invoke();
        ReleaseFocus();
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