using RoR2;
using RoR2.Navigation;
using UnityEngine;
using System.Linq;
using EntityStates;
using EntityStates.ImpBossMonster;
using EntityStates.VoidRaidCrab.Weapon;

namespace FathomlessVoidling
{
  public class Transpose : BlinkState
  {
    new public int blinkDistance = 500;
    public new float duration = 2f;
    private float stopwatch;

    public override void OnEnter()
    {
      this.modelTransform = this.GetModelTransform();
      if ((bool)(Object)this.modelTransform)
      {
        this.animator = this.modelTransform.GetComponent<Animator>();
        this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
        this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
      }
      if ((bool)(Object)this.characterModel)
        ++this.characterModel.invisibilityCount;
      if ((bool)(Object)this.hurtboxGroup)
        ++this.hurtboxGroup.hurtBoxesDeactivatorCounter;
      if ((bool)(Object)this.rigidbodyMotor)
        this.rigidbodyMotor.enabled = false;
      Vector3 vector3 = this.inputBank.moveVector * this.blinkDistance;
      this.CalculateBlinkDestination();
      this.CreateBlinkEffect(Util.GetCorePosition(this.gameObject));
    }

    private new void CreateBlinkEffect(Vector3 origin1)
    {
      int num = (int)Util.PlaySound(new EntityStates.VoidRaidCrab.Weapon.FireMultiBeamFinale().enterSoundString, this.gameObject);
      EffectManager.SimpleMuzzleFlash(FathomlessVoidling.deathBombPre, this.gameObject, new FireMissiles().muzzleName, false);
      EffectManager.SpawnEffect(FathomlessVoidling.deathBombPrefab, new EffectData()
      {
        rotation = Util.QuaternionSafeLookRotation(this.blinkDestination - this.blinkStart),
        origin = origin1,
        scale = 75
      }, false);
      if (this.characterBody.name != "MiniVoidRaidCrabBodyPhase1(Clone)")
      {
        new BlastAttack()
        {
          radius = 75,
          position = origin1,
          attacker = this.gameObject,
          teamIndex = TeamComponent.GetObjectTeam(this.gameObject),
          crit = Util.CheckRoll(this.characterBody.crit, this.characterBody.master),
          baseDamage = (this.damageStat * this.blastAttackDamageCoefficient),
          falloffModel = BlastAttack.FalloffModel.Linear,
          attackerFiltering = AttackerFiltering.NeverHitSelf,
          damageType = this.characterBody.name == "MiniVoidRaidCrabBodyPhase3(Clone)" ? DamageType.VoidDeath : DamageType.Stun1s,
          baseForce = this.blastAttackForce
        }.Fire();
      }
    }

    public override void FixedUpdate()
    {
      this.stopwatch += Time.fixedDeltaTime;
      if ((double)this.stopwatch < (double)this.duration || !this.isAuthority)
        return;
      this.rigidbodyMotor.AddDisplacement(this.blinkDestination);
      this.outer.SetNextStateToMain();
    }

    private new void CalculateBlinkDestination()
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
      groundNodes.GetNodePosition(groundNodes.FindClosestNodeWithFlagConditions(vector3, this.characterBody.hullClassification, NodeFlags.None, NodeFlags.NoCharacterSpawn, true), out this.blinkDestination);
    }

    public override void OnExit()
    {
      int num = (int)Util.PlaySound(new BlinkState().endSoundString, this.gameObject);
      this.CreateBlinkEffect(this.characterBody.corePosition);
      this.modelTransform = this.GetModelTransform();
      if ((bool)(Object)this.modelTransform && (bool)(Object)new BlinkState().destealthMaterial)
      {
        TemporaryOverlay temporaryOverlay = this.animator.gameObject.AddComponent<TemporaryOverlay>();
        temporaryOverlay.duration = 1f;
        temporaryOverlay.destroyComponentOnEnd = true;
        temporaryOverlay.originalMaterial = new BlinkState().destealthMaterial;
        temporaryOverlay.inspectorCharacterModel = this.animator.gameObject.GetComponent<CharacterModel>();
        temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0.0f, 1f, 1f, 0.0f);
        temporaryOverlay.animateShaderAlpha = true;
      }
      if ((bool)(Object)this.characterModel)
        --this.characterModel.invisibilityCount;
      if ((bool)(Object)this.hurtboxGroup)
        --this.hurtboxGroup.hurtBoxesDeactivatorCounter;
      if ((bool)(Object)this.rigidbodyMotor)
        this.rigidbodyMotor.enabled = true;
    }

    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;
  }
}