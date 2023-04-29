using System;
using UnityEngine;
using UnityEngine.Serialization;

public class IOScaler : MonoBehaviour
{
    public enum Mode
    {
        Input,
        Output
    }

    public Mode mode;
    public Pin pin;
    public Transform Connection;
    public Transform indicator;

    CircleCollider2D col;

    void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        ScalingManager.i.OnScaleChange += UpdateScale;
    }

    private void OnDestroy()
    {
        ScalingManager.i.OnScaleChange -= UpdateScale;
    }


    public void UpdateScale()
    {
        Connection.transform.localScale = new Vector3(
            ScalingManager.PinSize, ScalingManager.WireThickness / 10, 1);
        float xPos = mode == Mode.Input ? ScalingManager.PinSize
                                        : ScalingManager.PinSize * -1;
        pin.transform.localPosition = new Vector3(xPos, 0, -0.1f);
        
        indicator.transform.localScale =new Vector3(ScalingManager.PinSize, ScalingManager.PinSize, 1);
        
        col.radius = ScalingManager.PinSize / 2 * 1.25f;
    }
}