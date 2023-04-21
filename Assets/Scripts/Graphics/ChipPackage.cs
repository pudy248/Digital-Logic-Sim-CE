﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipPackage : MonoBehaviour
{
    public enum ChipType
    {
        Combapibility,
        Gate,
        Miscellaneous,
        Custom
    };

    public ChipType chipType;
    public TMPro.TextMeshPro nameText;
    public Transform container;
    public Pin chipPinPrefab;
    public bool override_width_and_height = false;
    public float override_width = 1f;
    public float override_height = 1f;

    const string pinHolderName = "Pin Holder";

    void Awake()
    {
        BuiltinChip builtinChip = GetComponent<BuiltinChip>();
        if (builtinChip != null)
        {
            SetSizeAndSpacing(GetComponent<Chip>());
            SetColour(builtinChip.packageColour);
        }

        nameText.fontSize = ScalingManager.packageFontSize;
    }

    public void PackageCustomChip(ChipEditor chipEditor)
    {
        var chipName = chipEditor.Data.name;
        gameObject.name = chipName;
        nameText.text = chipName;
        nameText.color = chipEditor.Data.NameColour;
        SetColour(chipEditor.Data.Colour);

        // Add and set up the custom chip component
        CustomChip chip = gameObject.AddComponent<CustomChip>();
        chip.chipName = chipName;
        chip.FolderIndex = chipEditor.Data.FolderIndex;
        chipType = ChipType.Custom;

        List<T> GetAllSignals<T>(ChipInterfaceEditor InterfaceEditor) where T : ChipSignal
        {
            var result = new List<T>();
            foreach (var signal in InterfaceEditor.GetAllSignals())
            {
                if (signal is T ele)
                    result.Add(ele);
            }
            return result;
        }

        // Set input signals
        chip.inputSignals = GetAllSignals<InputSignal>(chipEditor.inputsEditor).ToArray();
        // Set output signals
        chip.outputSignals = GetAllSignals<OutputSignal>(chipEditor.outputsEditor).ToArray();


        // Create pins and set set package size
        SpawnPins(chip);
        SetSizeAndSpacing(chip);

        // Parent chip holder to the template, and hide
        Transform implementationHolder = chipEditor.chipImplementationHolder;

        implementationHolder.parent = transform;
        //implementationHolder.localPosition = Vector3.zero;
        implementationHolder.gameObject.SetActive(false);
    }

    public void SpawnPins(CustomChip chip)
    {
        Transform pinHolder = new GameObject(pinHolderName).transform;
        pinHolder.parent = transform;
        pinHolder.localPosition = Vector3.zero;

        chip.inputPins = new Pin[chip.inputSignals.Length];
        chip.outputPins = new Pin[chip.outputSignals.Length];

        for (int i = 0; i < chip.inputPins.Length; i++)
        {
            Pin inputPin = Instantiate(chipPinPrefab, pinHolder.position,
                Quaternion.identity, pinHolder);
            inputPin.pinType = Pin.PinType.ChipInput;
            inputPin.chip = chip;
            inputPin.pinName = chip.inputSignals[i].outputPins[0].pinName;
            chip.inputPins[i] = inputPin;
            inputPin.SetScale();
        }

        for (int i = 0; i < chip.outputPins.Length; i++)
        {
            Pin outputPin = Instantiate(chipPinPrefab, pinHolder.position,
                Quaternion.identity, pinHolder);
            outputPin.pinType = Pin.PinType.ChipOutput;
            outputPin.chip = chip;
            outputPin.pinName = chip.outputSignals[i].inputPins[0].pinName;
            chip.outputPins[i] = outputPin;
            outputPin.SetScale();
        }
    }

    public void SetSizeAndSpacing(Chip chip)
    {
        nameText.fontSize = ScalingManager.packageFontSize;

        float containerHeightPadding = 0;
        float containerWidthPadding = 0.1f;
        float pinSpacePadding = Pin.radius * 0.2f;
        float containerWidth = nameText.preferredWidth +
                               Pin.interactionRadius * 2f + containerWidthPadding;

        int numPins = Mathf.Max(chip.inputPins.Length, chip.outputPins.Length);
        float unpaddedContainerHeight =
            numPins * (Pin.radius * 2 + pinSpacePadding);
        float containerHeight =
            Mathf.Max(unpaddedContainerHeight, nameText.preferredHeight + 0.05f) +
            containerHeightPadding;
        float topPinY = unpaddedContainerHeight / 2 - Pin.radius;
        float bottomPinY = -unpaddedContainerHeight / 2 + Pin.radius;
        const float z = -0.05f;

        // Input pins
        int numInputPinsToAutoPlace = chip.inputPins.Length;
        for (int i = 0; i < numInputPinsToAutoPlace; i++)
        {
            float percent = 0.5f;
            if (chip.inputPins.Length > 1)
            {
                percent = i / (numInputPinsToAutoPlace - 1f);
            }

            if (override_width_and_height)
            {
                float posX = -override_width / 2f;
                float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
                chip.inputPins[i].transform.localPosition = new Vector3(posX, posY, z);
            }
            else
            {
                float posX = -containerWidth / 2f;
                float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
                chip.inputPins[i].transform.localPosition = new Vector3(posX, posY, z);
            }
        }

        // Output pins
        for (int i = 0; i < chip.outputPins.Length; i++)
        {
            float percent = 0.5f;
            if (chip.outputPins.Length > 1)
            {
                percent = i / (chip.outputPins.Length - 1f);
            }

            float posX = containerWidth / 2f;
            float posY = Mathf.Lerp(topPinY, bottomPinY, percent);
            chip.outputPins[i].transform.localPosition = new Vector3(posX, posY, z);
        }

        // Set container size
        if (override_width_and_height)
        {
            container.transform.localScale =
                new Vector3(override_width, override_height, 1);
            GetComponent<BoxCollider2D>().size =
                new Vector2(override_width, override_height);
        }
        else
        {
            container.transform.localScale =
                new Vector3(containerWidth, containerHeight, 1);
            GetComponent<BoxCollider2D>().size =
                new Vector2(containerWidth, containerHeight);
        }
    }

    void SetColour(Color colour)
    {
        container.GetComponent<MeshRenderer>().material.color = colour;
    }
}