using System;
using System.Collections;
using System.Collections.Generic;
using Interaction;
using Interaction.Signal;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class SignalPropertiesMenu : MonoBehaviour
{
    [SerializeField] private RectTransform propertiesUI;
    public Vector2 propertiesHeightMinMax;

    //fuctionality
    public TMPro.TMP_InputField nameField;
    public UnityEngine.UI.Button deleteButton;
    public UnityEngine.UI.Toggle twosComplementToggle;
    public TMPro.TMP_Dropdown modeDropdown;
    public TMPro.TMP_InputField BusValueField;

    SignalInteraction SignalInteraction;


    private void Awake()
    {
        propertiesUI = (RectTransform)transform.GetChild(0);
    }

    // Start is called before the first frame update
    void Start()
    {
        deleteButton.onClick.AddListener(Delete);
        modeDropdown.onValueChanged.AddListener(OnValueDropDownChange);
        MenuManager.instance.RegisterFinalizer(MenuType.SignalPropertiesMenu, OnCloseUI);
        BusValueField.onSelect.AddListener((_) => DisableDeleteCommand());
        nameField.onSelect.AddListener((_) => DisableDeleteCommand());
        BusValueField.onDeselect.AddListener((_) => EnableDeleteCommand());
        nameField.onDeselect.AddListener((_) => EnableDeleteCommand());
        BusValueField.characterValidation = TMP_InputField.CharacterValidation.Integer;
        BusValueField.onValueChanged.AddListener(OnMyInputValueChanged);
    }


    private void OnMyInputValueChanged(string newValue)
    {
        if (SignalInteraction.WireType == Pin.WireType.Simple) return;

        var maxValue = Math.Pow(2, SignalInteraction.Signals[0].ChipSignal.State.Count) - 1;

        if (!int.TryParse(newValue, out var intValue))
            BusValueField.text = 0.ToString();
        else if (intValue > maxValue)
            BusValueField.text = maxValue.ToString();
    }


    public void SetActive(bool b)
    {
        propertiesUI.gameObject.SetActive(b);
    }


    public void SetUpUI(SignalInteraction signalInteraction)
    {
        SetActive(true);
        SignalInteraction = signalInteraction;

        nameField.text = SignalInteraction.SignalName;
        nameField.caretPosition = nameField.text.Length;

        twosComplementToggle.gameObject.SetActive(signalInteraction.IsGroup);
        twosComplementToggle.isOn = signalInteraction.UseTwosComplement;
        modeDropdown.gameObject.SetActive(!signalInteraction.IsGroup);
        modeDropdown.SetValueWithoutNotify((int)signalInteraction.WireType);

        var SizeDelta = new Vector2(propertiesUI.sizeDelta.x,
            (signalInteraction.IsGroup) ? propertiesHeightMinMax.y : propertiesHeightMinMax.x);
        propertiesUI.sizeDelta = SizeDelta;

        ToggleBusValueActivation(signalInteraction.WireType, signalInteraction.EditorInterfaceType);

        SetPosition(signalInteraction.GroupCenter, signalInteraction.EditorInterfaceType);


        MenuManager.instance.OpenMenu(MenuType.SignalPropertiesMenu);
    }

    private void ToggleBusValueActivation(Pin.WireType wireType, EditorInterfaceType InterfaceType)
    {
        if (wireType == Pin.WireType.Simple || InterfaceType == EditorInterfaceType.Output)
            BusValueField.gameObject.SetActive(false);
        else
        {
            BusValueField.gameObject.SetActive(true);
            BusValueField.text = SignalInteraction.Signals[0].ChipSignal.State.ToUInt().ToString();
        }
    }

    private void OnCloseUI()
    {
        SaveProperty();
        SignalInteraction = null;
    }

    private void SetPosition(Vector3 centre, EditorInterfaceType editorInterfaceType)
    {
        float propertiesUIX =ScalingManager.PropertiesUIX * (editorInterfaceType == EditorInterfaceType.Input ? 1 : -1);
        propertiesUI.transform.position = new Vector3(centre.x + propertiesUIX, centre.y, propertiesUI.transform.position.z);
    }


    void SaveProperty()
    {
        if (SignalInteraction == null) return;

        SignalInteraction.UpdateGroupProperty(nameField.text, twosComplementToggle.isOn);
        SignalInteraction.SetBusValue(int.Parse(BusValueField.text));
    }

    void Delete()
    {
        SignalInteraction.DeleteCommand();
    }

    void DeleteFinalizer()
    {
        UnregisterSignalGroup(SignalInteraction);
        MenuManager.instance.CloseMenu();
    }

    void OnValueDropDownChange(int mode)
    {
        var wireType = (Pin.WireType)mode;
        if (SignalInteraction != null)
            SignalInteraction.ChangeWireType(wireType);
        ToggleBusValueActivation(wireType, SignalInteraction.EditorInterfaceType);
    }

    public void RegisterSignalGroup(SignalInteraction signalInteraction)
    {
        signalInteraction.OnDragig += SetPosition;
        signalInteraction.OnDeleteInteraction += DeleteFinalizer;
    }

    private void UnregisterSignalGroup(SignalInteraction signalInteraction)
    {
        signalInteraction.OnDragig -= SetPosition;
        signalInteraction.OnDeleteInteraction -= DeleteFinalizer;
    }

    private void DisableDeleteCommand()
    {
        if (SignalInteraction == null) return;
        SignalInteraction.SilenceDeleteCommand();
    }

    private void EnableDeleteCommand()
    {
        if (SignalInteraction == null) return;
        SignalInteraction.EnableDeleteCommand();
    }
}