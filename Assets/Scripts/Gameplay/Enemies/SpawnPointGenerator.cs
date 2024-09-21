using System.Linq;
using Gameplay.Enemies;
using UnityEngine;

// ~ needs test
// * finished

// ordering todo in terms of importance/ease of access and rapid changing
// ordered todo:
// *UI/Damage Numbers - easy to start
// *Enemies/Resizing - easy to start - need to edit spawn values
// *UI/Upgrade menu - easy to improve
// *UI/Player Vignette on Hurt
// *Projectile/Collisions - Large Improvement - robot projectile went all wonky on us once for some reason
// ===Testing the above before making big change below===

// *Projectile/Regular - element.none projectiles
// ===test the projectiles since large changes were made. If alles klar, finish up player with powerup mechanic===

// *Player/Elements - Power-up mechanic, large improvement
// *Gameplay/Balance - Large improvement, balance out the power ups
// *Enemies/Balance - Large improvement
// *fire rate offset to make each hand shoot alternating
// ===Test the balance to ensure power ups feel rewarding and the game is challenging===

// Enemies/Shooting - QoL
// Enemies/Movement Patterns - Large improvement
// ===Test to ensure we tightened up the enemies and polish them off to complete their mechanics and have a solid enemy system==


// here we are! Do the combined elements and ammunition next for the gameloop testing

// ---Need animation blending *Unseen blocker*-----
// Finish animation library, if needed (see work flow)
// if shader/vfx draw calls are hitting us, create vfx workflow in new render system
// ------------------------------------------------
// finished above, will implement after the gameplay elements are in place

// Elemental Ammunition: Element Ammo Count


// Combined: Validate combined elements are working to tie off all feature sets
// Implement Single Blaster artwork

// ===Test the combined elements with the power ups and ensure the game is sound. Ship to play test here===

// Scripted events
// boss battle
// 

// ~~~Polish passing and visual fidelity/cohesion~~~
// Rendering: VFX for Enemies
// Rendering: VFX for Blasters/Transitions

// UI/Starting the game
// Gameplay/Start
// UI/Score Display
// Gameplay/Exit

// Sound dev and polish passes
// here should be finished with the game, but apply any feedback gathered in the ship to play test
// if it's good, while we wait for dan to finish up the artwork, start working on building out VR Game Engine design
// and a good tech demo showcase in unity. This will allow us to have a good demo to show off to potential investors

// ======= *Rendering* ========
// VFX for enemies
// Blaster Transition: Vfx, animation, sfx for transitioning between elements
// Shader library to handle rendering and all the diff effects and such. (abstraction)
// Animation restructuring for scalability:
// Create better animation system
// Apply it to enemies more abstractly and reuse functionality
// Take Blaster into account too
// account for all shader animations

// game list to do;
// ======= *Enemies* ========
// Shooting: Check LoS to player before shooting, if no LoS, move to a new position (generation of pos important here)
// Balance: spawning dynamically rather than batching. This will allow us to control for powerups better.
// Balance: make the waves ramp up a bit slower, we need to make them start out taking a bit less damage.
// Balance: Change the attack range/cool downs for new adjustments, tank/grunt shooting, ect.
// Movement Patterns: Make tiered patterns for waves.
// Movement Patterns: Support a node based movement pattern for swarms to overwhelm the player from a flank.
// Visuals: spawning the enemies in, we need to apply the vfx and a better spawn area for us to work with
// Visuals: Implement better animations for hitting, they should not loop over themselves
// Visuals: Implement dying FX for the enemies

// ======= *UI* ==========
// *Damage Numbers: Here and there the glyph offset is distorted. Find out why the numbers are showing text other than a number
// Scoring: End of game needs a score tweening menu so we can see our score and give oomphs
// *Starting the game: We need to make the cube make sense in the environment
// *Upgrade menu: Make the upgrade menu more clear what they do, and test whether we want it in the game or not
// Player Vignette on Hurt

// ======= *Projectiles* ========
// *Collisions: Make the collision system handle better. The projectiles should be able to collide mid air, but should
// not be able to collide with the respective party. (enemies projectiles can't collide with enemies, player projectiles can't collide with player)
// *Combined: Validate combined elements are working.
// *Regular Projectile: Support the ElementFlag.None for the blaster. This will allow us to have a standard projectile

// ======= *Player* =========
// *Elements: Power-up mechanic to switch the blaster with an element when shot
// *Elements: Element Ammo Count
// *Elements: Element Ammo UI/display on blaster like health bar(?)

// ======= *Gameplay* =========
// *Start: Start on an elevator with 2 plain blasters, for now.
// *Start: intro, moving elevator, door slam, platform moving. how we start?
// *Exit: exit game sequence: outro. Boom all enemies out, things get lighter (darker?) and the menu tweening happens. afterwards, menu screen appears again.
// *Balance: better/more controlled wave timings, accounting for elements/enemy types better
// *Balance: power-up probability distribution for the player
// *Balance: enemy spawning distribution for elements based on element upgrades

// ======= Audio =========

// SFX: shooting (each element and plain + combined)
// SFX: player taking damage
// SFX: player dying
// SFX: player getting a power-up
// SFX: Robots own sounds (tank, grunt, glass cannon, swarm)
// SFX: Robots getting hit
// SFX: Robots dying (black hole)
// SFX: Robots spawning in (teleportation)
// SFX: Game Starting
// SFX: Game Ending/Over
// SFX: Score menu tween
// SFX: Elevator scene/door slam
// SFX: Intro sequence (?)

// Music: Intro
// Music: Cut into gameplay intensity1
// Music: Intensity layering dynamically based on wave/enemies
// Music: GameOver/Score menu tweening screen


// Entrance Behaviors:
// Each enemy should have a sub-set of entrances
// We can either randomly choose between them, or we can use more difficult ones later

// entrance:
// tank/grunt, beam in
// tank, every once and a while, slam down onto the stage
// swarm, come in from sides or below the stage
// glass cannon, comes in from below the stage, zips in like he does now (but as entry)

// Below Stage Spawning area: Swarm/Glass Cannon
// Side Stage Areas: Swarms (less often)
// Beaming Into Play: Grunt/Tank
// Slamming ont stage: Tank
// Zip in: Glass Cannon (already there, less often earlier, more later)

// Below Stage Spawning Area (and side stage)
// pick below stage area randomly,
// bezier curve control point in control area
// generate random bezier, start->control->'landing'
// landing would be the flying height we have now

// Beaming into play
// make vfx phase in of the character before allowing any damage or behavior
// make the vfx phase really quick
// pick a random point in the spawning area


public static class SpawnPointGenerator
{
    private static readonly EnemyType[] _SFlyingUnitTypes = {EnemyType.Swarm, EnemyType.GlassCannon};

    public static Vector3 GenerateSingleSpawnPoint(Transform centralPoint, EnemyType enemyType, float spawnRadius)
    {
        var randomDivisor = Mathf.Max(1, Random.Range(1, 180));

        var angleStep = 360f / randomDivisor;
        var angle = angleStep + Random.Range(-angleStep / 2, angleStep / 2);

        var x = Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius;
        var z = Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius;

        var isFlying = IsFlyingUnit(enemyType);
        var y = isFlying ? Random.Range(6.5f, 8.5f) : GetValidSpawnHeight(enemyType);

        return centralPoint.position + new Vector3(x, y, z);
    }


    // todo, we need to make this class actually spawn an enemy based on the probabilities,
    // not populate a map. This will allow us to get the render size and snap them to the grid
    // we can use a collider to get the bounds, and the enemy pool returning an enemy could 
    // allow us to make an enemy helper function (get bound bottom) to snap them to the ground
    private static float GetValidSpawnHeight(EnemyType type)
    {
        if (type == EnemyType.Tank) return 1.75f;
        return 1.25f;
    }

    private static bool IsFlyingUnit(EnemyType type) =>
        _SFlyingUnitTypes.Contains(type);
}