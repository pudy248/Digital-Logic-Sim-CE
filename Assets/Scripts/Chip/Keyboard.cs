using System.Collections.Generic;
using UnityEngine;
using System;
using DLS.Simulation;

public class Keyboard : BuiltinChip
{

    public List<string> chars = new List<string>();
    void Update()
    {
        print(Input.anyKey);
        if (Input.anyKey)
        {
            if (Input.inputString?.ToCharArray()?.Length > 0)
            {
                chars.Clear();
                char tmp = Input.inputString.ToCharArray()[0];
                int temp = (int)tmp;

                string binary = Convert.ToString(temp, 2);

                if (binary.Length < 8)
                {
                    for (int i = 8 - binary.Length; i > 0; i--)
                    {
                        binary = "0" + binary;
                    }
                }

                for (var i = 0; i < 8; i++)
                {
                    chars.Add(Convert.ToString(binary[i]));
                }

                for (var i = 0; i < chars.Count; i++)
                {
                    PinState outputSignal = (PinState)uint.Parse(chars[i]);
                    outputPins[i].ReceiveSignal(outputSignal);
                }
            }
        }

        else
        {
            for (var i = 0; i < 8; i++)
            {
                PinState outputSignal = 0;
                outputPins[i].ReceiveSignal(outputSignal);
            }
        }
    }
}