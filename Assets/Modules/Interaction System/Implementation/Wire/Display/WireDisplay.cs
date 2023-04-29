using System;
using System.Collections.Generic;
using System.Linq;
using DLS.Simulation;
using UI.ThemeSystem;
using UnityEngine;

public class WireDisplay : MonoBehaviour, IThemeSettable
{
    LineRenderer LineRenderer;
    EdgeCollider2D WireCollider;

    Palette.VoltageColour CurrentTheme;


    const float thicknessMultiplier = 0.1f;
    Material mat;
    bool selected;

    public Color editCol;
    Palette _signalPalette;
    List<Vector2> drawPoints = new List<Vector2>();


    public float curveSize = 0.3f;
    public int resolution = 20;
    public bool Placed;
    float depth;

    bool IsSimulationActive => Simulation.instance.active;


    private void Awake()
    {
        LineRenderer = GetComponent<LineRenderer>();
        WireCollider = GetComponentInParent<EdgeCollider2D>();

        mat = LineRenderer.material;
        mat.color = editCol;

        RegisterEvent();
    }


    private void Start()
    {
        _signalPalette = ThemeManager.Palette;
        CurrentTheme = _signalPalette.GetDefaultTheme();
        CurrentStatusColor = CurrentTheme.Low;


        NormalApparence();
    }

    private void RegisterEvent()
    {
        var wire = GetComponentInParent<Wire>(true);
        wire.OnSelection += () =>
        {
            SelectApparence();

        };
        wire.OnDeSelection += NormalApparence;
        wire.OnWireChange += UpdateSmoothedLine;
        wire.OnPlacing += () =>
        {
            UpdateColor();
            Placed = true;
            NormalApparence();
            wire.startPin.OnStateChange += SetStatusColor;
        };
    }

    private void UpdateColor()
    {
        if (CurrentTheme != null)
            mat.color = CurrentTheme.GetColour(PinState.LOW);
        else
        {
            _signalPalette = ThemeManager.Palette;
            CurrentTheme = _signalPalette.GetDefaultTheme();
            mat.color = CurrentTheme.GetColour(PinState.LOW);
        }
    }


    private Color CurrentStatusColor;

    void SetStatusColor(PinState pinState, Pin.WireType wireType)
    {
        if (!Placed) return;

        CurrentStatusColor = CurrentTheme.GetColour(pinState, wireType);
        mat.color = IsSimulationActive ? CurrentStatusColor : CurrentTheme.Low;
    }


    private void SelectApparence()
    {
        SetUpThickness(ScalingManager.WireSelectedThickness * thicknessMultiplier);
        mat.color = _signalPalette.PinInteractionPalette.WireHighlighte;
    }

    private void NormalApparence()
    {
        SetUpThickness(ScalingManager.WireThickness * thicknessMultiplier);
        mat.color = IsSimulationActive ? CurrentStatusColor : CurrentTheme.Low;
    }

    private void SetUpThickness(float thickness)
    {
        LineRenderer.startWidth = thickness;
        LineRenderer.endWidth = thickness;
    }

    void UpdateCollider()
    {
        WireCollider.points = drawPoints.ToArray();
        WireCollider.edgeRadius =
            ScalingManager.WireThickness * thicknessMultiplier;
    }

    void UpdateSmoothedLine(List<Vector2> anchorPoints)
    {
        GenerateDrawPoints(anchorPoints);
        LineRenderer.positionCount = drawPoints.Count;

        for (int i = 0; i < LineRenderer.positionCount; i++)
        {
            Vector2 localPos = transform.parent.InverseTransformPoint(drawPoints[i]);
            LineRenderer.SetPosition(i, new Vector3(localPos.x, localPos.y, -0.01f));
        }

        float depthOffset = 5;
        transform.localPosition = Vector3.forward * (depth + depthOffset);

        UpdateCollider();
    }


    void GenerateDrawPoints(List<Vector2> anchorPoints)
    {
        drawPoints.Clear();
        drawPoints.Add(anchorPoints[0]);

        for (int i = 1; i < anchorPoints.Count - 1; i++)
        {
            Vector2 StartPoint = anchorPoints[i - 1];
            Vector2 TargetPoint = anchorPoints[i];
            Vector2 NextPoint = anchorPoints[i + 1];

            //calculate Start Curve point
            Vector2 StartToTarget = TargetPoint - StartPoint;
            Vector2 targetDir = StartToTarget.normalized;
            float dstToTarget = StartToTarget.magnitude;

            float dstToCurveStart = Mathf.Max(dstToTarget - curveSize, dstToTarget / 2);

            Vector2 curveStartPoint = StartPoint + targetDir * dstToCurveStart;


            //calulate end Curve point
            Vector2 TargetToNext = NextPoint - TargetPoint;
            Vector2 nextTargetDir = TargetToNext.normalized;
            float dstToNext = TargetToNext.magnitude;

            float dstToCurveEnd = Mathf.Min(curveSize, dstToNext / 2);

            Vector2 curveEndPoint = TargetPoint + nextTargetDir * dstToCurveEnd;

            // Bezier curve
            for (int j = 0; j < resolution; j++)
            {
                float t = j / (resolution - 1f);
                Vector2 a = Vector2.Lerp(curveStartPoint, TargetPoint, t);
                Vector2 b = Vector2.Lerp(TargetPoint, curveEndPoint, t);
                Vector2 p = Vector2.Lerp(a, b, t);

                if ((p - drawPoints[^1]).sqrMagnitude > 0.001f)
                    drawPoints.Add(p);
            }
        }

        drawPoints.Add(anchorPoints[^1]);
    }

    public void SetDepth(int numWires)
    {
        depth = numWires * 0.01f;
        transform.localPosition = Vector3.forward * depth;
    }

    public void SetTheme(Palette.VoltageColour voltageColour)
    {
        CurrentTheme = voltageColour;
        UpdateColor();
    }
}