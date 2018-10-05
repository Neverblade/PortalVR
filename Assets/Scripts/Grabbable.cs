using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour {

    public static string HAND_TAG = "Hand";

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag(HAND_TAG)) {
            Hand hand = other.gameObject.GetComponent<Hand>();
            if (hand != null) {
                hand.heldObjectContender = gameObject;
            }
        }
    }
}
