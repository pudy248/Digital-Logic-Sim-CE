using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIThemeManager : MonoBehaviour
{
    public static UIThemeManager instance;
    public static HadleTheme HadleTheme;
    public static Palette Palette => instance.palette;
    public Palette palette;
    
    private void Awake()
    {
        instance = this;
        HadleTheme = GetComponent<HadleTheme>();
    }

    private void Update()
    {
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.I))
            palette.DefaultIndex++;

        if (InputHelper.AnyOfTheseKeysDown(KeyCode.O))
            palette.DefaultIndex--;
    }
}
