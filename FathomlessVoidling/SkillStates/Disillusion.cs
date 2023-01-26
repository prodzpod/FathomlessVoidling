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
  public class Disillusion : BaseState
  {
    //public static GameObject portalBombProjectileEffect;
    //public static GameObject muzzleflashEffectPrefab;
    //public static string muzzleString;
    public static int portalBombCount;
    public static float baseDuration = 4f;
    public static float damageCoefficient;
    public static float procCoefficient;
    public static float randomRadius;
    public static float force;
    public static float minimumDistanceBetweenBombs;
    private GameObject bombPrefab;
    private float duration;
    private int bombsFired;
    private float fireTimer;
    private float stopwatch;
    private float fireInterval;
    private Vector3 lastBombPosition;
    private Vector3 predictedTargetPosition;
    private Predictor predictor;

    public override void OnEnter()
    {
      base.OnEnter();
      this.duration = Disillusion.baseDuration / this.attackSpeedStat;
      switch (this.characterBody.name)
      {
        case "MiniVoidRaidCrabBodyPhase1(Clone)":
          bombPrefab = FathomlessVoidling.bombPrefab1;
          break;
        case "MiniVoidRaidCrabBodyPhase2(Clone)":
          bombPrefab = FathomlessVoidling.bombPrefab2;
          break;
        case "MiniVoidRaidCrabBodyPhase3(Clone)":
          bombPrefab = FathomlessVoidling.bombPrefab3;
          break;
      }
      this.fireInterval = this.duration / 5;
      this.fireTimer = 0.0f;
      this.stopwatch = 0.0f;
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
      bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
      bullseyeSearch.RefreshCandidates();
      HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
      if (!(bool)(Object)hurtBox)
        return;
      this.predictor = new Predictor(this.transform);
      this.predictor.SetTargetTransform(hurtBox.transform);
    }

    private void FireBomb(Vector3 point)
    {
      ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
      {
        projectilePrefab = bombPrefab,
        position = point,
        rotation = Quaternion.identity,
        owner = this.gameObject,
        damage = 0,
        force = 0,
        crit = this.characterBody.RollCrit()
      });
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      // Predictive Death Bombs
      this.stopwatch += Time.fixedDeltaTime;
      predictor.Update();
      if ((double)this.stopwatch <= (double)1f)
        predictor.GetPredictedTargetPosition(1f, out predictedTargetPosition);
      else
      {
        this.stopwatch = 0f;
        FireBomb(predictedTargetPosition);
      }
      // Missile Firing
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
        EffectManager.SimpleMuzzleFlash(FirePortalBomb.muzzleflashEffectPrefab, this.gameObject, new FireMissiles().muzzleName, true);
        ++this.bombsFired;
      }
      if ((double)this.fixedAge < (double)this.duration)
        return;
      this.outer.SetNextStateToMain();
    }

    private class Predictor
    {
      public Ray aimRay;
      private Transform bodyTransform;
      private Transform targetTransform;
      private Vector3 targetPosition0;
      private Vector3 targetPosition1;
      private Vector3 targetPosition2;
      private int collectedPositions;

      public Predictor(Transform bodyTransform) => this.bodyTransform = bodyTransform;

      public bool hasTargetTransform => (bool)(Object)this.targetTransform;

      public bool isPredictionReady => this.collectedPositions > 2;

      private void PushTargetPosition(Vector3 newTargetPosition)
      {
        this.targetPosition2 = this.targetPosition1;
        this.targetPosition1 = this.targetPosition0;
        this.targetPosition0 = newTargetPosition;
        ++this.collectedPositions;
      }

      public void SetTargetTransform(Transform newTargetTransform)
      {
        this.targetTransform = newTargetTransform;
        this.targetPosition2 = this.targetPosition1 = this.targetPosition0 = newTargetTransform.position;
        this.collectedPositions = 1;
      }

      public void Update()
      {
        if (!(bool)(Object)this.targetTransform)
          return;
        this.PushTargetPosition(this.targetTransform.position);
      }

      public bool GetPredictedTargetPosition(float time, out Vector3 predictedPosition)
      {
        Vector3 vector3_1 = this.targetPosition1 - this.targetPosition2;
        Vector3 vector3_2 = this.targetPosition0 - this.targetPosition1;
        vector3_1.y = 0.0f;
        vector3_2.y = 0.0f;
        Predictor.ExtrapolationType extrapolationType = vector3_1 == Vector3.zero || vector3_2 == Vector3.zero ? Predictor.ExtrapolationType.None : ((double)Vector3.Dot(vector3_1.normalized, vector3_2.normalized) <= 0.980000019073486 ? Predictor.ExtrapolationType.Polar : Predictor.ExtrapolationType.Linear);
        float num1 = 1f / Time.fixedDeltaTime;
        predictedPosition = this.targetPosition0;
        switch (extrapolationType)
        {
          case Predictor.ExtrapolationType.Linear:
            predictedPosition = this.targetPosition0 + vector3_2 * (time * num1);
            break;
          case Predictor.ExtrapolationType.Polar:
            Vector3 position = this.bodyTransform.position;
            Vector3 vector2Xy1 = (Vector3)Util.Vector3XZToVector2XY(this.targetPosition2 - position);
            Vector3 vector2Xy2 = (Vector3)Util.Vector3XZToVector2XY(this.targetPosition1 - position);
            Vector3 vector2Xy3 = (Vector3)Util.Vector3XZToVector2XY(this.targetPosition0 - position);
            float magnitude1 = vector2Xy1.magnitude;
            float magnitude2 = vector2Xy2.magnitude;
            float magnitude3 = vector2Xy3.magnitude;
            float num2 = Vector2.SignedAngle((Vector2)vector2Xy1, (Vector2)vector2Xy2) * num1;
            float num3 = Vector2.SignedAngle((Vector2)vector2Xy2, (Vector2)vector2Xy3) * num1;
            double num4 = ((double)magnitude2 - (double)magnitude1) * (double)num1;
            float num5 = (magnitude3 - magnitude2) * num1;
            float num6 = (float)(((double)num2 + (double)num3) * 0.5);
            double num7 = (double)num5;
            float num8 = (float)((num4 + num7) * 0.5);
            float num9 = magnitude3 + num8 * time;
            if ((double)num9 < 0.0)
              num9 = 0.0f;
            Vector2 vector2 = Util.RotateVector2((Vector2)vector2Xy3, num6 * time) * (num9 * magnitude3);
            predictedPosition = position;
            predictedPosition.x += vector2.x;
            predictedPosition.z += vector2.y;
            break;
        }
        return true;
      }
      private enum ExtrapolationType
      {
        None,
        Linear,
        Polar,
      }
    }
  }
}