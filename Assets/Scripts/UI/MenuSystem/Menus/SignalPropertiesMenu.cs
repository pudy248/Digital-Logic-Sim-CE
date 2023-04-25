using System;
using System.Collections;
using System.Collections.Generic;
using Interaction;
using UnityEngine;

public class SignalPropertiesMenu : MonoBehaviour
{
    [SerializeField]
    private RectTransform propertiesUI;
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
        //GetComponentInChildren<Canvas>().worldCamera = Camera.main;
    }
    // Start is called before the first frame update
    void Start()
    {
        deleteButton.onClick.AddListener(Delete);
        modeDropdown.onValueChanged.AddListener(OnValueDropDownChange);
        MenuManager.instance.RegisterFinalizer(MenuType.SignalPropertiesMenu,OnUIClose);
    }

    public void SetActive(bool b)
    {
        propertiesUI.gameObject.SetActive(b);
    }


    public void EnableUI(SignalInteraction signalInteraction, string signalName, bool isGroup, bool useTwosComplement, int wireType)
    {
        SetActive(true);
        nameField.text = signalName;
        nameField.Select();
        nameField.caretPosition = nameField.text.Length;
        twosComplementToggle.gameObject.SetActive(isGroup);
        twosComplementToggle.isOn = useTwosComplement;
        modeDropdown.gameObject.SetActive(!isGroup);
        modeDropdown.SetValueWithoutNotify(wireType);
        SignalInteraction = signalInteraction;

        var SizeDelta = new Vector2(propertiesUI.sizeDelta.x, (isGroup) ? propertiesHeightMinMax.y : propertiesHeightMinMax.x);
        propertiesUI.sizeDelta = SizeDelta;

    }

    public void OnUIClose()
    {
        SaveProperty();
        ResetC();
    }

    private void ResetC()
    {
        nameField.text = "";
        SignalInteraction = null;
    }

    public void SetPosition(Vector3 centre, ChipInterfaceEditor.EditorInterfaceType editorInterfaceType)
    {
        float propertiesUIX = ScalingManager.propertiesUIX * (editorInterfaceType == ChipInterfaceEditor.EditorInterfaceType.Input ? 1 : -1);
        propertiesUI.transform.position = new Vector3(centre.x + propertiesUIX, centre.y, propertiesUI.transform.position.z);
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
    void OnValueDropDownChange(int mode)
    {
        if (SignalInteraction != null)
            SignalInteraction.ChangeWireType(mode);
    }
}
