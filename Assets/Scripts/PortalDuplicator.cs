﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Control mechanism that temporarily duplicates objects that pass through the portal.
 */
[RequireComponent(typeof(Portal))]
public class PortalDuplicator : MonoBehaviour {

    public static int DUPLICABLE_LAYER = 8;
    public static int TRANSPARENT_RENDER_QUEUE = 2700;
    public static int GEOMETRY_RENDER_QUEUE = 2000;

    public Detector detector;

    private Portal portal;
    private Dictionary<GameObject, GameObject> dupeMapping = new Dictionary<GameObject, GameObject>();
    private Dictionary<GameObject, GameObject> mirrorDupeMapping;

    private void Start() {
        portal = GetComponent<Portal>();
        mirrorDupeMapping = portal.mirrorPortalSurface.GetComponent<PortalDuplicator>().dupeMapping;
    }

    private void OnTriggerEnter(Collider other) {
        GameObject obj = other.gameObject;
        if (!IsSibling(other)
            && obj.layer == DUPLICABLE_LAYER
            && !dupeMapping.ContainsKey(obj) && !dupeMapping.ContainsValue(obj)
            && !mirrorDupeMapping.ContainsKey(obj) && !mirrorDupeMapping.ContainsValue(obj)
            && detector.detectedObjs.Contains(obj)) {

            // Set the object to appear invisible w/ the cube
            SetRenderQueue(obj, TRANSPARENT_RENDER_QUEUE);

            // Duplicate the Object
            GameObject dupedObj = GameObject.Instantiate(obj);
            Duplicate dupeScript = dupedObj.AddComponent<Duplicate>();
            dupeScript.original = obj.transform;
            dupeScript.source = portal.source;
            dupeScript.destination = portal.destination;

            dupeMapping.Add(obj, dupedObj);
        }
    }

    private void OnTriggerExit(Collider other) {
        GameObject obj = other.gameObject;
        GameObject dupedObj;
        if (detector.detectedObjs.Contains(obj) && dupeMapping.TryGetValue(obj, out dupedObj)) {
            // Destroy duped version
            Destroy(dupedObj);
            dupeMapping.Remove(obj);

            // Set back to normal render queue
            SetRenderQueue(obj, GEOMETRY_RENDER_QUEUE);
        }
    }

    /**
     * Checks if the given collider comes from one of its sibling objects.
     * (i.e. the portal frame).
     */
    private bool IsSibling(Collider other) {
        return transform.parent == other.transform.parent;
    }

    private void SetRenderQueue(GameObject obj, int renderQueue) {
        Material[] materials = obj.GetComponent<Renderer>().materials;
        for (int i = 0; i < materials.Length; ++i) {
            materials[i].renderQueue = renderQueue;
        }
    }
}
