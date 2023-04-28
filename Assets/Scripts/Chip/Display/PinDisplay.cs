using System;
using System.Collections;
using System.Collections.Generic;
using DLS.Simulation;
using UnityEngine;

public class PinDisplay : MonoBehaviour
{
    private Renderer renderer;

    // Appearance
    Color defaultCol = Color.black;
    private Palette _Palette;

    bool IsSimActive => Simulation.instance.active;
    public static float radius => ScalingManager.PinSize;

    public static float IteractionFactor => 1.1f;
    
    public static float interactionRadius => radius * IteractionFactor;
    
    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        renderer.material.color = defaultCol;
        var Pin = GetComponentInParent<Pin>();
        // Pin.OnStateChange += UpdateColor;
        Pin.OnInteraction += InteractionHadler;
        ScalingManager.OnScaleChange += UpdateScale;
    }

    private void Start()
    {
        _Palette = UIThemeManager.Palette;
        UpdateScale();
    }


    public void UpdateScale() { transform.localScale = Vector3.one * (radius * 2); }
    
    // private void UpdateColor(PinState state, Pin.WireType wireType)
    // {
    //     if (renderer == null) return;
    //     var material = renderer.material;
    //     if (!material) return;
    //
    //     Color newColor;
    //
    //     if (IsSimActive && state == PinState.HIGH)
    //     {
    //         newColor = _Palette.GetDefaultTheme().GetHigh(wireType);
    //     }
    //     else
    //     {
    //         newColor = defaultCol;
    //     }
    //
    //     SetColor(newColor);
    // }


    private void InteractionHadler(bool interaction)
    {
        var InteractionPalette = _Palette.PinInteractionPalette;
        SetColor(interaction
            ? InteractionPalette.PinHighlighte
            : InteractionPalette.Pindefault);

        if (interaction)
        {
            transform.localScale = Vector3.one * (interactionRadius * 2);
        }
        else
        {
            transform.localScale = Vector3.one * (radius * 2);
        }
    }

    private void SetColor(Color newColor)
    {
        var material = renderer.material;
        if (material.color != newColor)
        {
            material.color = newColor;
        }
    }

    private void OnDestroy()
    {
        ScalingManager.OnScaleChange -= UpdateScale;
    }
}