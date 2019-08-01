using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class KeyboardLocation : MonoBehaviour
{

    LeapServiceProvider provider; //!< Leap motion Service Provider
    public GameObject Keyboard;   //!< Keyboard GameObject
    public GameObject leftControlKey; //!< left Control Key Position on the Keyboard GameObject

    public bool LeftControlKeyDown = false; //!< is Left Control Key Down
    public bool RightKeyUp = false; //!< is Right Key Up

    private static KeyboardLocation _KeyboardLocation;

    public static KeyboardLocation GetInstance()
    {
        return _KeyboardLocation;
    }


    // Start is called before the first frame update
    void Start()
    {
        if (_KeyboardLocation == null)
        {
            _KeyboardLocation = this;
        }
        provider = FindObjectOfType<LeapServiceProvider>() as LeapServiceProvider;
    }

    // Update is called once per frame
    void Update()
    {
        Relocate();
    }


    /// <summary>  
    /// Checks if Left Control and Right is Pressed to locate Keyboard with help of the fingers location.
    /// </summary> 
    void Relocate()
    {
        if (LeftControlKeyDown && RightKeyUp)
            {
            RightKeyUp = false;
            Hand RightHand = null;
            Hand LeftHand = null;
            Frame frame = provider.CurrentFrame;
            foreach (Hand hand in frame.Hands)
            {
                if (hand.IsRight) RightHand = hand;
                if (hand.IsLeft) LeftHand = hand;
            }
            Keyboard.transform.position = RightHand.Fingers[1].TipPosition.ToVector3();
            Keyboard.transform.LookAt(new Vector3(LeftHand.Fingers[1].TipPosition.ToVector3().x,
                                      Keyboard.transform.position.y, LeftHand.Fingers[1].TipPosition.ToVector3().z));
            Keyboard.SetActive(true);
        }
    }
}
