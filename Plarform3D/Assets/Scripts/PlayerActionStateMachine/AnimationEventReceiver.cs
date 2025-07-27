using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    private PlayerActionStateMachine parentStateMachine;
    [SerializeField] private GameObject dustParticle;
    [SerializeField] private GameObject dustParticleSpawnLocation;

    void Awake()
    {
        parentStateMachine = GetComponentInParent<PlayerActionStateMachine>();

        if (parentStateMachine == null)
        {
            Debug.LogError($"PlayerActionStateMachine not found in parent of {gameObject.name}");
        }
    }

    public void OnPunchDashStart()
    {

        if (parentStateMachine != null)
        {
            parentStateMachine.OnPunchDashStart();
        }
    }

    public void OnPunchDashEnd()
    {

        if (parentStateMachine != null)
        {
            parentStateMachine.OnPunchDashEnd();
        }
    }

    [System.Obsolete]
    public void OnDustParticle()
    {
        ParticleManager.Instance.SpawnParticle(dustParticle, dustParticleSpawnLocation.transform.position, Quaternion.identity);
    }
}