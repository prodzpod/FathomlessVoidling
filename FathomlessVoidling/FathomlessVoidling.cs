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

namespace FathomlessVoidling
{
  [BepInPlugin("com.Nuxlar.FathomlessVoidling", "FathomlessVoidling", "1.0.0")]

  public class FathomlessVoidling : BaseUnityPlugin
  {
    static GameObject voidRaidCrabPhase1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase1.prefab").WaitForCompletion();
    GameObject voidRaidCrabPhase2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase2.prefab").WaitForCompletion();
    GameObject voidRaidCrabPhase3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion();
    GameObject safeWard = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/iscVoidRaidSafeWard.asset").WaitForCompletion();

    public void Awake()
    {
      On.RoR2.Run.Start += Run_Start;
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
      voidRaidCrabBody.baseMaxHealth = 1100;
      voidRaidCrabBody.baseAttackSpeed = 1.25f;
      voidRaidCrabBody.baseMoveSpeed = 67.5f;
      voidRaidCrabBody.baseAcceleration = 30;
      voidRaidCrabBody.baseArmor = 30;
      voidRaidCrabPhase1.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

      SkillLocator skillLocator = voidRaidCrabPhase1.GetComponent<SkillLocator>();
      SkillDef primaryDef = skillLocator.primary.skillFamily.variants[0].skillDef;
      SkillDef secondaryDef = skillLocator.secondary.skillFamily.variants[0].skillDef;
      primaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeMultiBeam));
      secondaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(OrbitalBarrage));
    }
    private void AdjustPhase2Stats()
    {
      CharacterBody voidRaidCrabBody = voidRaidCrabPhase2.GetComponent<CharacterBody>();
      voidRaidCrabBody.baseMaxHealth = 1100;
      voidRaidCrabBody.baseAttackSpeed = 1.25f;
      voidRaidCrabBody.baseMoveSpeed = 90;
      voidRaidCrabBody.baseAcceleration = 45;
      voidRaidCrabBody.baseArmor = 30;
      voidRaidCrabPhase2.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

      SkillLocator skillLocator = voidRaidCrabBody.skillLocator;
      SkillDef primary = skillLocator.primary.skillFamily.variants[0].skillDef;
      SkillDef secondary = skillLocator.secondary.skillFamily.variants[0].skillDef;
      SkillDef utility = skillLocator.utility.skillFamily.variants[0].skillDef;
      SkillDef special = skillLocator.special.skillFamily.variants[0].skillDef;
    }
    private void AdjustPhase3Stats()
    {
      CharacterBody voidRaidCrabBody = voidRaidCrabPhase3.GetComponent<CharacterBody>();
      voidRaidCrabBody.baseMaxHealth = 1100;
      voidRaidCrabBody.baseAttackSpeed = 1.25f;
      voidRaidCrabBody.baseMoveSpeed = 90;
      voidRaidCrabBody.baseAcceleration = 45;
      voidRaidCrabBody.baseArmor = 30;
      voidRaidCrabPhase3.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

      SkillLocator skillLocator = voidRaidCrabBody.skillLocator;
      SkillDef primary = skillLocator.primary.skillFamily.variants[0].skillDef;
      SkillDef secondary = skillLocator.secondary.skillFamily.variants[0].skillDef;
      SkillDef utility = skillLocator.utility.skillFamily.variants[0].skillDef;
      SkillDef special = skillLocator.special.skillFamily.variants[0].skillDef;
    }

    private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
    {
      //AdjustPhase2Stats();
      //AdjustPhase3Stats();
      orig(self);
      AdjustPhase1Stats();
    }
  }
}