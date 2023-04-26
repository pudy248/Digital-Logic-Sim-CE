using System;
using System.Collections;
using System.Collections.Generic;
using DLS.Simulation;
using UnityEngine;
using UnityEngine.Serialization;

public class Wire : MonoBehaviour
{
    //Event
    public event Action OnSelection;
    public event Action OnDeSelection;

    public event Action<List<Vector2>> OnWireChange;

    public event Action OnPlacing; 

    bool wireConnected;

    // [HideInInspector]
    public Pin startPin;

    // [HideInInspector]  
    public Pin endPin;



    public List<Vector2> anchorPoints { get; private set; }



    float depth;


    void Start()
    {
        // Simulation.instance.OnSimulationTogle += (_)=> NotifyStateChange();

    }

    public Pin ChipInputPin => (startPin.pinType == Pin.PinType.ChipInput) ? startPin : endPin;

    public Pin ChipOutputPin => (startPin.pinType == Pin.PinType.ChipOutput) ? startPin : endPin;


    public void SetAnchorPoints(Vector2[] newAnchorPoints)
    {
        anchorPoints = new List<Vector2>(newAnchorPoints);
        NotifyWireChange();
    }

    public void SetDepth(int numWires)
    {
        depth = numWires * 0.01f;
        transform.localPosition = Vector3.forward * depth;
    }

    void LateUpdate()
    {
        if (!wireConnected) return;

        float depthOffset = 5;

        transform.localPosition = Vector3.forward * (depth + depthOffset);
        UpdateWirePos();
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
            NotifyWireChange();
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


        transform.localPosition = new Vector3(0, 0, transform.localPosition.z);

        anchorPoints = new List<Vector2>();
        
        anchorPoints.Add(startPin.transform.position);
        anchorPoints.Add(startPin.transform.position);
        
        // UpdateSmoothedLine();
        NotifyWireChange();
    }

    public void ConnectToFirstPinViaWire(Pin startPin, Wire parentWire, Vector2 inputPoint)
    {
        anchorPoints = new List<Vector2>();

        
        this.startPin = startPin;
        transform.localPosition = new Vector3(0, 0, transform.localPosition.z);


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

        // UpdateSmoothedLine();
        NotifyWireChange();
    }

    // Connect the input pin to the output pin
    public void Place(Pin endPin)
    {
        this.endPin = endPin;
        anchorPoints[^1] = endPin.transform.position;

        wireConnected = true;
        
        OnPlacing?.Invoke();
        NotifyWireChange();

        if (endPin.pinType == Pin.PinType.ChipOutput)
            SwapStartEndPoints();
    }

    void SwapStartEndPoints()
    {
        (startPin, endPin) = (endPin, startPin);

        anchorPoints.Reverse();
        NotifyWireChange();
        // UpdateSmoothedLine();
        // UpdateCollider();
    }


    void NotifyWireChange()
    {
        OnWireChange?.Invoke(anchorPoints);
    }

    // Update position of wire end point (for when initially placing the wire)
    public void UpdateWireEndPoint(Vector2 endPointWorldSpace)
    {
        anchorPoints[^1] = ProcessPoint(endPointWorldSpace);
        NotifyWireChange();
        //UpdateSmoothedLine();
    }

    // Add anchor point (for when initially placing the wire)
    public void AddAnchorPoint(Vector2 pointWorldSpace)
    {
        anchorPoints[^1] = ProcessPoint(pointWorldSpace);
        anchorPoints.Add(ProcessPoint(pointWorldSpace));
        NotifyWireChange();
    }


    public void SetSelectionState(bool selected)
    {
        if (selected)
            OnSelection?.Invoke();
        else
            OnDeSelection?.Invoke();
    }

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

    
}