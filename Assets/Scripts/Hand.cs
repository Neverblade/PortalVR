using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Hand : MonoBehaviour {

    public static float TRIGGER_THRESHOLD = 0.95f;
    public static string GRABBABLE_TAG = "Grabbable";

    public OVRInput.Controller controller;

    private float triggerState;
    private GameObject heldObject;
    private List<GameObject> heldObjectContenders = new List<GameObject>();

    void Update() {
        triggerState = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller);
        
        if (triggerState > TRIGGER_THRESHOLD && heldObject == null && heldObjectContenders.Count > 0) {
            Grab(heldObjectContenders[heldObjectContenders.Count - 1]);
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

    private void OnTriggerEnter(Collider other) {
        GameObject contender = GetGrabbableObject(other);
        if (contender != null) {
            heldObjectContenders.Add(contender);
        }
    }

    private void OnTriggerExit(Collider other) {
        GameObject contender = GetGrabbableObject(other);
        if (contender != null) {
            heldObjectContenders.Remove(contender);
        }
    }

    /**
     * Returns whether or not this collider belongs to a grabbable object.
     * This is satisfied when its gameobject or one of its parents is marked with the grabbable tag.
     * Returns the grabbable object in question, or null if none is found.
     */ 
    private GameObject GetGrabbableObject(Collider other) {
        GameObject obj = other.gameObject;
        while (true) {
            if (obj.CompareTag(GRABBABLE_TAG)) {
                return obj;
            } else if (obj.transform.parent != null) {
                obj = obj.transform.parent.gameObject;
            } else {
                return null;
            }
        }
    }
}
