using UnityEngine;

public class PaintGunPickUp : PickUpItem
{
    public override void OnPickUp(Collider player)
    {
        base.OnPickUp(player);

        PlayerActionStateMachine actionStateMachine = player.GetComponent<PlayerActionStateMachine>();

        actionStateMachine.CurrentState.SwitchState(actionStateMachine.States.PaintGun());
    }
}
