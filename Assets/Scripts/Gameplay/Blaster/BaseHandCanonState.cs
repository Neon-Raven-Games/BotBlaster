using Gameplay.Enemies;
using UnityEngine;

public abstract class BaseHandCanonState
{
    protected HandCannon handCannon;

    protected BaseHandCanonState(HandCannon handCannon)
    {
        this.handCannon = handCannon;
    }

    public virtual void EnterState()
    {
    }

    public virtual void ExitState()
    {
    }

    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {
        
    }

    protected void ChangeState(CannonState state) => handCannon.ChangeState(state);

    public virtual void GripAction()
    {
    }

    public virtual void GripReleaseAction()
    {
    }

    public virtual void FireAction()
    {
    }

    public virtual void FireReleaseAction()
    {
    }

    public virtual void OnTriggerExit(Collider other)
    {
    }

    public virtual void OnTriggerEnter(Collider other)
    {
    }

    public virtual void OnDrawGizmos()
    {
    }

    public virtual void OnTriggerStay(Collider other)
    {
    }
}