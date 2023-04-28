﻿using System;
using System.Collections;
using System.Collections.Generic;
using DLS.Simulation;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class DecimalDisplay : MonoBehaviour
{
    
    private TMP_Text text;


    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    public void UpdateDecimalDisplay(IList<ChipSignal> signals ,bool useTwosComplement)
    {
        int decimalValue = 0;
        for (int i = 0; i < signals.Count; i++)
        {
            var signalState = signals[signals.Count - 1 - i].currentState;
            if (useTwosComplement && i == signals.Count - 1)
                decimalValue |= (-((int)signalState << i));
            else
                decimalValue |= (int)(signalState.ToUint() << i);
        }

        text.text = decimalValue + "";
    }
}
