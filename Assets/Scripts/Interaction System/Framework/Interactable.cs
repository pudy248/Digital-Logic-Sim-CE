using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for systems that handle user input:

// Only one system can have focus at a time. If a system wants focus, it must
// call RequestFocus(). The system that currently has focus will determine if it
// CanReleaseFocus(). If yes: • FocusLost() will be called on that system. •
// Requesting system will have HasFocus set to true.

public abstract class Interactable : MonoBehaviour
{
    public event Action OnFocusObtained;
    public event Action OnFocusLost;

    // Does this system currently have focus?
    public bool HasFocus { get; private set; } = false;

    public abstract void OrderedUpdate();
    public abstract void DeleteCommand();

    
    public virtual bool CanReleaseFocus() => true;


    public bool RequestFocus()
    {
        HasFocus = InteractionManager.Instance.RequestFocus(this);
        if (HasFocus)
            OnFocusObtained?.Invoke();
        return HasFocus;
    }

    public void ReleaseFocusNotHandled()
    {
        InteractionManager.Instance.ReleaseFocus(this);
        HasFocus = false;
    }

    public void ReleaseFocus()
    {
        ReleaseFocusNotHandled();
        OnFocusLost?.Invoke();
    }
}