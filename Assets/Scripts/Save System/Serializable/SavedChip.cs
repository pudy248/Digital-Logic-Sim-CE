﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



[System.Serializable]
// Composite chip is a custom chip made up from other chips ("components")
public class SavedChip
{
    public ChipData Data;

    // Names of all chips used as components in this new chip (each name appears
    // only once)
    public string[] ChipDependecies;
    // Data about all the chips used as components in this chip (positions,
    // connections, etc) Array is ordered: first come input signals, then output
    // signals, then remaining component chips
    public SavedComponentChip[] savedComponentChips;

    public SavedChip(ChipInstanceHolder chipInstanceHolder)
    {
        Data = chipInstanceHolder.Data;

        // Create list of (unique) names of all chips used to make this chip
        ChipDependecies = chipInstanceHolder.componentChips.Select(x => x.chipName)
                                .Distinct()
                                .ToArray();

        // Create serializable chips
        savedComponentChips = new SavedComponentChip[chipInstanceHolder.componentChips.Length];

        for (int i = 0; i < chipInstanceHolder.componentChips.Length; i++)
            savedComponentChips[i] = new SavedComponentChip(chipInstanceHolder, chipInstanceHolder.componentChips[i]);
    }

    public void ValidateDefaultData()
    {

        Data.ValidateDefaultData();
    }


}
