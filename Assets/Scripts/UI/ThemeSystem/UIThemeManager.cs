using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    
}
