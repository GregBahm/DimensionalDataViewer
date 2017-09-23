using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VrControlsScript : MonoBehaviour
{
    public MainScript MainScript;

    public float RowSpanModification;
    public float ColumnSpanModification;

    public Transform MainHand;
    public Transform OffHand;
    public Transform PaintMotionDampener;
    private Transform HandAverage;

    private bool _scaleMode;
    private bool _scaling;
    private float _initialScale;
    private float _initialHandDistance;

    public bool LeftHandPressed;
    public bool RightHandPressed;

    private EaseTowardsTarget _paintbrushEaser;

    private void Start()
    {
        _paintbrushEaser = PaintMotionDampener.GetComponent<EaseTowardsTarget>();
        HandAverage = new GameObject("Hand Average").transform;
    }

    void Update ()
    {
        UpdateGridSpans();
        MoveDrawing();
        UpdateScaleMode();
    }

    private void UpdateGridSpans()
    {
        MainScript.Width += RowSpanModification;
        MainScript.Width = Mathf.Clamp(MainScript.Width, 0.1f, 1f);
        MainScript.Height += ColumnSpanModification;
        MainScript.Height = Mathf.Clamp(MainScript.Height, 0.1f, 1f);
    }

    private void MoveDrawing()
    {
        HandAverage.position = Vector3.Lerp(MainHand.position, OffHand.position, .5f);
        HandAverage.rotation = Quaternion.Lerp(MainHand.rotation, OffHand.rotation, .5f);
        HandAverage.LookAt(MainHand.position, HandAverage.up);

        _scaleMode = LeftHandPressed && RightHandPressed;

        _paintbrushEaser.Target = GetDampenerTarget(LeftHandPressed, RightHandPressed);
    }
    private void UpdateScaleMode()
    {
        if (_scaleMode)
        {
            float dist = (MainHand.position - OffHand.position).magnitude;
            if (!_scaling)
            {
                _scaling = true;
                _initialScale = HandAverage.transform.localScale.x;
                _initialHandDistance = dist;
            }
            else
            {
                float newScale = _initialScale * (dist / _initialHandDistance);
                HandAverage.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
        else
        {
            _scaling = false;
        }
    }
    private Transform GetDampenerTarget(bool leftHand, bool rightHand)
    {
        if (leftHand && rightHand)
        {
            return HandAverage;
        }
        if (leftHand)
        {
            return MainHand;
        }
        if (rightHand)
        {
            return OffHand;
        }
        return null;
    }

}