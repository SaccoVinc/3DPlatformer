using UnityEngine;
using Unity.Cinemachine;

[DisallowMultipleComponent]
public class CameraBasedAim : MonoBehaviour
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
    [Tooltip("Massimo angolo X (valore sarà mappato in -max..+max).")]
    public float outputAngleMax = 25f;

    [Tooltip("Velocità di smoothing (maggiore = più veloce).")]
    public float smoothSpeed = 10f;

    [Tooltip("Inverti il mapping sull'asse X.")]
    public bool invertOutput = false;

    // cache
    CinemachineOrbitalFollow _orbitalFollow;

    void Reset()
    {
        // default camera
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        RefreshCinemachine();
    }

    void OnValidate()
    {
        // valori sensati
        outputAngleMax = Mathf.Abs(outputAngleMax);
        if (orbitalAxisMax < orbitalAxisMin) orbitalAxisMax = orbitalAxisMin;
        if (cameraPitchMax < cameraPitchMin) cameraPitchMax = cameraPitchMin;
    }

    void RefreshCinemachine()
    {
        _orbitalFollow = cameraTransform != null ? cameraTransform.GetComponent<CinemachineOrbitalFollow>() : null;
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        // aggiorna la cache se la camera è cambiata
        if (_orbitalFollow == null && preferCinemachineOrbitalFollow)
            RefreshCinemachine();

        bool usingOrbital = (preferCinemachineOrbitalFollow && _orbitalFollow != null);

        float targetX;

        if (usingOrbital)
        {
            // ottieni valore dell'asse (es. 0..1)
            float axisVal = _orbitalFollow.VerticalAxis.Value;
            // mappa orbitalAxisMin..orbitalAxisMax -> -outputAngleMax..outputAngleMax
            float t = Mathf.InverseLerp(orbitalAxisMin, orbitalAxisMax, axisVal);
            targetX = Mathf.Lerp(-outputAngleMax, outputAngleMax, t);
        }
        else
        {
            // calcola pitch della camera in gradi (-90..+90)
            Vector3 fwd = cameraTransform.forward;
            float pitch = Mathf.Asin(Mathf.Clamp(fwd.y, -1f, 1f)) * Mathf.Rad2Deg;
            // mappa cameraPitchMin..cameraPitchMax -> -outputAngleMax..outputAngleMax
            float t = Mathf.InverseLerp(cameraPitchMin, cameraPitchMax, pitch);
            targetX = Mathf.Lerp(-outputAngleMax, outputAngleMax, t);
        }

        if (invertOutput) targetX = -targetX;

        // applica smoothing con quaternion per evitare salti
        Quaternion current = transform.localRotation;
        Vector3 ce = current.eulerAngles; // ok usare eulerAngles qui perché poi ricostruiamo quaternion
        Quaternion targetRot = Quaternion.Euler(targetX, ce.y, ce.z);
        float step = Mathf.Clamp01(smoothSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(current, targetRot, step);
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
