using UnityEngine;

public class PlayerGrabActionState : PlayerActionBaseState
{
    private GrabbableBox _currentGrabbedBox;
    private bool _isGrabbingBox = false;

    public PlayerGrabActionState(PlayerActionStateMachine currentContext, PlayerActionStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public override void EnterState()
    {
        _isGrabbingBox = false;
        _ctx.PlayerStateMachine.IsGrabbingStarted = false;
        _ctx.Animator.SetBool(_ctx.GrabHash, true);
        _ctx.Animator.SetLayerWeight(3, 1f);
        InitializeSubState();
    }

    public override void UpdateState()
    {
        if (_currentGrabbedBox == null)
        {
            _currentGrabbedBox = FindGrabbableBox();
            _ctx.PlayerStateMachine.IsGrabbingStarted = false;
        }

        if (_currentGrabbedBox != null && !_isGrabbingBox)
        {
            _isGrabbingBox = true;
            _ctx.PlayerStateMachine.IsGrabbingStarted = true;
            _currentGrabbedBox.StartGrab(_ctx.transform);
        }

        CheckSwitchStates();
    }

    public override void ExitState()
    {
        if (_currentGrabbedBox != null)
        {
            _currentGrabbedBox.StopGrab();
            _currentGrabbedBox = null;
        }

        _ctx.PlayerStateMachine.IsGrabbingStarted = false;
        _ctx.Animator.SetBool(_ctx.GrabHash, false);
        _ctx.Animator.SetLayerWeight(3, 0f);
        _isGrabbingBox = false;
    }

    public override void InitializeSubState() { }

    public override void CheckSwitchStates()
    {
        if (!_ctx.IsGrabPressed)
        {
            SwitchState(_factory.Idle());
            return;
        }

        if (_currentGrabbedBox != null)
        {
            float distance = Vector3.Distance(_currentGrabbedBox.transform.position, _ctx.transform.position);
            if (distance > _currentGrabbedBox.GrabDistance + 3f)
            {
                SwitchState(_factory.Idle());
                return;
            }
        }

        if (_ctx.IsAttackPressed && !_ctx.RequireNewAttackPress)
        {
            // Eventuale logica futura per interazioni durante il grab
        }
    }

    private GrabbableBox FindGrabbableBox()
    {
        RaycastHit hit;
        Vector3 origin = _ctx.transform.position + Vector3.up * 0.5f;
        Vector3 direction = _ctx.transform.forward;

        if (Physics.Raycast(origin, direction, out hit, 3f))
        {
            GrabbableBox box = hit.collider.GetComponent<GrabbableBox>();
            if (box != null && box.CanBeGrabbed(_ctx.transform))
                return box;
        }

        Collider[] colliders = Physics.OverlapSphere(_ctx.transform.position, 2.5f);
        foreach (var col in colliders)
        {
            GrabbableBox box = col.GetComponent<GrabbableBox>();
            if (box != null && box.CanBeGrabbed(_ctx.transform))
                return box;
        }

        return null;
    }
}
