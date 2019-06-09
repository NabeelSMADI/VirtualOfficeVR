using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class KeyboardLocation : MonoBehaviour
{

    LeapServiceProvider provider; //!< Leap motion Service Provider
    public GameObject keybord;

    public GameObject rightKey;
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
        if (Input.GetKeyUp(KeyCode.LeftControl))
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

            Vector3 v1 = rightKey.transform.position - leftControlKey.transform.position ;
            Vector3 v2 = rightKey.transform.position - LeftHand.Fingers[1].TipPosition.ToVector3() ;
            v1 = new Vector3(v1.x, 0, v1.z);
            v2 = new Vector3(v2.x, 0, v2.z);
            float Angle = Vector2.Angle(v1, v2);

            Vector2 A = new Vector2(leftControlKey.transform.position.x, leftControlKey.transform.position.z);
            Vector2 B = new Vector2(rightKey.transform.position.x, rightKey.transform.position.z);
            Vector2 C = new Vector2(LeftHand.Fingers[1].TipPosition.ToVector3().x, LeftHand.Fingers[1].TipPosition.ToVector3().z);
            float Angle2 = Vector2.Angle(A - B, C - B);

            Debug.Log("Angle: " + Angle2);
            keybord.transform.LookAt(new Vector3(LeftHand.Fingers[1].TipPosition.ToVector3().x, keybord.transform.position.y, LeftHand.Fingers[1].TipPosition.ToVector3().z));
          //  keybord.transform.LookAt(LeftHand.Fingers[1].TipPosition.ToVector3());
            //keybord.transform.eulerAngles = new Vector3(keybord.transform.rotation.x, keybord.transform.rotation.y + Angle, keybord.transform.rotation.z);
            //keybord.transform.rotation = Quaternion.Euler(new Vector3(0, keybord.transform.rotation.y, 0));
            // keybord.transform.RotateAround(rightKey.transform.position, Vector3.right, Angle);
        }
    }

}
