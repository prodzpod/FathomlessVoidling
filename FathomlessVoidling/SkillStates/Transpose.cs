using RoR2;
using RoR2.Projectile;
using RoR2.Navigation;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.ImpBossMonster;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.BrotherMonster;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using EntityStates.BrotherMonster.Weapon;
using System.Collections.Generic;

namespace FathomlessVoidling
{
  public class Transpose : BlinkState
  {
    public override void OnEnter()
    {
      int num = (int)Util.PlaySound(this.beginSoundString, this.gameObject);
      this.modelTransform = this.GetModelTransform();
      if ((bool)(Object)this.modelTransform)
      {
        this.animator = this.modelTransform.GetComponent<Animator>();
        this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
        this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
      }
      if (this.disappearWhileBlinking)
      {
        if ((bool)(Object)this.characterModel)
          ++this.characterModel.invisibilityCount;
        if ((bool)(Object)this.hurtboxGroup)
          ++this.hurtboxGroup.hurtBoxesDeactivatorCounter;
      }
      if ((bool)(Object)this.characterMotor)
        this.characterMotor.enabled = false;
      this.gameObject.layer = LayerIndex.fakeActor.intVal;
      // this.characterMotor.Motor.RebuildCollidableLayers();
      this.CalculateBlinkDestination();
      this.CreateBlinkEffect(Util.GetCorePosition(this.gameObject));
    }

    private void CalculateBlinkDestination()
    {
      Vector3 vector3 = Vector3.zero;
      Ray aimRay = this.GetAimRay();
      BullseyeSearch bullseyeSearch = new BullseyeSearch();
      bullseyeSearch.searchOrigin = aimRay.origin;
      bullseyeSearch.searchDirection = aimRay.direction;
      bullseyeSearch.maxDistanceFilter = this.blinkDistance;
      bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
      bullseyeSearch.filterByLoS = false;
      bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(this.gameObject));
      bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
      bullseyeSearch.RefreshCandidates();
      HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
      if ((bool)(Object)hurtBox)
        vector3 = hurtBox.transform.position - this.transform.position;
      this.blinkDestination = this.transform.position;
      this.blinkStart = this.transform.position;
      NodeGraph groundNodes = SceneInfo.instance.groundNodes;
      groundNodes.GetNodePosition(groundNodes.FindClosestNode(this.transform.position + vector3, this.characterBody.hullClassification), out this.blinkDestination);
      this.blinkDestination += this.transform.position - this.characterBody.footPosition;
    }

    private void CreateBlinkEffect(Vector3 origin)
    {
      if (!(bool)(Object)this.blinkPrefab)
        return;
      EffectManager.SpawnEffect(this.blinkPrefab, new EffectData()
      {
        rotation = Util.QuaternionSafeLookRotation(this.blinkDestination - this.blinkStart),
        origin = origin
      }, false);
    }

    private void SetPosition(Vector3 newPosition)
    {
      if (!(bool)(Object)this.characterMotor)
        return;
      this.characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity);
    }

    public override void FixedUpdate()
    {
      if ((bool)(Object)this.characterMotor)
        this.characterMotor.velocity = Vector3.zero;
      if (!this.hasBlinked)
        this.SetPosition(Vector3.Lerp(this.blinkStart, this.blinkDestination, this.fixedAge / this.duration));
      if ((double)this.fixedAge >= (double)this.duration - (double)this.destinationAlertDuration && !this.hasBlinked)
      {
        this.hasBlinked = true;
        if ((bool)(Object)this.blinkDestinationPrefab)
        {
          this.blinkDestinationInstance = Object.Instantiate<GameObject>(this.blinkDestinationPrefab, this.blinkDestination, Quaternion.identity);
          this.blinkDestinationInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = this.destinationAlertDuration;
        }
        this.SetPosition(this.blinkDestination);
      }
      if ((double)this.fixedAge >= (double)this.duration)
        this.ExitCleanup();
      if ((double)this.fixedAge < (double)this.duration + (double)this.exitDuration || !this.isAuthority)
        return;
      this.outer.SetNextStateToMain();
    }

    private void ExitCleanup()
    {
      if (this.isExiting)
        return;
      this.isExiting = true;
      this.gameObject.layer = LayerIndex.defaultLayer.intVal;
      this.characterMotor.Motor.RebuildCollidableLayers();
      int num = (int)Util.PlaySound(this.endSoundString, this.gameObject);
      this.CreateBlinkEffect(Util.GetCorePosition(this.gameObject));
      this.modelTransform = this.GetModelTransform();
      if ((double)this.blastAttackDamageCoefficient > 0.0)
        new BlastAttack()
        {
          attacker = this.gameObject,
          inflictor = this.gameObject,
          teamIndex = TeamComponent.GetObjectTeam(this.gameObject),
          baseDamage = (this.damageStat * this.blastAttackDamageCoefficient),
          baseForce = this.blastAttackForce,
          position = this.blinkDestination,
          radius = this.blastAttackRadius,
          falloffModel = BlastAttack.FalloffModel.Linear,
          attackerFiltering = AttackerFiltering.NeverHitSelf
        }.Fire();
      if (this.disappearWhileBlinking)
      {
        if ((bool)(Object)this.modelTransform && (bool)(Object)this.destealthMaterial)
        {
          TemporaryOverlay temporaryOverlay = this.animator.gameObject.AddComponent<TemporaryOverlay>();
          temporaryOverlay.duration = 1f;
          temporaryOverlay.destroyComponentOnEnd = true;
          temporaryOverlay.originalMaterial = this.destealthMaterial;
          temporaryOverlay.inspectorCharacterModel = this.animator.gameObject.GetComponent<CharacterModel>();
          temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0.0f, 1f, 1f, 0.0f);
          temporaryOverlay.animateShaderAlpha = true;
        }
        if ((bool)(Object)this.characterModel)
          --this.characterModel.invisibilityCount;
        if ((bool)(Object)this.hurtboxGroup)
          --this.hurtboxGroup.hurtBoxesDeactivatorCounter;
      }
      if ((bool)(Object)this.blinkDestinationInstance)
        EntityState.Destroy((Object)this.blinkDestinationInstance);
      if (!(bool)(Object)this.characterMotor)
        return;
      this.characterMotor.enabled = true;
    }

    public override void OnExit()
    {
      this.ExitCleanup();
    }
  }
}