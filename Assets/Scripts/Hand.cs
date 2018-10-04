using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour {

    public static float TRIGGER_THRESHOLD = 0.95f;
    public static string PORTAL_TAG = "Portal";

    public OVRInput.Controller controller;

    private float triggerState;
    private bool holdingObject;
    private GameObject heldObject;

    void Update() {
        triggerState = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);
        if (triggerState < TRIGGER_THRESHOLD && holdingObject) {
            Release(heldObject);
        }
    }

    void OnTriggerStay(Collider other) {
        if (other.transform.parent != null && other.transform.parent.CompareTag(PORTAL_TAG)) {
            if (triggerState > TRIGGER_THRESHOLD && !holdingObject) {
                Grab(other.transform.parent.gameObject);
            }
        }
    }

    void Grab(GameObject obj) {
        holdingObject = true;
        heldObject = obj;
        heldObject.transform.parent = this.transform;
    }

    void Release(GameObject obj) {
        heldObject.transform.parent = null;
        heldObject = null;
        holdingObject = false;
    }
}
