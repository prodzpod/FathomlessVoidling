using BepInEx.Logging;
using RoR2;
using System.Linq;
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
  public class AimDisillusion : BaseState
  {
    private HurtBox target;
    // public static float baseDuration;
    // public static float arcMultiplier;
    private float stopwatch;
    private float duration;
    private Vector3? pointA;
    private Vector3? pointB;

    public override void OnEnter()
    {
      base.OnEnter();
      if (this.isAuthority)
      {
        BullseyeSearch bullseyeSearch = new BullseyeSearch();
        bullseyeSearch.viewer = this.characterBody;
        bullseyeSearch.searchOrigin = this.characterBody.corePosition;
        bullseyeSearch.searchDirection = this.characterBody.corePosition;
        bullseyeSearch.maxDistanceFilter = FirePortalBomb.maxDistance;
        bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(this.GetTeam());
        bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
        bullseyeSearch.RefreshCandidates();
        this.target = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
        if ((bool)(Object)this.target)
          this.pointA = this.RaycastToFloor(this.target.transform.position);
      }
      this.duration = AimPortalBomb.baseDuration;
    }
    public override void FixedUpdate()
    {
      this.stopwatch += Time.fixedDeltaTime;
      if (!this.isAuthority || (double)this.stopwatch < (double)this.duration)
        return;
      EntityState newNextState = (EntityState)null;
      if ((bool)(Object)this.target)
      {
        this.pointB = this.RaycastToFloor(this.target.transform.position);
        if (this.pointA.HasValue && this.pointB.HasValue)
        {
          Ray aimRay = this.GetAimRay();
          Vector3 forward1 = this.pointA.Value - aimRay.origin;
          Vector3 forward2 = this.pointB.Value - aimRay.origin;
          Quaternion a = Quaternion.LookRotation(forward1);
          Quaternion b = Quaternion.LookRotation(forward2);
          Quaternion quaternion1 = b;
          Quaternion quaternion2 = Quaternion.SlerpUnclamped(a, b, 1f + AimPortalBomb.arcMultiplier);
          newNextState = (EntityState)new Disillusion()
          {
            startRotation = new Quaternion?(quaternion1),
            endRotation = new Quaternion?(quaternion2)
          };
        }
      }
      if (newNextState != null)
        this.outer.SetNextState(newNextState);
      else
        this.outer.SetNextStateToMain();
    }
    private Vector3? RaycastToFloor(Vector3 position)
    {
      RaycastHit hitInfo;
      return Physics.Raycast(new Ray(position, Vector3.down), out hitInfo, 10f, (int)LayerIndex.world.mask, QueryTriggerInteraction.Ignore) ? new Vector3?(hitInfo.point) : new Vector3?();
    }
  }
}