using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {

    public static string surfaceName = "Portal Surface";
    public static string shaderName = "Custom/PortalShaderVR";
    public static string leftTextureName = "_LeftTex";
    public static string rightTextureName = "_RightTex";

    public int TEXTURE_SIZE = 4096;
    public Transform source, destination;

    private Camera portalCamera;
    private Material material;
    private RenderTexture leftTexture, rightTexture;

    // Helpers
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m) { return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); }
    public static Vector4 PosToV4(Vector3 v) { return new Vector4(v.x, v.y, v.z, 1.0f); }
    public static Vector3 ToV3(Vector4 v) { return new Vector3(v.x, v.y, v.z); }

    void Start () {
        // Create material
        material = new Material(Shader.Find(shaderName));
        GetComponent<MeshRenderer>().materials = new Material[] { material };

        // Create shader textures
        leftTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 24);
        rightTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 24);

        // Use LeftEyeAnchor camera for reference
        Camera leftEyeCamera = GameObject.Find("LeftEyeAnchor").GetComponent<Camera>();
        if (leftEyeCamera == null) {
            print("Couldn't find left eye for reference, failed to initialize portal camera.");
        }

        // Create portal camera
        GameObject cameraObject = new GameObject("Portal Camera");
        portalCamera = cameraObject.AddComponent<Camera>();
        portalCamera.stereoTargetEye = StereoTargetEyeMask.None;
        portalCamera.fieldOfView = 87.9f; // TODO: Look into a better way of determining this number.
        portalCamera.aspect = leftEyeCamera.aspect;
        portalCamera.enabled = false;
    }

    private void OnWillRenderObject() {
        if (Camera.current.name.Equals("LeftEyeAnchor")) {
            MirrorCamera(Camera.current);
            portalCamera.targetTexture = leftTexture;
            portalCamera.Render();
            Shader.SetGlobalInt("RenderingEye", 0);
            material.SetTexture(leftTextureName, leftTexture);
        } else if (Camera.current.name.Equals("RightEyeAnchor")) {
            MirrorCamera(Camera.current);
            portalCamera.targetTexture = rightTexture;
            portalCamera.Render();
            Shader.SetGlobalInt("RenderingEye", 1);
            material.SetTexture(rightTextureName, rightTexture);
        }
    }

    private void MirrorCamera(Camera cam) {
        // Rotate Source 180 degrees so PortalCamera is mirror image of rootCamera
        Matrix4x4 destinationFlipRotation = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), Vector3.one);
        Matrix4x4 sourceInvMat = destinationFlipRotation * source.worldToLocalMatrix;

        //Matrix4x4 sourceInvMat = source.worldToLocalMatrix;

        // Calculate translation and rotation of MainCamera in Source space
        Vector3 cameraPositionInSourceSpace = ToV3(sourceInvMat * PosToV4(cam.transform.position));
        Quaternion cameraRotationInSourceSpace = QuaternionFromMatrix(sourceInvMat) * cam.transform.rotation;

        // Transform Portal Camera to World Space relative to Destination transform,
        // matching the Main Camera position/orientation
        portalCamera.transform.position = destination.TransformPoint(cameraPositionInSourceSpace);
        portalCamera.transform.rotation = destination.rotation * cameraRotationInSourceSpace;

        // Calculate clip plane for portal (for culling of objects inbetween destination camera and portal)
        Vector4 clipPlaneWorldSpace = new Vector4(destination.forward.x, destination.forward.y, destination.forward.z, Vector3.Dot(destination.position, -destination.forward));
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

        // Update projection based on new clip plane
        // Note: http://aras-p.info/texts/obliqueortho.html and http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
        portalCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }
}
