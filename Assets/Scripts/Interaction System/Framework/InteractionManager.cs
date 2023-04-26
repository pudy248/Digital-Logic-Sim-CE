using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    private Interactable InteractableWhitFocus;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (InteractableWhitFocus == null) return;
        if (InputHelper.AnyOfTheseKeysDown(KeyCode.Backspace, KeyCode.Delete) || Input.GetMouseButton(2))
            InteractableWhitFocus.DeleteCommand();
    }

    public bool HadFocus(Interactable interactable) => InteractableWhitFocus == interactable;

    public void ReleaseFocus(Interactable interactable)
    {
        if (!HadFocus(interactable)) return;
        InteractableWhitFocus = null;
    }

    public bool RequestFocus(Interactable interactable)
    {
        if (HadFocus(interactable)) return true;
        if (InteractableWhitFocus == null)
        {
            SetInteragibleWhitFocus(interactable);
            return true;
        }

        if (!InteractableWhitFocus.CanReleaseFocus()) return false;
        
        
        InteractableWhitFocus.ReleaseFocus();
        SetInteragibleWhitFocus(interactable);
        return true;

    }

    private void SetInteragibleWhitFocus(Interactable interactable)
    {
        InteractableWhitFocus = interactable;
    }
}