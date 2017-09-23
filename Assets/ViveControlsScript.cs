using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControlsScript : MonoBehaviour 
{
    public VrControlsScript VrControls;
    public SteamVR_TrackedObject LeftHand;
    public SteamVR_TrackedObject RightHand;
    
    private bool _wasScrolling;
    private float _lastLeftScrollPos;
    private float _lastRightScrollPos;

    private void Update()
    {

        if (!LeftHand.isValid || !RightHand.isValid)
        {
            Debug.Log("Waiting for controllers.");
            return;
        }

        SteamVR_Controller.Device leftHandDevice = SteamVR_Controller.Input((int)LeftHand.index);
        SteamVR_Controller.Device rightHandDevice = SteamVR_Controller.Input((int)RightHand.index);
        
        bool scrolling = leftHandDevice.GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        Debug.Log(scrolling);
        Vector2 scroll = leftHandDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        float leftScrollDelta = (scrolling && !_wasScrolling) || !scrolling ? 0 : scroll.y - _lastLeftScrollPos;
        _lastLeftScrollPos = scroll.y;
        float rightScrollDelta = (scrolling && !_wasScrolling) || !scrolling ? 0 : scroll.x - _lastRightScrollPos;
        _lastRightScrollPos = scroll.x;
        _wasScrolling = scrolling;

        VrControls.RowSpanModification = leftScrollDelta;
        VrControls.ColumnSpanModification = rightScrollDelta;
        VrControls.LeftHandPressed = leftHandDevice.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip);
        VrControls.RightHandPressed = rightHandDevice.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip);

    }

}
