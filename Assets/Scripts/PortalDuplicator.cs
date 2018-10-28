using System.Collections;
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
            dupedObj.name = obj.name;

            // Add Duplicate script
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
        if (dupeMapping.TryGetValue(obj, out dupedObj)) {
            dupeMapping.Remove(obj);

            if (detector.detectedObjs.Contains(obj)) { // Obj reversed back through the portal
                Destroy(dupedObj);
                SetRenderQueue(obj, GEOMETRY_RENDER_QUEUE);
            } else { // Obj has passed through the portal
                Destroy(dupedObj.GetComponent<Duplicate>());
                SetRenderQueue(dupedObj, GEOMETRY_RENDER_QUEUE);

                // Transfer rigidbody parameters from obj to dupedObj
                Rigidbody dupedObjRb = dupedObj.GetComponent<Rigidbody>();
                if (dupedObjRb != null) {
                    Rigidbody objRb = obj.GetComponent<Rigidbody>();
                    dupedObjRb.isKinematic = objRb.isKinematic;
                    dupedObjRb.useGravity = objRb.useGravity;
                    Vector3 localVelocity = obj.transform.InverseTransformVector(objRb.velocity);
                    dupedObjRb.velocity = dupedObjRb.transform.TransformVector(localVelocity);
                    Vector3 localAngularVelocity = obj.transform.InverseTransformVector(objRb.angularVelocity);
                    dupedObjRb.angularVelocity = dupedObjRb.transform.TransformVector(localAngularVelocity);
                }

                Destroy(obj);
            }
        }
    }

    /**
     * Checks if the given collider comes from one of its sibling objects.
     * (i.e. the portal frame).
     */
    private bool IsSibling(Collider other) {
        return transform.parent == other.transform.parent;
    }

    /**
     * Sets the render queue of an object's materials.
     * If set to TRANSPARENT_RENDER_QUEUE, this will make the object invisible
     * when blocked by a depth mask shader.
     * If set to GEOMETRY_RENDER_QUEUE, it's a normal object.
     * TODO: Set renderqueue to the (remembered) original value, not a preset one.
     */
    private void SetRenderQueue(GameObject obj, int renderQueue) {
        Material[] materials = obj.GetComponent<Renderer>().materials;
        for (int i = 0; i < materials.Length; ++i) {
            materials[i].renderQueue = renderQueue;
        }
    }

    /**
     * Looks at the given obj's chain of parents and returns the highest
     * level obj that's marked as Duplicable. Returns null if none found.
     */ 
    private GameObject FindDuplicableObject(GameObject obj) {
        return null;
    }
}
