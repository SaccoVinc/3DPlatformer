using UnityEngine;
using Unity.Cinemachine;

[DisallowMultipleComponent]
public class CameraBasedTargetMovement : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Transform della camera (se nullo userà Camera.main).")]
    public Transform cameraTransform;

    [Tooltip("Se true e se presente, usa CinemachineOrbitalFollow.VerticalAxis.Value come sorgente (valori tipicamente 0..1).")]
    public bool preferCinemachineOrbitalFollow = true;

    [Header("Mapping (quando si usa CinemachineOrbitalFollow)")]
    [Tooltip("Range dell'asse VerticalAxis.Value che verrà mappato (default 0..1).")]
    public float orbitalAxisMin = 0f;
    public float orbitalAxisMax = 1f;

    [Header("Mapping (quando si usa il pitch della camera)")]
    [Tooltip("Range della inclinazione della camera (in gradi) che verrà mappato. Es: -45..45")]
    public float cameraPitchMin = -45f;
    public float cameraPitchMax = 45f;

    [Header("Output")]
    [Tooltip("Massimo offset Y (valore sarà mappato in -max..+max).")]
    public float outputYOffsetMax = 1f;

    [Tooltip("Velocità di smoothing (maggiore = più veloce).")]
    public float smoothSpeed = 10f;

    [Tooltip("Inverti il mapping sull'asse Y.")]
    public bool invertOutput = false;

    [Header("Look Target")]
    [Tooltip("Transform del target da spostare verticalmente.")]
    public Transform lookTarget;

    // cache
    CinemachineOrbitalFollow _orbitalFollow;

    Vector3 _initialLookTargetPos;

    void Reset()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (lookTarget == null)
            Debug.LogWarning("LookTarget non assegnato!");

        if (lookTarget != null)
            _initialLookTargetPos = lookTarget.localPosition;

        RefreshCinemachine();
    }

    void OnValidate()
    {
        outputYOffsetMax = Mathf.Abs(outputYOffsetMax);
        if (orbitalAxisMax < orbitalAxisMin) orbitalAxisMax = orbitalAxisMin;
        if (cameraPitchMax < cameraPitchMin) cameraPitchMax = cameraPitchMin;
    }

    void RefreshCinemachine()
    {
        _orbitalFollow = cameraTransform != null ? cameraTransform.GetComponent<CinemachineOrbitalFollow>() : null;
    }

    void LateUpdate()
    {
        if (cameraTransform == null || lookTarget == null)
            return;

        if (_orbitalFollow == null && preferCinemachineOrbitalFollow)
            RefreshCinemachine();

        bool usingOrbital = (preferCinemachineOrbitalFollow && _orbitalFollow != null);

        float targetYOffset;

        if (usingOrbital)
        {
            float axisVal = _orbitalFollow.VerticalAxis.Value;
            float t = Mathf.InverseLerp(orbitalAxisMin, orbitalAxisMax, axisVal);
            targetYOffset = Mathf.Lerp(-outputYOffsetMax, outputYOffsetMax, t);
        }
        else
        {
            Vector3 fwd = cameraTransform.forward;
            float pitch = Mathf.Asin(Mathf.Clamp(fwd.y, -1f, 1f)) * Mathf.Rad2Deg;
            float t = Mathf.InverseLerp(cameraPitchMin, cameraPitchMax, pitch);
            targetYOffset = Mathf.Lerp(-outputYOffsetMax, outputYOffsetMax, t);
        }

        if (invertOutput) targetYOffset = -targetYOffset;

        Vector3 currentPos = lookTarget.localPosition;
        Vector3 targetPos = new Vector3(_initialLookTargetPos.x, _initialLookTargetPos.y + targetYOffset, _initialLookTargetPos.z);

        float step = Mathf.Clamp01(smoothSpeed * Time.deltaTime);
        lookTarget.localPosition = Vector3.Lerp(currentPos, targetPos, step);
    }

    /// <summary>
    /// Se cambi la camera a runtime, chiama questo per aggiornare la cache
    /// </summary>
    public void SetCameraTransform(Transform cam)
    {
        cameraTransform = cam;
        RefreshCinemachine();
    }
}
