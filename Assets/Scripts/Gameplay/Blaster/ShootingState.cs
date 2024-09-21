using Gameplay;
using Gameplay.Enemies;
using UnityEngine;
using Util;

public class ShootingState : BaseHandCanonState
{
    private readonly CharacterController _controller;

    private bool _beam;
    private GameObject _beamObject;
    private bool _launchRequested;
    private float _fireTime;

    public ShootingState(HandCannon handCannon) : base(handCannon)
    {
        _controller = handCannon.actor.GetComponent<CharacterController>();
    }

    public override void EnterState()
    {
        handCannon.muzzleFlash.SetActive(false);
        base.EnterState();
    }

    private void RequestLaunch()
    {
        _launchRequested = true;
    }

    public override void FixedUpdate()
    {
        if (_launchRequested)
        {
            // todo, make a different hand cannon for combined elements
            // if (handCannon.soloCannon && handCannon.blasterElement != ElementFlag.Electricity &&
            //     handCannon.blasterElement != ElementFlag.Fire && handCannon.blasterElement != ElementFlag.Water &&
            //     handCannon.blasterElement != ElementFlag.Wind && handCannon.blasterElement != ElementFlag.Rock)
            // {
            //     _beamObject = ElementPool.GetElement(handCannon.blasterElement, handCannon.barrelTransform.position);
            //     if (!_beamObject) return;
            //     _beam = true;
            //     _launchRequested = false;
            //     _beamObject.SetActive(true);
            //     return;
            // }

            var ball = ElementPool.GetElement(handCannon.blasterElement, handCannon.barrelTransform.position);
            LaunchDodgeball(ball);
            _launchRequested = false;
        }
    }

    private float fireRate => handCannon.FireRate;

    public override void Update()
    {
        base.Update();
        if (handCannon.fireTime > 0)
        {
            handCannon.fireTime -= Time.deltaTime;
        }
        else if (handCannon.state == CannonState.Shooting)
        {
            RequestLaunch();
            handCannon.fireTime = fireRate;
        }

        // todo, move this over to new class for combined hand cannon
        // _beamObject.transform.position = handCannon.barrelTransform.position;
        // _beamObject.transform.rotation = handCannon.barrelTransform.rotation;
        // _beamObject.transform.position += _controller.velocity.normalized * Time.deltaTime;
    }

    public override void FireReleaseAction()
    {
        base.FireReleaseAction();
        // todo move to combined class
        // if (_beamObject)
        // {
        //     _beamObject.SetActive(false);
        //     _beamObject = null;
        // }
        //
        // _beam = false;
        ChangeState(CannonState.Idle);
    }

    private void LaunchDodgeball(GameObject dodgeball)
    {
        handCannon.PlayOneShotAnimation();
        handCannon.muzzleFlash.SetActive(false);
        handCannon.muzzleFlash.SetActive(true);
        var rb = dodgeball.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        dodgeball.transform.position = handCannon.barrelTransform.position;
        dodgeball.transform.rotation = handCannon.barrelTransform.rotation;

        var projectile = dodgeball.GetComponent<Projectile>();
        projectile.isPlayerProjectile = true;
        projectile.damage = handCannon.actor.baseDamage;
        projectile.cannon = handCannon;
        projectile.effectiveDamage = handCannon.actor.FetchEffectiveElementalDamage(handCannon.blasterElement);

        dodgeball.SetActive(true);
        projectile.speed = 80;

        var launchVelocity = handCannon.barrelTransform.forward * handCannon.launchForce;
        launchVelocity += _controller.velocity;
        rb.velocity = launchVelocity;
        handCannon.actor.HapticFeedback(handCannon.handSide);
        handCannon.ReduceAmmuniton();
    }
}