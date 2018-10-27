using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour {

    [HideInInspector]
    public List<GameObject> detectedObjs = new List<GameObject>();

    private void Start() {
        
    }

    private void OnTriggerEnter(Collider other) {
        detectedObjs.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other) {
        detectedObjs.Remove(other.gameObject);
    }
}
