using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class HadleTheme : MonoBehaviour
{
    public PinInteractionPalette PinPalette;

    //Handler
    public Material HandleMat;
    public Material HighlightedHandleMat;
    public Material FocusedHandle;


    public void Start()
    {
        PinPalette = UIThemeManager.Palette.PinInteractionPalette;

        if (HandleMat == null)
            HandleMat = MaterialUtility.CreateUnlitMaterial(PinPalette.handleCol);
        
        if (HighlightedHandleMat == null)
            HighlightedHandleMat =  MaterialUtility.CreateUnlitMaterial(PinPalette.highlightedHandleCol);
        
        if (FocusedHandle == null)
            FocusedHandle = MaterialUtility.CreateUnlitMaterial(PinPalette.FocusedHandleCol);
    }
}