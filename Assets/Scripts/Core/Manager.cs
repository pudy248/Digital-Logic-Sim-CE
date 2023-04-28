using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;
using Plugin.VitoBarra.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

public enum ChipEditorMode
{
    Create,
    Update
};

public class Manager : MonoBehaviour
{
    public static ChipEditorMode chipEditorMode;

    public event Action<SpawnableChip> customChipCreated;
    public event Action<SpawnableChip> customChipUpdated;

    public ChipEditor chipEditorPrefab;
    public Wire wirePrefab;
    public Chip[] SpawnableBuiltinChips;
    public List<SpawnableChip> SpawnableCustomChips;
    [FormerlySerializedAs("UIManager")] public MenuManager menuManager;

    public ChipEditor activeChipEditor;
    public static Manager instance;

    void Awake()
    {
        instance = this;
        SaveSystem.Init();
        FolderSystem.Init();
    }

    void Start()
    {
        SpawnableCustomChips = new List<SpawnableChip>();
        activeChipEditor = FindObjectOfType<ChipEditor>();
        SaveSystem.LoadAllChips(this);


    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Y)) return;

        Pin[] unconnectedInputs =
            activeChipEditor.chipInteraction.UnconnectedInputPins;
        Pin[] unconnectedOutputs =
            activeChipEditor.chipInteraction.UnconnectedOutputPins;
        if (unconnectedInputs.Length > 0)
        {
            Debug.Log("Found " + unconnectedInputs.Length.ToString() +
                      " unconnected input pins!");
        }

        if (unconnectedOutputs.Length > 0)
        {
            Debug.Log("Found " + unconnectedOutputs.Length.ToString() +
                      " unconnected output pins!");
        }
    }

    public static ChipEditor ActiveChipEditor => instance.activeChipEditor;

    public Chip GetChipPrefab(Chip chip)
    {
        foreach (Chip prefab in SpawnableBuiltinChips)
            if (chip.chipName == prefab.chipName)
                return prefab;

        return SpawnableCustomChips.FirstOrDefault(prefab => chip.chipName == prefab.chipName);
    }

    public static Chip GetChipByName(string name)
    {
        return instance.SpawnableCustomChips.FirstOrDefault(chip => name == chip.chipName);
    }

    public Chip LoadCustomChip(ChipSaveData loadedChipData)
    {
        if (loadedChipData == null) return null;
        activeChipEditor.LoadFromSaveData(loadedChipData);

        Chip loadedChip = PackageCustomChip();
        if (loadedChip is CustomChip custom)
            custom.ApplyWireModes();

        ClearEditor();
        return loadedChip;
    }

    public void ViewChip(Chip chip)
    {
        ChipSaveData chipSaveData = ChipLoader.GetChipSaveData(chip, wirePrefab, activeChipEditor);
        ClearEditor();
        chipEditorMode = ChipEditorMode.Update;
        menuManager.SetEditorMode(chipEditorMode, chipSaveData.Data.name);
        activeChipEditor.LoadFromSaveData(chipSaveData);
    }

    public void SaveAndPackageChip()
    {
        ChipSaver.Save(activeChipEditor);
        PackageCustomChip();
        ClearEditor();
    }

    public void UpdateChip()
    {
        Chip updatedChip = TryPackageAndReplaceChip(activeChipEditor.Data.name);
        ChipSaver.Update(activeChipEditor, updatedChip);
        chipEditorMode = ChipEditorMode.Create;
        ClearEditor();
    }

    internal void DeleteChip(string nameBeforeChanging)
    {
        SpawnableCustomChips = SpawnableCustomChips.Where(x => !string.Equals(x.chipName, nameBeforeChanging)).ToList();
    }

    internal void RenameChip(string nameBeforeChanging, string nameAfterChanging)
    {
        SpawnableCustomChips.First(x => string.Equals(x.chipName, nameBeforeChanging)).chipName = nameAfterChanging;
    }


    //Generate Package from current editing chip
    SpawnableChip PackageCustomChip()
    {
        var customChip = ChipPackageSpawner.i.GenerateCustomPackageAndChip();

        customChipCreated?.Invoke(customChip);
        SpawnableCustomChips.Add(customChip);
        return customChip;
    }

    SpawnableChip TryPackageAndReplaceChip(string original)
    {
        ChipPackageDisplay oldPackageDisplay = Array.Find(
            GetComponentsInChildren<ChipPackageDisplay>(true), cp => cp.name == original);
        if (oldPackageDisplay != null)
        {
            Destroy(oldPackageDisplay.gameObject);
        }

        var customChip = ChipPackageSpawner.i.GenerateCustomPackageAndChip();

        int index = SpawnableCustomChips.FindIndex(c => c.chipName == original);

        if (index < 0) return customChip;

        SpawnableCustomChips[index] = customChip;
        customChipUpdated?.Invoke(customChip);


        return customChip;
    }


    public void ResetEditor()
    {
        chipEditorMode = ChipEditorMode.Create;
        menuManager.SetEditorMode(chipEditorMode);
        ClearEditor();
    }

    void ClearEditor()
    {
        if (activeChipEditor)
        {
            Destroy(activeChipEditor.gameObject);
            menuManager.SetEditorMode(chipEditorMode, menuManager.ChipName.text);
        }

        activeChipEditor =
            Instantiate(chipEditorPrefab, Vector3.zero, Quaternion.identity);

        activeChipEditor.inputsEditor.CurrentEditor = activeChipEditor;
        activeChipEditor.outputsEditor.CurrentEditor = activeChipEditor;

        Simulation.instance.ResetSimulation();
        ScalingManager.i.SetScale(1);
        ChipEditorOptions.instance.SetUIValues(activeChipEditor);
    }

    public void ChipButtonHanderl(Chip chip)
    {
        if (chip is CustomChip custom)
            custom.ApplyWireModes();

        activeChipEditor.chipInteraction.ChipButtonInteraction(chip);
    }

    public void LoadMainMenu()
    {
        if (chipEditorMode == ChipEditorMode.Update)
        {
            chipEditorMode = ChipEditorMode.Create;
            ClearEditor();
        }
        else
        {
            FolderSystem.Reset();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    public List<string> AllChipNames(bool builtin = true, bool custom = true)
    {
        List<string> allChipNames = new List<string>();

        if (builtin) allChipNames.AddRange(SpawnableBuiltinChips.Select(chip => chip.chipName));
        if (custom) allChipNames.AddRange(SpawnableCustomChips.Select(chip => chip.chipName));

        return allChipNames;
    }

    public Dictionary<string, Chip> AllSpawnableChipDic()
    {
        var allChips = new List<Chip>(SpawnableBuiltinChips);
        allChips.AddRange(SpawnableCustomChips);

        return allChips.ToDictionary(chip => chip.chipName);
    }

    public void ChangeFolderToChip(string ChipName, int index)
    {
        if (SpawnableCustomChips.First(x => string.Equals(x.name, ChipName)) is CustomChip customChip)
            customChip.FolderIndex = index;
        ChipSaver.ChangeFolder(ChipName, index);
    }
}