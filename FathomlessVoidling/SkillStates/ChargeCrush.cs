using UnityEngine;
using RoR2;
using EntityStates;
using EntityStates.VoidRaidCrab.Weapon;

namespace FathomlessVoidling
{
  public class ChargeCrush : BaseState
  {
    [SerializeField]
    public float baseDuration = 3.3f;
    protected float duration { get; private set; }

    public override void OnEnter()
    {
      base.OnEnter();
      this.duration = this.baseDuration / this.attackSpeedStat;
      this.PlayAnimation("Body", "SuckEnter", "Suck.playbackRate", 3.3f);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if (!this.isAuthority || (double)this.fixedAge < (double)this.duration)
        return;
      int num2 = (int)Util.PlaySound(new EntityStates.VoidRaidCrab.Weapon.FireMultiBeamSmall().enterSoundString, this.gameObject);
      EffectManager.SimpleMuzzleFlash(new FireMissiles().muzzleFlashPrefab, this.gameObject, BaseMultiBeamState.muzzleName, false);
      this.PlayAnimation("Body", "SuckExit", "Suck.playbackRate", 3.3f);
      this.outer.SetNextState((EntityState)new Crush());
    }

    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
  }
}
