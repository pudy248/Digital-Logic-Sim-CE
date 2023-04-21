﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{

    public Material simpleMat;

    [HideInInspector]
    public LineRenderer lineRenderer;
    public Color editCol;
    PinPalette _pinPalette;
    public Color placedCol;
    public float curveSize = 0.5f;
    public int resolution = 10;
    bool selected;

    bool wireConnected;
    // [HideInInspector]
    public Pin startPin;
    // [HideInInspector]
    public Pin endPin;

    public bool simActive = false;
    EdgeCollider2D wireCollider;
    public List<Vector2> anchorPoints { get; private set; }
    List<Vector2> drawPoints;
    const float thicknessMultiplier = 0.1f;
    float length;
    Material mat;
    float depth;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth =
            ScalingManager.wireSelectedThickness * thicknessMultiplier;
        lineRenderer.endWidth =
            ScalingManager.wireSelectedThickness * thicknessMultiplier;
    }

    void Start()
    {
        _pinPalette = UIManager.Palette.pinPalette;
        lineRenderer.material = simpleMat;
        mat = lineRenderer.material;
    }

    public Pin ChipInputPin => (startPin.pinType == Pin.PinType.ChipInput) ? startPin : endPin;

    public Pin ChipOutputPin => (startPin.pinType == Pin.PinType.ChipOutput) ? startPin : endPin;

    public void tellWireSimIsOff()
    {
        simActive = false;
        startPin.TellPinSimIsOff();
        endPin.TellPinSimIsOff();
    }

    public void tellWireSimIsOn()
    {
        simActive = true;
        startPin.tellPinSimIsOn();
        endPin.tellPinSimIsOn();
    }

    public void SetAnchorPoints(Vector2[] newAnchorPoints)
    {
        anchorPoints = new List<Vector2>(newAnchorPoints);
        UpdateSmoothedLine();
        UpdateCollider();
    }

    public void SetDepth(int numWires)
    {
        depth = numWires * 0.01f;
        transform.localPosition = Vector3.forward * depth;
    }

    void LateUpdate()
    {
        SetWireCol();
        if (wireConnected)
        {
            float depthOffset = 5;

            transform.localPosition = Vector3.forward * (depth + depthOffset);
            UpdateWirePos();
            // transform.position = new Vector3 (transform.position.x,
            // transform.position.y, inputPin.sequentialState * -0.01f);
        }
        lineRenderer.startWidth = ((selected) ? ScalingManager.wireSelectedThickness
                                              : ScalingManager.wireThickness) *
                                  thicknessMultiplier;
        lineRenderer.endWidth = ((selected) ? ScalingManager.wireSelectedThickness
                                            : ScalingManager.wireThickness) *
                                thicknessMultiplier;
    }

    void UpdateWirePos()
    {
        const float maxSqrError = 0.00001f;
        // How far are start and end points from the pins they're connected to (chip
        // has been moved)
        Vector2 startPointError =
            (Vector2)startPin.transform.position - anchorPoints[0];
        Vector2 endPointError = (Vector2)endPin.transform.position -
                                anchorPoints[^1];

        if (startPointError.sqrMagnitude > maxSqrError ||
            endPointError.sqrMagnitude > maxSqrError)
        {
            // If start and end points are both same offset from where they should be,
            // can move all anchor points (entire wire)
            if ((startPointError - endPointError).sqrMagnitude < maxSqrError &&
                startPointError.sqrMagnitude > maxSqrError)
            {
                for (int i = 0; i < anchorPoints.Count; i++)
                {
                    anchorPoints[i] += startPointError;
                }
            }

            anchorPoints[0] = startPin.transform.position;
            anchorPoints[^1] = endPin.transform.position;
            UpdateSmoothedLine();
            UpdateCollider();
        }
    }

    void SetWireCol()
    {
        if (wireConnected)
        {
            Color onCol = _pinPalette.onCol;
            Color offCol = _pinPalette.offCol;
            Color selectedCol = _pinPalette.selectedColor;

            if (selected)
            {
                mat.color = selectedCol;
            }
            else
            {

                // High Z
                if (ChipOutputPin.State == Bus.HighZ)
                {
                    onCol = _pinPalette.highZCol;
                    offCol = _pinPalette.highZCol;
                }
                if (simActive)
                {
                    if (startPin.wireType != Pin.WireType.Simple)
                    {
                        mat.color = (ChipOutputPin.State == 0) ? offCol : _pinPalette.busColor;
                    }
                    else
                    {
                        mat.color = (ChipOutputPin.State == 0) ? offCol : onCol;
                    }
                }
                else
                {
                    mat.color = offCol;
                }
            }
        }
        else
        {
            mat.color = Color.black;
        }
    }

    public void Connect(Pin inputPin, Pin outputPin)
    {
        ConnectToFirstPin(inputPin);
        Place(outputPin);
    }

    public void ConnectToFirstPin(Pin startPin)
    {
        this.startPin = startPin;
        lineRenderer = GetComponent<LineRenderer>();
        mat = simpleMat;
        drawPoints = new List<Vector2>();

        transform.localPosition = new Vector3(0, 0, transform.localPosition.z);

        wireCollider = GetComponent<EdgeCollider2D>();

        anchorPoints = new List<Vector2>();
        anchorPoints.Add(startPin.transform.position);
        anchorPoints.Add(startPin.transform.position);
        UpdateSmoothedLine();
        mat.color = editCol;
    }

    public void ConnectToFirstPinViaWire(Pin startPin, Wire parentWire, Vector2 inputPoint)
    {
        lineRenderer = GetComponent<LineRenderer>();
        mat = simpleMat;
        drawPoints = new List<Vector2>();
        this.startPin = startPin;
        transform.localPosition = new Vector3(0, 0, transform.localPosition.z);

        wireCollider = GetComponent<EdgeCollider2D>();

        anchorPoints = new List<Vector2>();

        // Find point on wire nearest to input point
        Vector2 closestPoint = Vector2.zero;
        float smallestDst = float.MaxValue;
        int closestI = 0;
        for (int i = 0; i < parentWire.anchorPoints.Count - 1; i++)
        {
            var a = parentWire.anchorPoints[i];
            var b = parentWire.anchorPoints[i + 1];
            var pointOnWire = MathUtility.ClosestPointOnLineSegment(a, b, inputPoint);
            float sqrDst = (pointOnWire - inputPoint).sqrMagnitude;
            if (sqrDst < smallestDst)
            {
                smallestDst = sqrDst;
                closestPoint = pointOnWire;
                closestI = i;
            }
        }

        for (int i = 0; i <= closestI; i++)
        {
            anchorPoints.Add(parentWire.anchorPoints[i]);
        }
        anchorPoints.Add(closestPoint);
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            anchorPoints.Add(closestPoint);
        }
        anchorPoints.Add(inputPoint);

        UpdateSmoothedLine();
        mat.color = editCol;
    }

    // Connect the input pin to the output pin
    public void Place(Pin endPin)
    {

        this.endPin = endPin;
        anchorPoints[anchorPoints.Count - 1] = endPin.transform.position;
        UpdateSmoothedLine();

        wireConnected = true;
        UpdateCollider();

        if (endPin.pinType == Pin.PinType.ChipOutput)
            SwapStartEndPoints();

        if (Simulation.instance.active)
            tellWireSimIsOn();
    }

    void SwapStartEndPoints()
    {
        (startPin, endPin) = (endPin, startPin);

        anchorPoints.Reverse();
        drawPoints.Reverse();

        UpdateSmoothedLine();
        UpdateCollider();
    }

    // Update position of wire end point (for when initially placing the wire)
    public void UpdateWireEndPoint(Vector2 endPointWorldSpace)
    {
        anchorPoints[^1] = ProcessPoint(endPointWorldSpace);
        UpdateSmoothedLine();
    }

    // Add anchor point (for when initially placing the wire)
    public void AddAnchorPoint(Vector2 pointWorldSpace)
    {
        anchorPoints[^1] = ProcessPoint(pointWorldSpace);
        anchorPoints.Add(ProcessPoint(pointWorldSpace));
    }

    void UpdateCollider()
    {
        wireCollider.points = drawPoints.ToArray();
        wireCollider.edgeRadius =
            ScalingManager.wireThickness * thicknessMultiplier;
    }

    void UpdateSmoothedLine()
    {
        length = 0;
        GenerateDrawPoints();

        lineRenderer.positionCount = drawPoints.Count;
        Vector2 lastLocalPos = Vector2.zero;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector2 localPos = transform.parent.InverseTransformPoint(drawPoints[i]);
            lineRenderer.SetPosition(i, new Vector3(localPos.x, localPos.y, -0.01f));

            if (i > 0)
                length += (lastLocalPos - localPos).magnitude;

            lastLocalPos = localPos;
        }
    }

    public void SetSelectionState(bool selected) { this.selected = selected; }

    Vector2 ProcessPoint(Vector2 endPointWorldSpace)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Vector2 a = anchorPoints[^2];
            Vector2 b = endPointWorldSpace;
            Vector2 mid = (a + b) / 2;

            bool xAxisLonger = (Mathf.Abs(a.x - b.x) > Mathf.Abs(a.y - b.y));
            if (xAxisLonger)
            {
                return new Vector2(b.x, a.y);
            }
            else
            {
                return new Vector2(a.x, b.y);
            }
        }
        return endPointWorldSpace;
    }

    void GenerateDrawPoints()
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
}
