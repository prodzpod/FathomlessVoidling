using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates;
using EntityStates.GrandParentBoss;
using EntityStates.VoidRaidCrab.Weapon;

namespace FathomlessVoidling
{
  public class Crush : BaseState
  {
    private float stopwatch;
    private float missileStopwatch;
    public static float baseDuration = 6;
    public static string muzzleString = BaseMultiBeamState.muzzleName;
    public static float missileSpawnFrequency = 6;
    public static float missileSpawnDelay = 0;
    public static float damageCoefficient;
    public static float maxSpread = 1;
    public static GameObject projectilePrefab;
    public static GameObject muzzleflashPrefab;
    private ChildLocator childLocator;
    static System.Random rand = new();

    public override void OnEnter()
    {
      base.OnEnter();
      this.missileStopwatch -= missileSpawnDelay;
      Transform modelTransform = this.GetModelTransform();
      if (!(bool)(Object)modelTransform)
        return;
      this.childLocator = modelTransform.GetComponent<ChildLocator>();
      if (!(bool)(Object)this.childLocator)
        return;
      int num = (bool)(Object)this.childLocator.FindChild(muzzleString) ? 1 : 0;
    }
    private void FireBlob(Ray projectileRay, float bonusPitch, float bonusYaw)
    {
      EffectManager.SpawnEffect(FathomlessVoidling.portal, new EffectData { origin = projectileRay.origin, rotation = Util.QuaternionSafeLookRotation(projectileRay.direction) }, false);
      FireProjectileInfo voidMeteor = new FireProjectileInfo()
      {
        position = projectileRay.origin,
        rotation = Util.QuaternionSafeLookRotation(projectileRay.direction),
        crit = Util.CheckRoll(this.critStat, this.characterBody.master),
        damage = this.damageStat * (new FireSecondaryProjectile().damageCoefficient * 2),
        owner = this.gameObject,
        force = (new FireSecondaryProjectile().force * 2),
        speedOverride = 100,
        projectilePrefab = FathomlessVoidling.meteor
      };
      ProjectileManager.instance.FireProjectile(voidMeteor);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      this.stopwatch += Time.fixedDeltaTime;
      this.missileStopwatch += Time.fixedDeltaTime;
      if ((double)this.missileStopwatch >= 1.0 / (double)missileSpawnFrequency)
      {
        this.missileStopwatch -= 1f / missileSpawnFrequency;
        Transform child = this.childLocator.FindChild(muzzleString);
        if ((bool)(Object)child)
        {
          Ray aimRay = this.GetAimRay();
          Ray projectileRay = new Ray();
          projectileRay.direction = aimRay.direction;
          float maxDistance = 1000f;
          float randX = UnityEngine.Random.Range(-100f, 100f);
          float randY = UnityEngine.Random.Range(50f, 75f);
          float randZ = UnityEngine.Random.Range(-100f, 100f);
          Vector3 randVector = new Vector3(randX, randY, randZ);
          Vector3 position = child.position + randVector;
          projectileRay.origin = position;
          RaycastHit hitInfo;
          {
            if (Physics.Raycast(aimRay, out hitInfo, maxDistance, (int)LayerIndex.world.mask))
              projectileRay.direction = hitInfo.point - projectileRay.origin;
            this.FireBlob(projectileRay, 0.0f, 0.0f);
          }
        }
        if ((double)this.stopwatch < (double)baseDuration || !this.isAuthority)
          return;
        this.outer.SetNextStateToMain();
      }
    }

    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
  }
}