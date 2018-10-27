using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duplicate : MonoBehaviour {

    public Transform original;
    public Transform source, destination;

    // Helpers
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m) { return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); }
    public static Vector4 PosToV4(Vector3 v) { return new Vector4(v.x, v.y, v.z, 1.0f); }
    public static Vector3 ToV3(Vector4 v) { return new Vector3(v.x, v.y, v.z); }

    void Update () {
        MirrorObject();
	}

    private void MirrorObject() {
        // Rotate Source 180 degrees so it's mirror image of original
        Matrix4x4 destinationFlipRotation = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), Vector3.one);
        Matrix4x4 sourceInvMat = destinationFlipRotation * source.worldToLocalMatrix;

        // Calculate translation and rotation of original in Source space
        Vector3 cameraPositionInSourceSpace = ToV3(sourceInvMat * PosToV4(original.position));
        Quaternion cameraRotationInSourceSpace = QuaternionFromMatrix(sourceInvMat) * original.rotation;

        // Transform to World Space relative to Destination transform,
        // matching the original's position/orientation
        transform.position = destination.TransformPoint(cameraPositionInSourceSpace);
        transform.rotation = destination.rotation * cameraRotationInSourceSpace;
    }
}
