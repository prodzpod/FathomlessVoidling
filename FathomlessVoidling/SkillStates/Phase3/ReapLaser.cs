using RoR2;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.VoidRaidCrab;
using EntityStates.Huntress;
using System;
using System.Linq;

namespace FathomlessVoidling
{
  public class ReapLaser : BaseSpinBeamAttackState
  {
    public int count;
    public Vector3 targetPosition;
    private Ray aimRay;
    private new float baseDuration = 2;
    private new float duration;
    private float beamTickTimer;
    private LoopSoundManager.SoundLoopPtr loopPtr;

    public override void OnEnter()
    {
      if (count > 3)
        this.outer.SetNextStateToMain();
      this.duration = this.baseDuration / this.attackSpeedStat;
      this.aimRay = this.GetAimRay();
      this.aimRay.direction = targetPosition - this.aimRay.origin;
      this.beamVfxInstance = UnityEngine.Object.Instantiate<GameObject>(SpinBeamAttack.beamVfxPrefab, this.aimRay.origin, Quaternion.LookRotation(this.aimRay.direction));
      this.loopPtr = LoopSoundManager.PlaySoundLoopLocal(this.gameObject, SpinBeamAttack.loopSound);
      int num = (int)Util.PlaySound(SpinBeamAttack.enterSoundString, this.gameObject);
    }

    public override void FixedUpdate()
    {
      //if ((double)this.fixedAge >= (double)this.duration && this.isAuthority)
      //this.outer.SetNextStateToMain();
      if (this.isAuthority)
      {
        if ((double)this.beamTickTimer <= 0.0)
        {
          this.beamTickTimer += 1f / SpinBeamAttack.beamTickFrequency;
          this.FireBeamBulletAuthority();
        }
        this.beamTickTimer -= Time.fixedDeltaTime;
      }
    }

    public override void OnExit()
    {
      LoopSoundManager.StopSoundLoopLocal(this.loopPtr);
      VfxKillBehavior.KillVfxObject(this.beamVfxInstance);
      this.beamVfxInstance = (GameObject)null;
      this.outer.SetNextState((EntityState)new Reap());
    }

    private void FireBeamBulletAuthority()
    {
      new BulletAttack()
      {
        origin = this.aimRay.origin,
        aimVector = this.aimRay.direction,
        minSpread = 0.0f,
        maxSpread = 0.0f,
        maxDistance = 400f,
        hitMask = LayerIndex.CommonMasks.bullet,
        stopperMask = ((LayerMask)0),
        bulletCount = 1U,
        radius = SpinBeamAttack.beamRadius,
        smartCollision = false,
        queryTriggerInteraction = QueryTriggerInteraction.Ignore,
        procCoefficient = 1f,
        procChainMask = new ProcChainMask(),
        owner = this.gameObject,
        weapon = this.gameObject,
        damage = (SpinBeamAttack.beamDpsCoefficient * this.damageStat / SpinBeamAttack.beamTickFrequency),
        damageColorIndex = DamageColorIndex.Default,
        damageType = DamageType.Generic,
        falloffModel = BulletAttack.FalloffModel.None,
        force = 0.0f,
        hitEffectPrefab = SpinBeamAttack.beamImpactEffectPrefab,
        tracerEffectPrefab = ((GameObject)null),
        isCrit = false,
        HitEffectNormal = false
      }.Fire();
    }

  }
}