using UnityEngine;

public class PaintGunPickUp : PickUpItem
{
    [SerializeField] GameObject tankModel;
    [SerializeField] GameObject handModel;
    [SerializeField] ParticleSystem shootParticles;

    [SerializeField] Transform playerBackTransform;
    [SerializeField] Transform playerHandTransform;
    [SerializeField] Transform shootParticlesTransform;
    public override void OnPickUp(Collider player)
    {
        base.OnPickUp(player);

        PlayerActionStateMachine actionStateMachine = player.GetComponent<PlayerActionStateMachine>();

        GameObject tank = Instantiate(tankModel, playerBackTransform);

        GameObject handGun = Instantiate(handModel, playerHandTransform);
        
        ParticleSystem particles = Instantiate(shootParticles, shootParticlesTransform);

        actionStateMachine.PaintParticles = particles;
        actionStateMachine.PaintParticlesParent = shootParticlesTransform;

        actionStateMachine.CurrentState.SwitchState(actionStateMachine.States.PaintGun());
    }
}
