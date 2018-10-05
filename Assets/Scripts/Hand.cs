using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour {

    public static float TRIGGER_THRESHOLD = 0.95f;

    public OVRInput.Controller controller;

    [HideInInspector]
    public GameObject heldObjectContender;

    private float triggerState;
    private GameObject heldObject;

    void Update() {
        triggerState = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);
        
        if (triggerState > TRIGGER_THRESHOLD && heldObject == null) {
            Grab(heldObjectContender);
        }

        if (triggerState < TRIGGER_THRESHOLD && heldObject != null) {
            Release(heldObject);
        }
    }

    void Grab(GameObject obj) {
        heldObject = obj;
        heldObject.transform.parent = this.transform;
    }

    void Release(GameObject obj) {
        heldObject.transform.parent = null;
        heldObject = null;
    }
}
