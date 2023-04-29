using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager instance;
    public static Palette Palette => instance.palette;
    public Palette palette;
    
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.I))
            palette.DefaultIndex++;

        if (InputHelper.AnyOfTheseKeysDown(KeyCode.O))
            palette.DefaultIndex--;
    }
}
