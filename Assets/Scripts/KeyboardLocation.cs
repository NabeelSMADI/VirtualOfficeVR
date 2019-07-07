using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class KeyboardLocation : MonoBehaviour
{

    LeapServiceProvider provider; //!< Leap motion Service Provider
    public GameObject keybord;
    public GameObject leftControlKey;

    // Start is called before the first frame update
    void Start()
    {
        provider = FindObjectOfType<LeapServiceProvider>() as LeapServiceProvider;
    }

    // Update is called once per frame
    void Update()
    {
        Relocate();
    }


    /// <summary>  
    /// Checks if pinch gesture is active
    /// </summary> 
    void Relocate()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey("right"))
        {
            Debug.Log("click: " );

            Hand RightHand = null;
            Hand LeftHand = null;
            Frame frame = provider.CurrentFrame;
            foreach (Hand hand in frame.Hands)
            {
                if (hand.IsRight) RightHand = hand;
                if (hand.IsLeft) LeftHand = hand;
            }

            keybord.transform.position = RightHand.Fingers[1].TipPosition.ToVector3();
            keybord.transform.LookAt(new Vector3(LeftHand.Fingers[1].TipPosition.ToVector3().x, keybord.transform.position.y, LeftHand.Fingers[1].TipPosition.ToVector3().z));
            keybord.SetActive(true);

        }
    }

}
