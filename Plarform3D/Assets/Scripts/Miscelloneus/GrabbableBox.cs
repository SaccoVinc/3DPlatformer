using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableBox : MonoBehaviour
{
    [Header("Grab Settings")]
    [SerializeField] private float grabDistance = 1.5f;
    [SerializeField] private float maxVelocity = 6f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float stopDistance = 0.05f;

    private Rigidbody rb;
    private Transform grabber;
    private bool isGrabbed = false;

    public bool IsGrabbed => isGrabbed;
    public float GrabDistance => grabDistance;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }


    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        if (!isGrabbed || grabber == null) return;

        // Target davanti al grabber, stessa altezza della box
        Vector3 targetPosition = grabber.position + grabber.forward * grabDistance;

        // Solo movimento orizzontale (X/Z)
        Vector3 toTarget = targetPosition - rb.position;
        toTarget.y = 0f;

        if (toTarget.magnitude < stopDistance)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 desiredVelocity = toTarget.normalized * maxVelocity;
        Vector3 velocityChange = desiredVelocity - rb.linearVelocity;
        velocityChange.y = 0f;

        rb.AddForce(velocityChange * acceleration * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    public void StartGrab(Transform grabberTransform)
    {
        if (isGrabbed) return;

        isGrabbed = true;
        grabber = grabberTransform;

        rb.mass = 1f;
        rb.linearDamping = 6f;                    
        rb.angularDamping = 10f;             
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true;        
    }

    public void StopGrab()
    {
        if (!isGrabbed) return;

        isGrabbed = false;
        grabber = null;

        rb.mass = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.freezeRotation = false;
    }

    public bool CanBeGrabbed(Transform grabberTransform)
    {
        if (isGrabbed) return false;
        float distance = Vector3.Distance(transform.position, grabberTransform.position);
        return distance <= grabDistance + 1f;
    }
}
