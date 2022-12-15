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
  public class ChargeDesolate : BaseMultiBeamState
  {
    [SerializeField]
    public float baseDuration = new ChargeMultiBeam().baseDuration;
    [SerializeField]
    public GameObject chargeEffectPrefab = new ChargeMultiBeam().chargeEffectPrefab;
    [SerializeField]
    public GameObject warningLaserVfxPrefab = new ChargeMultiBeam().warningLaserVfxPrefab;
    [SerializeField]
    public new string muzzleName = new ChargeMultiBeam().muzzleName;
    [SerializeField]
    public string enterSoundString = new ChargeMultiBeam().enterSoundString;
    [SerializeField]
    public bool isSoundScaledByAttackSpeed = new ChargeMultiBeam().isSoundScaledByAttackSpeed;
    [SerializeField]
    public string animationLayerName = new ChargeMultiBeam().animationLayerName;
    [SerializeField]
    public string animationStateName = new ChargeMultiBeam().animationStateName;
    [SerializeField]
    public string animationPlaybackRateParam = new ChargeMultiBeam().animationPlaybackRateParam;
    private float duration;
    private GameObject chargeEffectInstance;
    private GameObject warningLaserVfxInstance;
    private RayAttackIndicator warningLaserVfxInstanceRayAttackIndicator;

    public override void OnEnter()
    {
      base.OnEnter();
      this.duration = this.baseDuration / this.attackSpeedStat;
      this.PlayAnimation(this.animationLayerName, this.animationStateName, this.animationPlaybackRateParam, this.duration);
      ChildLocator modelChildLocator = this.GetModelChildLocator();
      if ((bool)(Object)modelChildLocator && (bool)(Object)this.chargeEffectPrefab)
      {
        Transform transform = modelChildLocator.FindChild(this.muzzleName) ?? this.characterBody.coreTransform;
        if ((bool)(Object)transform)
        {
          this.chargeEffectInstance = Object.Instantiate<GameObject>(this.chargeEffectPrefab, transform.position, transform.rotation);
          this.chargeEffectInstance.transform.parent = transform;
          ScaleParticleSystemDuration component = this.chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
          if ((bool)(Object)component)
            component.newDuration = this.duration;
        }
      }
      if (!string.IsNullOrEmpty(this.enterSoundString))
      {
        if (this.isSoundScaledByAttackSpeed)
        {
          int num1 = (int)Util.PlayAttackSpeedSound(this.enterSoundString, this.gameObject, this.attackSpeedStat);
        }
        else
        {
          int num2 = (int)Util.PlaySound(this.enterSoundString, this.gameObject);
        }
      }
      this.warningLaserEnabled = true;
    }

    public override void OnExit()
    {
      this.warningLaserEnabled = false;
      EntityState.Destroy((Object)this.chargeEffectInstance);
      base.OnExit();
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if (!this.isAuthority || (double)this.fixedAge < (double)this.duration)
        return;
      this.outer.SetNextState((EntityState)new Desolate());
    }

    public override void Update()
    {
      base.Update();
      this.UpdateWarningLaser();
    }

    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Death;

    private bool warningLaserEnabled
    {
      get => (bool)(Object)this.warningLaserVfxInstance;
      set
      {
        if (value == this.warningLaserEnabled)
          return;
        if (value)
        {
          if (!(bool)(Object)this.warningLaserVfxPrefab)
            return;
          this.warningLaserVfxInstance = Object.Instantiate<GameObject>(this.warningLaserVfxPrefab);
          this.warningLaserVfxInstanceRayAttackIndicator = this.warningLaserVfxInstance.GetComponent<RayAttackIndicator>();
          this.UpdateWarningLaser();
        }
        else
        {
          EntityState.Destroy((Object)this.warningLaserVfxInstance);
          this.warningLaserVfxInstance = (GameObject)null;
          this.warningLaserVfxInstanceRayAttackIndicator = (RayAttackIndicator)null;
        }
      }
    }

    private void UpdateWarningLaser()
    {
      if (!(bool)(Object)this.warningLaserVfxInstanceRayAttackIndicator)
        return;
      this.warningLaserVfxInstanceRayAttackIndicator.attackRange = BaseMultiBeamState.beamMaxDistance;
      Ray beamRay;
      this.CalcBeamPath(out beamRay, out Vector3 _);
      this.warningLaserVfxInstanceRayAttackIndicator.attackRay = beamRay;
    }
  }
}
