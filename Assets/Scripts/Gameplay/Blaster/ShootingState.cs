using Gameplay;
using Gameplay.Enemies;
using UnityEngine;
using Util;

public class ShootingState : BaseHandCanonState
{
    private CharacterController _controller;
    private bool beam;
    private GameObject beamObject;
    private CharacterController controller;
    private bool launchRequested;

    public ShootingState(HandCannon handCannon) : base(handCannon)
    {
        controller = handCannon.actor.GetComponent<CharacterController>();
    }

    public override void EnterState()
    {
        if (beam || beamObject) return;
        handCannon.animator.Play("Fire");
        // todo, handle muzzle flash
        handCannon.muzzleFlash.SetActive(false);
        base.EnterState();
        RequestLaunch();
    }

    public override void ExitState()
    {
        base.ExitState();
        launchRequested = false;
    }


    private void RequestLaunch()
    {
        launchRequested = true;
    }

    public override void FixedUpdate()
    {
        if (launchRequested)
        {
            if (handCannon.soloCannon && handCannon.blasterElement != ElementFlag.Electricity &&
                handCannon.blasterElement != ElementFlag.Fire && handCannon.blasterElement != ElementFlag.Water &&
                handCannon.blasterElement != ElementFlag.Wind && handCannon.blasterElement != ElementFlag.Rock)
            {
                beamObject = ElementPool.GetElement(handCannon.blasterElement, handCannon.barrelTransform.position);
                if (!beamObject) return;
                beam = true;
                launchRequested = false;
                beamObject.SetActive(true);
                return;
            }

            var ball = ElementPool.GetElement(handCannon.blasterElement, handCannon.barrelTransform.position);
            LaunchDodgeball(ball);
            handCannon.muzzleFlash.SetActive(true);
            // handCannon.audioSource.PlayOneShot(ConfigurationManager.GetBlasterSound());
            launchRequested = false;
        }
    }

    private float fireRate = 0.5f;
    private float fireTime;

    public override void Update()
    {
        base.Update();
        if (!beam || !beamObject)
        {
            if (fireTime > 0)
            {
                fireTime -= Time.deltaTime;
            }
            else
            {
                RequestLaunch();
                fireTime = fireRate;
            }
            return;
        }

        beamObject.transform.position = handCannon.barrelTransform.position;
        beamObject.transform.rotation = handCannon.barrelTransform.rotation;
        beamObject.transform.position += controller.velocity.normalized * Time.deltaTime;
    }

    public override void FireReleaseAction()
    {
        base.FireReleaseAction();
        if (beamObject)
        {
            beamObject.SetActive(false);
            beamObject = null;
        }

        beam = false;
        ChangeState(CannonState.Idle);
    }

    private void LaunchDodgeball(GameObject dodgeball)
    {
        var rb = dodgeball.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        dodgeball.transform.position = handCannon.barrelTransform.position;
        dodgeball.transform.rotation = handCannon.barrelTransform.rotation;

        var projectile = dodgeball.GetComponent<Projectile>();
        projectile.isPlayerProjectile = true;
        projectile.damage = handCannon.actor.FetchDamage();
        projectile.effectiveDamage = handCannon.actor.FetchEffectiveElementalDamage(handCannon.blasterElement);
        dodgeball.SetActive(true);

        var launchVelocity = handCannon.barrelTransform.forward * handCannon.launchForce;
        launchVelocity += controller.velocity;

        rb.velocity = launchVelocity;
    }
}