using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.VoidRaidCrab;
using EntityStates.Huntress;
using System;
using System.Linq;

namespace FathomlessVoidling
{
  public class Reap : BaseState
  {
    public static int portalBombCount;
    public static float baseDuration = 4f;
    private float duration;
    private float stopwatch;
    private float beamTickTimer = 0f;
    private Vector3 predictedTargetPosition;
    private GameObject predictiveLaserIndicator;
    private int counter = 0;
    private Predictor predictor;
    private static GameObject areaIndicatorPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressArrowRainIndicator.prefab").WaitForCompletion();

    public override void OnEnter()
    {
      base.OnEnter();
      counter += 1;
      predictiveLaserIndicator = UnityEngine.Object.Instantiate<GameObject>(areaIndicatorPrefab);
      predictiveLaserIndicator.transform.localScale = new Vector3(ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius);
      this.duration = Reap.baseDuration / this.attackSpeedStat;
      this.stopwatch = 0.0f;
      BullseyeSearch bullseyeSearch = new BullseyeSearch();
      bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
      if ((bool)(UnityEngine.Object)this.teamComponent)
        bullseyeSearch.teamMaskFilter.RemoveTeam(this.teamComponent.teamIndex);
      bullseyeSearch.maxDistanceFilter = 500;
      bullseyeSearch.maxAngleFilter = 360f;
      Ray aimRay = this.GetAimRay();
      bullseyeSearch.searchOrigin = aimRay.origin;
      bullseyeSearch.searchDirection = aimRay.direction;
      bullseyeSearch.filterByLoS = false;
      bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
      bullseyeSearch.RefreshCandidates();
      HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
      if (!(bool)(UnityEngine.Object)hurtBox)
        return;
      this.predictor = new Predictor(this.transform);
      this.predictor.SetTargetTransform(hurtBox.transform);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      // Predictive Lasers
      this.stopwatch += Time.fixedDeltaTime;
      predictor.Update();
      if ((double)this.stopwatch <= (double)1f)
      {
        predictor.GetPredictedTargetPosition(1f, out predictedTargetPosition);
        predictiveLaserIndicator.transform.position = predictedTargetPosition;

      }
      else
      {
        this.stopwatch = 0f;
        predictiveLaserIndicator.transform.position = new Vector3(0f, 0f, 0f);
        this.outer.SetNextState((EntityState)new ReapLaser() { count = counter });
      }
      if ((double)this.fixedAge < (double)this.duration)
        return;
      this.outer.SetNextStateToMain();
    }

    private class Predictor
    {
      private Transform bodyTransform;
      private Transform targetTransform;
      private Vector3 targetPosition0;
      private Vector3 targetPosition1;
      private Vector3 targetPosition2;
      private int collectedPositions;

      public Predictor(Transform bodyTransform) => this.bodyTransform = bodyTransform;

      public bool hasTargetTransform => (bool)(UnityEngine.Object)this.targetTransform;

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
        if (!(bool)(UnityEngine.Object)this.targetTransform)
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