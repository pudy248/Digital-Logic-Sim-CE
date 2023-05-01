using System;
using System.Collections;
using System.Collections.Generic;
using Interaction;
using UnityEngine;

public class HandleEvent : MonoBehaviour
{
    public event Action OnHandleEnter;
    public event Action OnHandleExit;
    public event Action OnHandleClick;
    public event Action<Vector3> OnStartDrag;
    public event Action OnDrag;
    public event Action OnStopDrag;

    private bool isDragging = false;
    private float clickTime = 0f;
    private float clickThreshold = 0.1f;
    

    private void OnMouseEnter()
    {
        OnHandleEnter?.Invoke();
    }

    private void OnMouseExit()
    {
        OnHandleExit?.Invoke();
    }

    private void OnMouseDown()
    {
        clickTime = Time.time;
        OnHandleClick?.Invoke();
    }

    private void OnMouseUp()
    {
        if (!isDragging)
        {
            OnStopDrag?.Invoke();
        }

        isDragging = false;
    }

    private void OnMouseDrag()
    {
        if (!isDragging && Time.time - clickTime > clickThreshold)
        {
            OnStartDrag?.Invoke(transform.position);
            isDragging = true;
        }

        if (isDragging)
        {
            OnDrag?.Invoke();
        }
    }
}