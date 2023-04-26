using System.Collections;
using System.Collections.Generic;
using DLS.Simulation;
using UnityEngine;
using TMPro;
public class Clock : BuiltinChip
{
    private WaitForSeconds Waiter;
    [SerializeField]
    private float _hz = 1f;
    public float Hz
    {
        get => _hz;
        set
        {
            _hz = value;
            HzThext.text = $"{_hz}Hz";
            Waiter = new WaitForSeconds((1 / Hz) / 2);
            StopAllCoroutines();
            StartCoroutine(ClockTick());
        }
    }

    [SerializeField]
    private TMP_Text HzThext;
    [SerializeField]
    private GameObject HzEditor;
    protected override void Start()
    {
        base.Start();
        HzThext.text = $"{_hz}Hz";
        StartCoroutine(ClockTick());
        Waiter = new WaitForSeconds((1 / Hz) / 2);
    }
    protected override void ProcessOutput()
    {

    }
    private IEnumerator ClockTick()
    {
        yield return Waiter;
        outputPins[0].ReceiveSignal(PinState.HIGH);
        yield return Waiter;
        outputPins[0].ReceiveSignal(PinState.LOW);
        StartCoroutine(ClockTick());
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
            MenuManager.instance.OpenMenu(MenuType.ClockMenu);
    }
}
