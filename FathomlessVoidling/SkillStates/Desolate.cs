using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.BrotherMonster;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using EntityStates.BrotherMonster.Weapon;
using System.Collections.Generic;

namespace FathomlessVoidling
{
  public class Desolate : BaseState
  {
    private float stopwatch;
    private float missileStopwatch;
    public float baseDuration = 4;
    public static string muzzleString = BaseMultiBeamState.muzzleName;
    public static float missileSpawnFrequency = 6;
    public static float missileSpawnDelay = 0;
    public static float damageCoefficient;
    public static float maxSpread = 1;
    public static float blastDamageCoefficient = 1;
    public static float blastForceMagnitude = 3000;
    public static float blastRadius = 6;
    public static Vector3 blastBonusForce = new Vector3(0.0f, 100.0f, 0.0f);
    public static string muzzleName = "EyeProjectileCenter";
    public static string animationLayerName = "Gesture";
    public static string animationStateName = "FireMultiBeamFinale";
    public static string animationPlaybackRateParam = "MultiBeam.playbackRate";
    public static GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/TracerVoidRaidCrabTripleBeamSmall.prefab").WaitForCompletion();
    public static GameObject explosionEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamExplosion.prefab").WaitForCompletion();
    public static GameObject muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamMuzzleflash.prefab").WaitForCompletion();
    private float duration;
    private static System.Random rand = new();

    protected Transform muzzleTransform { get; private set; }

    // Play_voidRaid_snipe_shoot_final
    public override void OnEnter()
    {
      base.OnEnter();
      this.missileStopwatch -= missileSpawnDelay;
      this.duration = this.baseDuration / this.attackSpeedStat;
      this.muzzleTransform = this.FindModelChild(Desolate.muzzleName);
    }
    private void FireBlob()
    {
      this.PlayAnimation(Desolate.animationLayerName, Desolate.animationStateName, Desolate.animationPlaybackRateParam, this.duration);
      int num2 = (int)Util.PlaySound(new EntityStates.VoidRaidCrab.Weapon.FireMultiBeamSmall().enterSoundString, this.gameObject);
      Ray beamRay;
      Vector3 beamEndPos;
      this.CalcBeamPath(out beamRay, out beamEndPos);
      beamEndPos += new Vector3(UnityEngine.Random.Range(-15f, 15f), UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-15f, 15f));
      new BlastAttack()
      {
        attacker = this.gameObject,
        inflictor = this.gameObject,
        teamIndex = TeamComponent.GetObjectTeam(this.gameObject),
        baseDamage = this.damageStat * (Desolate.blastDamageCoefficient / 2),
        baseForce = Desolate.blastForceMagnitude,
        position = beamEndPos,
        radius = Desolate.blastRadius,
        falloffModel = BlastAttack.FalloffModel.SweetSpot,
        bonusForce = Desolate.blastBonusForce,
        damageType = DamageType.Generic
      }.Fire();
      Transform modelTransform = this.GetModelTransform();
      if ((bool)(Object)modelTransform)
      {
        ChildLocator component = modelTransform.GetComponent<ChildLocator>();
        if ((bool)(Object)component)
        {
          int childIndex = component.FindChildIndex(BaseMultiBeamState.muzzleName);
          if ((bool)(Object)Desolate.tracerEffectPrefab)
          {
            EffectData effectData = new EffectData()
            {
              origin = beamEndPos,
              start = beamRay.origin,
              scale = Desolate.blastRadius
            };
            effectData.SetChildLocatorTransformReference(this.gameObject, childIndex);
            EffectManager.SpawnEffect(Desolate.tracerEffectPrefab, effectData, true);
            EffectManager.SpawnEffect(Desolate.explosionEffectPrefab, effectData, true);
          }
        }
      }
      this.OnFireBeam(beamRay.origin, beamEndPos);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      this.stopwatch += Time.fixedDeltaTime;
      this.missileStopwatch += Time.fixedDeltaTime;
      if ((double)this.missileStopwatch >= 1.0 / (double)missileSpawnFrequency)
      {
        this.missileStopwatch -= 1f / missileSpawnFrequency;
        this.FireBlob();
        if ((double)this.stopwatch < (double)this.baseDuration || !this.isAuthority)
          return;
        this.outer.SetNextStateToMain();
      }
    }

    public void OnFireBeam(Vector3 beamStart, Vector3 beamEnd)
    {
      ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
      {
        projectilePrefab = new FireMultiBeamFinale().projectilePrefab,
        position = beamEnd + Vector3.up * new FireMultiBeamFinale().projectileVerticalSpawnOffset,
        owner = this.gameObject,
        damage = this.damageStat * (new FireMultiBeamFinale().projectileDamageCoefficient / 6),
        crit = Util.CheckRoll(this.critStat, this.characterBody.master)
      });
    }

    public void CalcBeamPath(out Ray beamRay, out Vector3 beamEndPos)
    {
      Ray aimRay = this.GetAimRay();
      float a = float.PositiveInfinity;
      RaycastHit[] raycastHitArray = Physics.RaycastAll(aimRay, BaseMultiBeamState.beamMaxDistance, (int)LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore);
      Transform root = this.GetModelTransform().root;
      for (int index = 0; index < raycastHitArray.Length; ++index)
      {
        ref RaycastHit local = ref raycastHitArray[index];
        float distance = local.distance;
        if ((double)distance < (double)a && local.collider.transform.root != root)
          a = distance;
      }
      float distance1 = Mathf.Min(a, BaseMultiBeamState.beamMaxDistance);
      beamEndPos = aimRay.GetPoint(distance1);
      Vector3 position = this.muzzleTransform.position;
      beamRay = new Ray(position, beamEndPos - position);
    }

  }
}