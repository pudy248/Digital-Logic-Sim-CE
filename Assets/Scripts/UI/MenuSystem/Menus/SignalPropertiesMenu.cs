using System;
using System.Collections;
using System.Collections.Generic;
using Interaction;
using UnityEngine;

public class SignalPropertiesMenu : MonoBehaviour
{
    [SerializeField] private RectTransform propertiesUI;
    public Vector2 propertiesHeightMinMax;

    //fuctionality
    public TMPro.TMP_InputField nameField;
    public UnityEngine.UI.Button deleteButton;
    public UnityEngine.UI.Toggle twosComplementToggle;
    public TMPro.TMP_Dropdown modeDropdown;

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
        nameField.onSelect.AddListener((_) => DisableDeleteCommand());
        nameField.onDeselect.AddListener((_) => EnableDeleteCommand());
    }

    public void SetActive(bool b)
    {
        propertiesUI.gameObject.SetActive(b);
    }


    public void SetUpUI(SignalInteraction signalInteraction)
    {
        SetActive(true);
        
        nameField.text = signalInteraction.SignalName;
        nameField.caretPosition = nameField.text.Length;

        twosComplementToggle.gameObject.SetActive(signalInteraction.IsGroup);
        twosComplementToggle.isOn = signalInteraction.UseTwosComplement;
        modeDropdown.gameObject.SetActive(!signalInteraction.IsGroup);
        modeDropdown.SetValueWithoutNotify((int)signalInteraction.WireType);
        SignalInteraction = signalInteraction;

        var SizeDelta = new Vector2(propertiesUI.sizeDelta.x,
            (signalInteraction.IsGroup) ? propertiesHeightMinMax.y : propertiesHeightMinMax.x);
        propertiesUI.sizeDelta = SizeDelta;
        

        SetPosition(signalInteraction.GroupCenter, signalInteraction.EditorInterfaceType);
    }

    private void OnCloseUI()
    {
        SaveProperty();
        // nameField.text = "";
        SignalInteraction = null;
    }

    private void SetPosition(Vector3 centre, ChipInterfaceEditor.EditorInterfaceType editorInterfaceType)
    {
        float propertiesUIX = ScalingManager.PropertiesUIX *
                              (editorInterfaceType == ChipInterfaceEditor.EditorInterfaceType.Input ? 1 : -1);
        propertiesUI.transform.position =
            new Vector3(centre.x + propertiesUIX, centre.y, propertiesUI.transform.position.z);
    }


    void SaveProperty()
    {
        if (SignalInteraction != null)
            SignalInteraction.UpdateGroupProperty(nameField.text, twosComplementToggle.isOn);
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
        if (SignalInteraction != null)
            SignalInteraction.ChangeWireType(mode);
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