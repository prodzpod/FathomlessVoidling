using RoR2;
using RoR2.Navigation;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.ImpMonster;
using EntityStates.BrotherMonster;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using EntityStates.BrotherMonster.Weapon;
using System.Collections.Generic;

namespace FathomlessVoidling
{
  public class Transpose : BlinkState
  {
    public static GameObject blinkPrefab;
    public static Material destealthMaterial;
    public static float duration = 3f;
    public static string beginSoundString;
    public static string endSoundString;

    public override void OnEnter()
    {
      int num = (int)Util.PlaySound(BlinkState.beginSoundString, this.gameObject);
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
      Vector3 vector3 = this.inputBank.moveVector * BlinkState.blinkDistance;
      this.CreateBlinkEffect(Util.GetCorePosition(this.gameObject));
    }

    private void CreateBlinkEffect(Vector3 origin) => EffectManager.SpawnEffect(BlinkState.blinkPrefab, new EffectData()
    {
      rotation = Util.QuaternionSafeLookRotation(this.blinkDestination - this.blinkStart),
      origin = origin
    }, false);

    public override void FixedUpdate()
    {
      this.stopwatch += Time.fixedDeltaTime;
      if ((double)this.stopwatch < (double)Transpose.duration || !this.isAuthority)
        return;
      this.outer.SetNextStateToMain();
    }

    public override void OnExit()
    {
      int num = (int)Util.PlaySound(BlinkState.endSoundString, this.gameObject);
      this.CreateBlinkEffect(Util.GetCorePosition(this.gameObject));
      this.modelTransform = this.GetModelTransform();
      if ((bool)(Object)this.modelTransform && (bool)(Object)BlinkState.destealthMaterial)
      {
        TemporaryOverlay temporaryOverlay = this.animator.gameObject.AddComponent<TemporaryOverlay>();
        temporaryOverlay.duration = 1f;
        temporaryOverlay.destroyComponentOnEnd = true;
        temporaryOverlay.originalMaterial = BlinkState.destealthMaterial;
        temporaryOverlay.inspectorCharacterModel = this.animator.gameObject.GetComponent<CharacterModel>();
        temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0.0f, 1f, 1f, 0.0f);
        temporaryOverlay.animateShaderAlpha = true;
      }
      if ((bool)(Object)this.characterModel)
        --this.characterModel.invisibilityCount;
      if ((bool)(Object)this.hurtboxGroup)
        --this.hurtboxGroup.hurtBoxesDeactivatorCounter;
    }

    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
  }
}