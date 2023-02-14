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
  public class ChargeReap : BaseSpinBeamAttackState
  {
    public override void OnEnter()
    {
      base.OnEnter();
      this.CreateBeamVFXInstance(SpinBeamWindUp.warningLaserPrefab);
      int num = (int)Util.PlaySound(SpinBeamWindUp.enterSoundString, this.gameObject);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((double)this.fixedAge >= (double)this.duration && this.isAuthority)
        this.outer.SetNextState((EntityState)new Reap());
      this.SetHeadYawRevolutions(SpinBeamWindUp.revolutionsCurve.Evaluate(this.normalizedFixedAge));
    }
  }
}