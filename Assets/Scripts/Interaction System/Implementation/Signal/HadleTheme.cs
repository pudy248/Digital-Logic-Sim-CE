using UnityEditor;
using UnityEngine;

public class HadleTheme : MonoBehaviour
{
    public PinInteractionPalette PinPalette;

    //Handler
    public Material HandleMat;
    public Material HighlightedHandleMat;
    public Material SelectedHandleMat;
    public Material SelectedAndhighlightedHandle;


    public void Start()
    {
        PinPalette = UIThemeManager.Palette.PinInteractionPalette;

        if (HandleMat == null)
            HandleMat = MaterialUtility.CreateUnlitMaterial(PinPalette.handleCol);
        
        if (HighlightedHandleMat == null)
            HighlightedHandleMat =  MaterialUtility.CreateUnlitMaterial(PinPalette.highlightedHandleCol);
        
        if (SelectedHandleMat == null)
            SelectedHandleMat =  MaterialUtility.CreateUnlitMaterial(PinPalette.selectedHandleCol);
        
        if (SelectedAndhighlightedHandle == null)
            SelectedAndhighlightedHandle = MaterialUtility.CreateUnlitMaterial(PinPalette.selectedAndFocusedHandleCol);
    }
}