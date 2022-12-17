using RoR2;
using System.Linq;
using RoR2.Navigation;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.TitanMonster;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.NullifierMonster;
using EntityStates.BrotherMonster;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using EntityStates.BrotherMonster.Weapon;
using System.Collections.Generic;

namespace FathomlessVoidling
{
  public class Disillusion : FireFist
  {
    //public static GameObject portalBombProjectileEffect;
    //public static GameObject muzzleflashEffectPrefab;
    //public static string muzzleString;
    public static int portalBombCount;
    public static float baseDuration = 2f;
    public static float damageCoefficient;
    public static float procCoefficient;
    public static float randomRadius;
    public static float force;
    public static float minimumDistanceBetweenBombs;
    private GameObject deathBombPrefab;
    private float duration;
    private int bombsFired;
    private float fireTimer;
    private float fireInterval;
    private Vector3 lastBombPosition;

    public override void OnEnter()
    {
      base.OnEnter();
      this.duration = Disillusion.baseDuration / this.attackSpeedStat;
      switch (this.characterBody.name)
      {
        case "MiniVoidRaidCrabBodyPhase1(Clone)":
          deathBombPrefab = FathomlessVoidling.deathBomb;
          break;
        case "MiniVoidRaidCrabBodyPhase2(Clone)":
          deathBombPrefab = FathomlessVoidling.deathBomb2;
          break;
        case "MiniVoidRaidCrabBodyPhase3(Clone)":
          deathBombPrefab = FathomlessVoidling.deathBomb3;
          break;
      }
      this.StartAimMode(4f);
      if (!this.isAuthority)
        return;
      this.fireInterval = this.duration / (float)FirePortalBomb.portalBombCount;
      this.fireTimer = 0.0f;
      BullseyeSearch bullseyeSearch = new BullseyeSearch();
      bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
      if ((bool)(Object)this.teamComponent)
        bullseyeSearch.teamMaskFilter.RemoveTeam(this.teamComponent.teamIndex);
      bullseyeSearch.maxDistanceFilter = 1000;
      bullseyeSearch.maxAngleFilter = 360f;
      Ray aimRay = this.GetAimRay();
      bullseyeSearch.searchOrigin = aimRay.origin;
      bullseyeSearch.searchDirection = aimRay.direction;
      bullseyeSearch.filterByLoS = false;
      bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
      bullseyeSearch.RefreshCandidates();
      HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
      if (!(bool)(Object)hurtBox)
        return;
      this.predictor = new Disillusion.Predictor(this.transform);
      this.predictor.SetTargetTransform(hurtBox.transform);
    }

    private void FireBomb(Vector3 point)
    {
      ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
      {
        projectilePrefab = FathomlessVoidling.deathBomb,
        position = point,
        rotation = Quaternion.identity,
        owner = this.gameObject,
        damage = new EntityStates.VoidMegaCrab.DeathState().damageStat,
        crit = this.characterBody.RollCrit()
      });
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
        EffectManager.SimpleMuzzleFlash(new FireMissiles().muzzleFlashPrefab, this.gameObject, new FireMissiles().muzzleName, false);
        Util.PlayAttackSpeedSound(new FireMissiles().fireWaveSoundString, this.gameObject, this.attackSpeedStat);
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
        if (this.predictor != null)
        {
          this.predictionOk = this.predictor.GetPredictedTargetPosition(1, out this.predictedTargetPosition);
          if (this.predictionOk && (bool)(Object)this.predictorDebug)
            this.predictorDebug.transform.position = this.predictedTargetPosition;
          this.FireBomb(new Vector3(this.predictedTargetPosition.x, this.predictor.targetTransform.position.y, this.predictedTargetPosition.z));
        }
        EffectManager.SimpleMuzzleFlash(FirePortalBomb.muzzleflashEffectPrefab, this.gameObject, new FireMissiles().muzzleName, true);
        ++this.bombsFired;
      }
      if ((double)this.fixedAge < (double)this.duration)
        return;
      this.outer.SetNextStateToMain();
    }
  }
}