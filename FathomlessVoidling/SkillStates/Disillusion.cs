using RoR2;
using RoR2.Navigation;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.NullifierMonster;
using EntityStates.BrotherMonster;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using EntityStates.BrotherMonster.Weapon;
using System.Collections.Generic;

namespace FathomlessVoidling
{
  public class Disillusion : BaseState
  {
    //public static GameObject portalBombProjectileEffect;
    //public static GameObject muzzleflashEffectPrefab;
    //public static string muzzleString;
    public static int portalBombCount;
    public static float baseDuration;
    public static float maxDistance;
    public static float damageCoefficient;
    public static float procCoefficient;
    public static float randomRadius;
    public static float force;
    public static float minimumDistanceBetweenBombs;
    public Quaternion? startRotation;
    public Quaternion? endRotation;
    private float duration;
    private int bombsFired;
    private float fireTimer;
    private float fireInterval;
    private Vector3 lastBombPosition;

    public override void OnEnter()
    {
      base.OnEnter();
      this.duration = FirePortalBomb.baseDuration / this.attackSpeedStat;
      this.StartAimMode(4f);
      if (!this.isAuthority)
        return;
      this.fireInterval = this.duration / (float)FirePortalBomb.portalBombCount;
      this.fireTimer = 0.0f;
    }

    private void FireBomb(Ray fireRay)
    {
      EffectManager.SimpleMuzzleFlash(new FireMissiles().muzzleFlashPrefab, this.gameObject, new FireMissiles().muzzleName, false);
      Util.PlayAttackSpeedSound(new FireMissiles().fireWaveSoundString, this.gameObject, this.attackSpeedStat);
      RaycastHit hitInfo;
      if (!Physics.Raycast(fireRay, out hitInfo, FirePortalBomb.maxDistance, (int)LayerIndex.world.mask))
        return;
      Vector3 point = hitInfo.point;
      Vector3 vector3 = point - this.lastBombPosition;
      if (this.bombsFired > 0 && (double)vector3.sqrMagnitude < (double)FirePortalBomb.minimumDistanceBetweenBombs * (double)FirePortalBomb.minimumDistanceBetweenBombs)
        point += vector3.normalized * FirePortalBomb.minimumDistanceBetweenBombs;
      Quaternion quaternion = Util.QuaternionSafeLookRotation(this.GetAimRay().direction);
      FireProjectileInfo fireProjectileInfo = new FireProjectileInfo()
      {
        projectilePrefab = new FireMissiles().projectilePrefab,
        position = this.FindModelChild(new FireMissiles().muzzleName).position,
        owner = this.gameObject,
        damage = this.damageStat * new FireMissiles().damageCoefficient,
        force = new FireMissiles().force
      };
      for (int index = 0; index < new FireMissiles().numMissilesPerWave; ++index)
      {
        fireProjectileInfo.rotation = quaternion * new FireMissiles().GetRandomRollPitch();
        fireProjectileInfo.crit = Util.CheckRoll(this.critStat, this.characterBody.master);
        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
      }
      ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
      {
        projectilePrefab = FathomlessVoidling.deathBomb,
        position = point,
        rotation = Quaternion.identity,
        owner = this.gameObject,
        damage = new EntityStates.VoidMegaCrab.DeathState().damageStat,
        crit = this.characterBody.RollCrit()
      });
      this.lastBombPosition = point;
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if (!this.isAuthority)
        return;
      this.fireTimer -= Time.fixedDeltaTime;
      if ((double)this.fireTimer <= 0.0)
      {
        this.fireTimer += this.fireInterval;
        if (this.startRotation.HasValue && this.endRotation.HasValue)
        {
          float t = (float)this.bombsFired * (float)(1.0 / ((double)FirePortalBomb.portalBombCount - 1.0));
          Ray aimRay = this.GetAimRay();
          Quaternion quaternion = Quaternion.Slerp(this.startRotation.Value, this.endRotation.Value, t);
          aimRay.direction = quaternion * Vector3.forward;
          this.FireBomb(aimRay);
          EffectManager.SimpleMuzzleFlash(FirePortalBomb.muzzleflashEffectPrefab, this.gameObject, new FireMissiles().muzzleName, true);
        }
        ++this.bombsFired;
      }
      if ((double)this.fixedAge < (double)this.duration)
        return;
      this.outer.SetNextStateToMain();
    }
  }
}