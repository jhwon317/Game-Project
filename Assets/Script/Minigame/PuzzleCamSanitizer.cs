using UnityEngine;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif
#if CINEMACHINE
using Cinemachine;
#endif

public static class PuzzleCamSanitizer
{
    public static void Apply(Camera cam, int miniLayer = -1)
    {
        if (!cam) return;

        // MainCamera �±� ����
        if (cam.CompareTag("MainCamera")) cam.tag = "Untagged";

        // ����� ������ ����
        var al = cam.GetComponent<AudioListener>();
        if (al) Object.Destroy(al);

        // URP: Base + ���� ���
#if UNITY_RENDER_PIPELINE_UNIVERSAL
        var urp = cam.GetComponent<UniversalAdditionalCameraData>();
        if (urp){ urp.renderType = CameraRenderType.Base; urp.cameraStack.Clear(); }
#endif

        // Cinemachine ���� ����(�ʿ� ��)
#if CINEMACHINE
        var brain = cam.GetComponent<CinemachineBrain>();
        if (brain) Object.Destroy(brain);
        foreach (var vcam in cam.GetComponentsInChildren<CinemachineVirtualCamera>(true))
            vcam.enabled = false;
#endif

        // ���� ���̾ ����
        if (miniLayer >= 0) cam.cullingMask = 1 << miniLayer;

        cam.depth = -100;
        cam.clearFlags = CameraClearFlags.SolidColor;
        var c = cam.backgroundColor; c.a = 0f; cam.backgroundColor = c;
    }

    public static void SetLayerRecursive(GameObject root, int layer)
    {
        if (!root) return;
        root.layer = layer;
        foreach (Transform t in root.transform)
            SetLayerRecursive(t.gameObject, layer);
    }
}
