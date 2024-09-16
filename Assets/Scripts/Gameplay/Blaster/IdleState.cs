using UnityEngine;

public class IdleState : BaseHandCanonState
{
    private static readonly int _SRelease = Animator.StringToHash("Release");

    public IdleState(HandCannon handCannon) : base(handCannon)
    {
    }

    public override void EnterState()
    {
    }
}