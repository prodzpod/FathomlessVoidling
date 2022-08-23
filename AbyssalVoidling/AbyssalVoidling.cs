using BepInEx;
using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using R2API;
using R2API.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace AbyssalVoidling
{
  [BepInPlugin("com.Nuxlar.AbyssalVoidling", "AbyssalVoidling", "0.5.0")]

  public class AbyssalVoidling : BaseUnityPlugin
  {
    GameObject voidRaidCrabPhase1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase1.prefab").WaitForCompletion();
    GameObject voidRaidCrabPhase2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase2.prefab").WaitForCompletion();
    GameObject voidRaidCrabPhase3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion();

    public void Awake()
    {
      On.RoR2.Run.Start += Run_Start;
      On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.OnEnter += BaseSpinBeamAttackState_OnEnter;
    }
    /** 
        Base Stats
        accel 20
        armor 20
        attkspd 1
        dmg 15
        HP 2000
        movespd 45
        jump 0
        projectile rotation 360
    **/
    private void AdjustPhase1Stats()
    {
      CharacterBody voidRaidCrabBody = voidRaidCrabPhase1.GetComponent<CharacterBody>();
      voidRaidCrabBody.baseMaxHealth = 1250;
      voidRaidCrabBody.baseAttackSpeed = 1.5f;
      voidRaidCrabBody.baseMoveSpeed = 65;
      voidRaidCrabBody.baseAcceleration = 40;
      ProjectileSteerTowardTarget voidRaidMissiles = new FireMissiles().projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
      voidRaidMissiles.rotationSpeed = 160;
    }
    private void AdjustPhase2Stats()
    {
      CharacterBody voidRaidCrabBody = voidRaidCrabPhase2.GetComponent<CharacterBody>();
      voidRaidCrabBody.baseMaxHealth = 1500;
      voidRaidCrabBody.baseAttackSpeed = 1.75f;
      voidRaidCrabBody.baseMoveSpeed = 85;
      voidRaidCrabBody.baseAcceleration = 45;
      ProjectileSteerTowardTarget voidRaidMissiles = new FireMissiles().projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
      voidRaidMissiles.rotationSpeed = 180;
    }
    private void AdjustPhase3Stats()
    {
      CharacterBody voidRaidCrabBody = voidRaidCrabPhase3.GetComponent<CharacterBody>();
      voidRaidCrabBody.baseMaxHealth = 1750;
      voidRaidCrabBody.baseAttackSpeed = 2;
      voidRaidCrabBody.baseMoveSpeed = 105;
      voidRaidCrabBody.baseAcceleration = 50;
      ProjectileSteerTowardTarget voidRaidMissiles = new FireMissiles().projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
      voidRaidMissiles.rotationSpeed = 200;
    }

    private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
    {
      AdjustPhase1Stats();
      AdjustPhase2Stats();
      AdjustPhase3Stats();
      orig(self);
    }

    private void BaseSpinBeamAttackState_OnEnter(On.EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.orig_OnEnter orig, EntityStates.VoidRaidCrab.BaseSpinBeamAttackState self)
    {
      self.baseDuration = 3.5f;
      orig(self);
    }

    private void BaseVacuumAttackState_OnEnter(On.EntityStates.VoidRaidCrab.BaseVacuumAttackState.orig_OnEnter orig, EntityStates.VoidRaidCrab.BaseVacuumAttackState self)
    {
      self.baseDuration = 5.5f;
      orig(self);
    }

  }
}