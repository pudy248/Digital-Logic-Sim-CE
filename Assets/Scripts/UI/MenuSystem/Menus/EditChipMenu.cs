using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using SFB;
using System.Linq;

public class EditChipMenu : MonoBehaviour
{
    public TMP_InputField chipNameField;
    public Button doneButton;
    public Button deleteButton;
    public Button viewButton;
    public Button exportButton;
    public TMP_Dropdown folderDropdown;
    private Chip currentChip;

    private string nameBeforeChanging;


    string CurrentFolderText { get => folderDropdown.options[folderDropdown.value].text; }

    void Awake()
    {
        chipNameField.onValueChanged.AddListener(ChipNameFieldChanged);
        doneButton.onClick.AddListener(FinishCreation);
        deleteButton.onClick.AddListener(SubmitDeleteChip);
        viewButton.onClick.AddListener(ViewChip);
        exportButton.onClick.AddListener(ExportChip);
    }

    public void EditChipInit(string chipName)
    {

        chipNameField.text = chipName;
        nameBeforeChanging = chipName;
        doneButton.interactable = true;
        var IsSafeToDelate = ChipSaver.IsSafeToDelete(nameBeforeChanging);
        chipNameField.interactable = IsSafeToDelate;
        deleteButton.interactable = IsSafeToDelate;

        currentChip = Manager.GetChipByName(chipName);
        viewButton.interactable = true;
        exportButton.interactable = true;

        folderDropdown.ClearOptions();
        var FolderOption = ChipBarUI.instance.FolderDropdown.options;
        folderDropdown.AddOptions(FolderOption.GetRange(1, FolderOption.Count - 2));


        if (currentChip is CustomChip customChip)
        {
            for (int i = 0; i < folderDropdown.options.Count; i++)
            {

                if (FolderSystem.CompareValue(customChip.FolderIndex, folderDropdown.options[i].text))
                {
                    folderDropdown.value = i;
                    break;
                }
            }
        }

    }

    public void ChipNameFieldChanged(string value)
    {
        string formattedName = value.ToUpper();
        doneButton.interactable = IsValidChipName(formattedName.Trim());
        chipNameField.text = formattedName;
    }


    public bool IsValidRename(string chipName)
    {
        // Name has not changed
        if (string.Equals(nameBeforeChanging, chipName))
            return true;
        // Name is either empty or in builtin chips
        if (!IsValidChipName(chipName))
            return false;

        SavedChip[] savedChips = SaveSystem.GetAllSavedChips();
        for (int i = 0; i < savedChips.Length; i++)
        {
            // Name already exists in custom chips
            if (savedChips[i].Data.name == chipName)
                return false;
        }
        return true;
    }

    public bool IsValidChipName(string chipName)
    {
        // If chipName is not in list of builtin chips then is a valid name
        return !Manager.instance.AllChipNames(builtin: true, custom: false)
                    .Contains(chipName) && chipName.Length > 0;
    }

    public void SubmitDeleteChip()
    {
        MenuManager.NewSubmitMenu(header: "Delete Chip",
                                text: $"Are you sure you want to delete the chip '{currentChip.chipName}'? \nIt will be lost forever!",
                                onSubmit: DeleteChip);
    }

    public void DeleteChip()
    {
        ChipSaver.Delete(nameBeforeChanging);
        Manager.instance.DeleteChip(nameBeforeChanging);
        FindObjectOfType<ChipInteraction>().DeleteChip(currentChip);

        ReloadChipBar();


        DLSLogger.Log($"Successfully deleted chip '{currentChip.chipName}'");
        currentChip = null;
    }

    public void ReloadChipBar()
    {
        ChipBarUI.instance.ReloadChipButton();
    }

    public void FinishCreation()
    {
        if (chipNameField.text != nameBeforeChanging)
        {
            // Chip has been renamed
            var NameAfterChanging = chipNameField.text.Trim();
            ChipSaver.Rename(nameBeforeChanging, NameAfterChanging);
            Manager.instance.RenameChip(nameBeforeChanging, NameAfterChanging);

            ReloadChipBar();
        }
        if (currentChip is CustomChip customChip)
        {

            var index = FolderSystem.ReverseIndex(CurrentFolderText);
            if (index != customChip.FolderIndex)
            {
                Manager.instance.ChangeFolderToChip(customChip.name, index);
                ReloadChipBar();
            }
        }
        currentChip = null;
    }

    public void ViewChip()
    {
        if (currentChip == null) return;
        
        Manager.instance.ViewChip(currentChip);
        currentChip = null;
    }

    public void ExportChip() { ImportExport.instance.ExportChip(currentChip); }
}
