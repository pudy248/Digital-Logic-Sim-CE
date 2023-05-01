﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Windows.Forms;
using Interaction.Display;
using SebInput;
using SebInput.Internal;
using TMPro;
using UI.ThemeSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PinAndWireInteraction : Interactable
{
    public event System.Action onConnectionChanged;

    public enum State
    {
        None,
        PlacingWire,
        PasteWires
    }

    public Transform wireHolder;
    public Wire wirePrefab;

    State _currentState;
    Pin wireStartPin;
    Wire wireToPlace;
    Dictionary<Pin, Wire> wiresByChipInputPin;
    public List<Wire> allWires { get; private set; }

    ChipInteraction chipInteraction;
    ChipInterfaceEditor inputEditor;
    ChipInterfaceEditor outputEditor;

    List<Wire> wiresToPaste;
    public State CurrentState => _currentState;


    float PinRadius => PinDisplay.radius / 4;
    float PinInteraction => PinRadius * PinDisplay.IteractionFactor;

    void Awake()
    {
        allWires = new List<Wire>();
        wiresToPaste = new List<Wire>();
        wiresByChipInputPin = new Dictionary<Pin, Wire>();
        OnFocusLost += FocusLostHandler;
    }


    public void Init(ChipInteraction chipInteraction, ChipInterfaceEditor inputEditor, ChipInterfaceEditor outputEditor)
    {
        this.chipInteraction = chipInteraction;
        this.inputEditor = inputEditor;
        this.outputEditor = outputEditor;
        chipInteraction.onDeleteChip += DeleteChipWires;
        inputEditor.OnDeleteChip += DeleteChipWires;
        outputEditor.OnDeleteChip += DeleteChipWires;
    }

    public override void OrderedUpdate()
    {
        var mouseOverUI = InputHelper.MouseOverUIObject();

        if (mouseOverUI) return;

        switch (_currentState)
        {
            case State.None:
                break;
            case State.PlacingWire:
            {
                if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape, KeyCode.Backspace, KeyCode.Delete))
                    StopPlacingWire(null);
                if (wireToPlace != null)
                    wireToPlace.UpdateWireEndPoint(InputHelper.MouseWorldPos);
            }
                break;
            case State.PasteWires:
                HandlePasteWires();
                break;
        }
    }

    public void PasteWires(List<WireInformation> wires, List<Chip> chips)
    {
        wiresToPaste.Clear();
        foreach (WireInformation wire in wires)
        {
            Wire newWire = Instantiate(wirePrefab, parent: wireHolder);
            allWires.Add(newWire);
            newWire.Connect(
                chips[wire.startChipIndex].outputPins[wire.startChipPinIndex],
                chips[wire.endChipIndex].inputPins[wire.endChipPinIndex]);
            newWire.SetAnchorPoints(wire.anchorPoints);
            newWire.endPin.parentPin = newWire.startPin;
            newWire.startPin.childPins.Add(newWire.endPin);
            wiresByChipInputPin.Add(newWire.ChipInputPin, newWire);
            wiresToPaste.Add(newWire);
        }

        _currentState = State.PasteWires;
    }


    public Wire CreateAndLoadWire(Pin connectedPin, Pin pin)
    {
        var e = Instantiate(wirePrefab);
        e.Connect(connectedPin, pin);
        LoadWire(e);
        return e;
    }

    public void LoadWire(Wire wire)
    {
        wire.transform.parent = wireHolder;
        allWires.Add(wire);
        wiresByChipInputPin.Add(wire.ChipInputPin, wire);
    }

    void HandlePasteWires()
    {
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape, KeyCode.Backspace,
                KeyCode.Delete) || Input.GetMouseButtonDown(1))
        {
            foreach (Wire wire in wiresToPaste)
                DestroyWire(wire);

            wiresToPaste.Clear();
            _currentState = State.None;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            wiresToPaste.Clear();
            _currentState = State.None;
            Invoke(nameof(ConnectionChanged), 0.01f);
        }
    }


    void TryPlaceWire(Pin startPin, Pin endPin)
    {
        if (Pin.IsValidConnection(startPin, endPin))
        {
            Pin chipInputPin =
                (startPin.pinType == Pin.PinType.ChipInput) ? startPin : endPin;
            RemoveConflictingWire(chipInputPin);

            wireToPlace.Place(endPin);
            Pin.MakeConnection(startPin, endPin);
            allWires.Add(wireToPlace);
            wiresByChipInputPin.Add(chipInputPin, wireToPlace);

            wireToPlace = null;
            _currentState = State.None;


            onConnectionChanged?.Invoke();
        }
        else
        {
            StopPlacingWire(null);
        }
    }

    // Pin cannot have multiple inputs, so when placing a new wire, first remove
    // the wire that already goes to that pin (if there is one)
    void RemoveConflictingWire(Pin chipInputPin)
    {
        if (wiresByChipInputPin.ContainsKey(chipInputPin))
            DestroyWire(wiresByChipInputPin[chipInputPin]);
    }

    public void DestroyConnectedWires(Pin pin)
    {
        List<Wire> allWiresStatic = new(allWires);
        foreach (var w in allWiresStatic.Where(w => w.startPin == pin || w.endPin == pin))
        {
            DestroyWire(w);
        }
    }

    void DestroyWire(Wire wire)
    {
        wire.DestroyWire();
        NotifyDestroyWire(wire);
    }

    private void NotifyDestroyWire(Wire wire)
    {
        wiresByChipInputPin.Remove(wire.ChipInputPin);
        allWires.Remove(wire);
        onConnectionChanged?.Invoke();
    }


    private void StartPlaceWire()
    {
        _currentState = State.PlacingWire;
        wireToPlace = Instantiate(wirePrefab, wireHolder);
    }


    #region PinEvent

    private List<MouseInteraction<Pin>> RegisterdPinEvent = new List<MouseInteraction<Pin>>();

    public void RegisterPin(MouseInteraction<Pin> pinEvent)
    {
        pinEvent.LeftMouseDown += PinLeftClickHandler;
        pinEvent.RightMouseDown += PinRightClickHandler;
        pinEvent.LeftMouseReleased += PinLeftClickReleased;
        RegisterdPinEvent.Add(pinEvent);
    }

    private void PinLeftClickHandler(Pin pin)
    {
        switch (CurrentState)
        {
            case State.None when !RequestFocus():
                return;
            case State.None:
                StartPlaceWire();
                // Creating new wire starting from pin
                wireStartPin = pin;
                wireToPlace.ConnectToFirstPin(wireStartPin);
                break;
            case State.PlacingWire:
                // If mouse pressed over pin, try connecting the wire to that pin
                TryPlaceWire(wireStartPin, pin);
                break;
        }
    }

    private void PinLeftClickReleased(Pin pin)
    {
        if (CurrentState != State.PlacingWire || wireStartPin == null || wireStartPin == pin) return;
        TryPlaceWire(wireStartPin, pin);
    }

    private void PinRightClickHandler(Pin pin)
    {
        SetTheme(pin);
    }

    #endregion

    #region WireEvent

    private List<MouseInteraction<Wire>> RegisterdWireEvent = new List<MouseInteraction<Wire>>();

    public void RegisterWire(MouseInteraction<Wire> wireEvent)
    {
        wireEvent.LeftMouseDown += WireLeftClickHandler;
        wireEvent.RightMouseDown += WireRightClickHandler;
        wireEvent.Context.OnWireDestroy += NotifyDestroyWire;
        RegisterdWireEvent.Add(wireEvent);
    }


    private void WireRightClickHandler(Wire wire)
    {
        SetTheme(wire);
    }

    private void WireLeftClickHandler(Wire wire)
    {
        if (!wire || wire == wireToPlace || CurrentState == State.PlacingWire|| !RequestFocus()) return;
        StartPlaceWire();
        // Creating new wire starting from existing wire
        wireStartPin = wire.ChipOutputPin;
        wireToPlace.ConnectToFirstPinViaWire(wireStartPin, wire, InputHelper.MouseWorldPos);
    }

    #endregion

    #region EditorEvent

    private MouseInteraction<PlacmentAreaEvent> EditorInteraction;

    public void RegisterEditorArea(MouseInteraction<PlacmentAreaEvent> editorAreaEvent)
    {
        editorAreaEvent.LeftMouseDown += EditorClickHandler;
        editorAreaEvent.RightMouseDown += StopPlacingWire;
        EditorInteraction = editorAreaEvent;
    }

    private void EditorClickHandler(PlacmentAreaEvent e)
    {
        // Cancel placing wire
        if (CurrentState != State.PlacingWire) return;
        wireToPlace.AddAnchorPoint(InputHelper.MouseWorldPos);
    }

    #endregion


    private void OnDestroy()
    {
        foreach (var pinEvent in RegisterdPinEvent)
        {
            pinEvent.LeftMouseDown -= PinLeftClickHandler;
            pinEvent.RightMouseDown -= PinRightClickHandler;
            pinEvent.LeftMouseReleased -= PinLeftClickReleased;
        }

        foreach (var wireEvent in RegisterdWireEvent)
        {
            wireEvent.LeftMouseDown -= WireLeftClickHandler;
            wireEvent.RightMouseDown -= WireRightClickHandler;
            wireEvent.Context.OnWireDestroy -= NotifyDestroyWire;
        }

        EditorInteraction.LeftMouseDown -= EditorClickHandler;
        EditorInteraction.RightMouseDown -= StopPlacingWire;
    }


    void SetTheme<T>(T gameobject) where T : MonoBehaviour
    {
        var themeSettable = gameobject.GetComponentInChildren<IThemeSettable>(true) 
                            ?? gameobject.GetComponentInParent<IThemeSettable>(true);

        if(themeSettable == null) return;

        MenuManager.instance.themeChangerMenu.OpenUI(themeSettable);
    }


    // Delete all wires connected to given chip
    void DeleteChipWires(Chip chip)
    {
        var wiresToDestroy = (chip.outputPins.SelectMany(outputPin => outputPin.childPins,
            (outputPin, childPin) => wiresByChipInputPin[childPin])).ToList();

        wiresToDestroy.AddRange(chip.inputPins.Where(inputPin => inputPin.parentPin)
            .Select(inputPin => wiresByChipInputPin[inputPin]));

        foreach (var wire in wiresToDestroy)
            DestroyWire(wire);

        onConnectionChanged?.Invoke();
    }

    void StopPlacingWire(PlacmentAreaEvent e = null)
    {
        if (CurrentState != State.PlacingWire) return;

        if (wireToPlace)
        {
            Destroy(wireToPlace.gameObject);
            wireToPlace = null;
            wireStartPin = null;
        }

        _currentState = State.None;
    }

    private void FocusLostHandler()
    {
        _currentState = State.None;
    }

    void ConnectionChanged()
    {
        onConnectionChanged?.Invoke();
    }


    public override bool CanReleaseFocus() => _currentState != State.PlacingWire;

    public override void DeleteCommand()
    {
    }
}