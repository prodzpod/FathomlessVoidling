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
  [BepInDependency("com.bepis.r2api")]
  [R2APISubmoduleDependency(new string[]
    {
        "PrefabAPI",
        "ContentAddition"
    })]

  public class FathomlessVoidling : BaseUnityPlugin
  {
    private static GameObject voidRaidCrabPhase1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase1.prefab").WaitForCompletion();
    private static GameObject voidRaidCrabPhase2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase2.prefab").WaitForCompletion();
    private static GameObject voidRaidCrabPhase3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion();
    private static GameObject safeWard = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/iscVoidRaidSafeWard.asset").WaitForCompletion();
    public static GameObject meteor = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab").WaitForCompletion();
    private static GameObject meteorGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab").WaitForCompletion();
    public static GameObject portal = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabSpawnEffect.prefab").WaitForCompletion();
    public static GameObject deathBomb = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab").WaitForCompletion();
    public static GameObject deathBombGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombGhost.prefab").WaitForCompletion();
    public static GameObject deathBombExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombExplosion.prefab").WaitForCompletion();
    public static GameObject deathBombletExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsExplosion.prefab").WaitForCompletion();
    public static GameObject deathBombletGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsGhost.prefab").WaitForCompletion();
    public static GameObject deathBomblet = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsProjectile.prefab").WaitForCompletion();
    private static Material boulderMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Grandparent/matGrandparentBoulderProjectile.mat").WaitForCompletion();
    private static Material voidAffixMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/EliteVoid/matEliteVoidOverlay.mat").WaitForCompletion();
    public static GameObject barnacleBullet = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidBarnacle/VoidBarnacleBullet.prefab").WaitForCompletion();
    public static GameObject deathBombPre = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathPreExplosion.prefab").WaitForCompletion();
    public static GameObject deathBombPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombExplosion.prefab").WaitForCompletion();

    public void Awake()
    {
      On.RoR2.Run.Start += Run_Start;
      On.EntityStates.VoidRaidCrab.Weapon.BaseFireMultiBeam.OnEnter += BaseFireMultiBeamOnEnter;
      CreateSpecial();
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

    private void BaseFireMultiBeamOnEnter(On.EntityStates.VoidRaidCrab.Weapon.BaseFireMultiBeam.orig_OnEnter orig, EntityStates.VoidRaidCrab.Weapon.BaseFireMultiBeam self)
    {
      orig(self);
      FireMissiles instance = new FireMissiles();
      Quaternion quaternion = Util.QuaternionSafeLookRotation(self.GetAimRay().direction);
      FireProjectileInfo fireProjectileInfo = new FireProjectileInfo()
      {
        projectilePrefab = instance.projectilePrefab,
        position = self.muzzleTransform.position,
        owner = self.gameObject,
        damage = self.damageStat * instance.damageCoefficient,
        force = instance.force
      };
      for (int index = 0; index < instance.numMissilesPerWave; ++index)
      {
        fireProjectileInfo.rotation = quaternion * instance.GetRandomRollPitch();
        fireProjectileInfo.crit = Util.CheckRoll(self.critStat, self.characterBody.master);
        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
      }
    }

    private void SetupProjectiles()
    {
      ProjectileController meteorController = meteor.GetComponent<ProjectileController>();
      meteorController.cannotBeDeleted = true;
      meteor.transform.localScale = new Vector3(2, 2, 2);
      meteorGhost.transform.localScale = new Vector3(2, 2, 2);
      meteorGhost.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials = new Material[] { boulderMat, voidAffixMat };
    }

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
      SkillDef utilityDef = skillLocator.utility.skillFamily.variants[0].skillDef;
      primaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Disillusion));
      primaryDef.interruptPriority = EntityStates.InterruptPriority.Death;
      secondaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Crush));
      secondaryDef.interruptPriority = EntityStates.InterruptPriority.Death;
      utilityDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Transpose));

      ProjectileSteerTowardTarget voidRaidMissiles = new FireMissiles().projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
      voidRaidMissiles.rotationSpeed = 180;
    }

    private void CreateSpecial()
    {
      SkillLocator skillLocator = voidRaidCrabPhase1.GetComponent<SkillLocator>();
      GenericSkill skill = voidRaidCrabPhase1.AddComponent<GenericSkill>();
      skill.skillName = "Transpose";
      SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
      (newFamily as ScriptableObject).name = voidRaidCrabPhase1.name + "Transpose" + "Family";
      newFamily.variants = new SkillFamily.Variant[1];
      skill._skillFamily = newFamily;

      SkillDef transpose = ScriptableObject.CreateInstance<SkillDef>();
      transpose.activationState = new EntityStates.SerializableEntityStateType(typeof(Transpose));
      transpose.skillNameToken = "Transpose";
      transpose.activationStateMachineName = "Weapon";
      transpose.baseMaxStock = 2;
      transpose.baseRechargeInterval = 15f;
      transpose.beginSkillCooldownOnSkillEnd = true;
      transpose.canceledFromSprinting = false;
      transpose.cancelSprintingOnActivation = false;
      transpose.fullRestockOnAssign = true;
      transpose.interruptPriority = EntityStates.InterruptPriority.Death;
      transpose.isCombatSkill = true;
      transpose.mustKeyPress = false;
      transpose.rechargeStock = 1;
      transpose.requiredStock = 1;
      transpose.stockToConsume = 1;

      newFamily.variants[0] = new SkillFamily.Variant
      {
        skillDef = transpose,
        viewableNode = new ViewablesCatalog.Node(transpose.skillNameToken, false, null)
      };

      ContentAddition.AddSkillFamily(newFamily);
      skillLocator.special = skill;
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
      SetupProjectiles();
      AdjustPhase1Stats();
    }
  }
}