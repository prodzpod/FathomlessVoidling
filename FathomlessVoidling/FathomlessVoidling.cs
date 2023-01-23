using BepInEx;
using RoR2;
using RoR2.Skills;
using RoR2.Projectile;
using RoR2.CharacterAI;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using R2API;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace FathomlessVoidling
{
  [BepInPlugin("com.Nuxlar.FathomlessVoidling", "FathomlessVoidling", "0.6.3")]
  [BepInDependency("com.bepis.r2api.prefab", BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency("com.bepis.r2api.content_management", BepInDependency.DependencyFlags.HardDependency)]

  public class FathomlessVoidling : BaseUnityPlugin
  {
    private static GameObject voidRaidCrabPhase1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase1.prefab").WaitForCompletion();
    private static GameObject voidRaidCrabPhase2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase2.prefab").WaitForCompletion();
    private static GameObject voidRaidCrabPhase3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion();
    private static GameObject safeWard = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/iscVoidRaidSafeWard.asset").WaitForCompletion();
    public static GameObject meteor = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab").WaitForCompletion(), "VoidMeteor");
    private static GameObject meteorGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab").WaitForCompletion(), "VoidMeteorGhost");
    public static GameObject portal = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabSpawnEffect.prefab").WaitForCompletion();
    public static GameObject bombPrefab3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerDeathBombProjectile.prefab").WaitForCompletion();
    public static GameObject bombPrefab2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierDeathBombProjectile.prefab").WaitForCompletion();
    public static GameObject bombPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierPreBombProjectile.prefab").WaitForCompletion(), "BigPortalBomb");
    public static GameObject bombGhostPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierPreBombGhost.prefab").WaitForCompletion(), "BigPortalBombGhost");
    private static Material boulderMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Grandparent/matGrandparentBoulderProjectile.mat").WaitForCompletion();
    private static Material voidAffixMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/EliteVoid/matEliteVoidOverlay.mat").WaitForCompletion();
    public static GameObject deathBombPre = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathPreExplosion.prefab").WaitForCompletion();
    public static GameObject deathBombPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombExplosion.prefab").WaitForCompletion();
    public static GameObject spawnEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpawnEffect.prefab").WaitForCompletion();
    public static GameObject spinBeamVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpinBeamVFX.prefab").WaitForCompletion();
    public static SpawnCard deepVoidCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/DeepVoidPortal/iscDeepVoidPortal.asset").WaitForCompletion();
    public static SceneDef voidRaid = Addressables.LoadAssetAsync<SceneDef>("RoR2/DLC1/voidraid/voidraid.asset").WaitForCompletion();

    public void Awake()
    {
      On.RoR2.Run.Start += RunStart;
      On.RoR2.Stage.Start += StageStart;
      CharacterMaster.onStartGlobal += MasterChanges;
      On.EntityStates.VoidRaidCrab.BaseVacuumAttackState.OnEnter += BaseVacuumAttackStateOnEnter;
      voidRaid.blockOrbitalSkills = false;
      CreateSpecial();
      AddContent();
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
        -158.7 -152.8 -389
        Body
        3.3
        SuckStart
        SuckLoop
        10
        SuckExit
        3.3
        Suck.playbackRate
    **/

    private void BaseVacuumAttackStateOnEnter(On.EntityStates.VoidRaidCrab.BaseVacuumAttackState.orig_OnEnter orig, EntityStates.VoidRaidCrab.BaseVacuumAttackState self)
    {
      orig(self);
      GameObject donutCenter = GameObject.Find("ReflectionProbe, Center");
      if ((bool)donutCenter)
        self.vacuumOrigin = donutCenter.transform;
    }

    private void AddContent()
    {
      ProjectileController meteorController = meteor.GetComponent<ProjectileController>();
      meteorController.ghost = meteorGhost.GetComponent<ProjectileGhostController>();
      meteorController.ghostPrefab = meteorGhost;
      ContentAddition.AddProjectile(meteor);
      bombGhostPrefab.transform.localScale = new Vector3(10, 10, 10);
      bombPrefab.transform.localScale = new Vector3(5, 5, 5);
      bombPrefab.GetComponent<ProjectileController>().ghostPrefab = bombGhostPrefab;
      ContentAddition.AddProjectile(bombPrefab);
    }

    private void SetupProjectiles()
    {
      Logger.LogInfo("Setting Up Projectiles");
      ProjectileController meteorController = meteor.GetComponent<ProjectileController>();
      meteorController.cannotBeDeleted = true;
      meteor.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
      meteorGhost.transform.localScale = new Vector3(2, 2, 2);
      meteorGhost.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials = new Material[] { boulderMat, voidAffixMat };
      Logger.LogInfo("Finished Setting Up Projectiles");
    }

    private void AdjustPhase1Stats()
    {
      Logger.LogInfo("Adjusting P1 Stats");
      CharacterBody voidRaidCrabBody = voidRaidCrabPhase1.GetComponent<CharacterBody>();
      voidRaidCrabBody.subtitleNameToken = "Augur of the Abyss";
      voidRaidCrabBody.baseMaxHealth = 1250;
      voidRaidCrabBody.levelMaxHealth = 350;
      voidRaidCrabBody.baseAttackSpeed = 1.25f;
      voidRaidCrabBody.baseMoveSpeed = 67.5f;
      voidRaidCrabBody.baseAcceleration = 30;
      voidRaidCrabBody.baseArmor = 30;
      Logger.LogInfo("Finished Adjusting P1 Stats");

      Logger.LogInfo("Adjusting P1 Skills");
      SkillLocator skillLocator = voidRaidCrabPhase1.GetComponent<SkillLocator>();
      SkillDef primaryDef = skillLocator.primary.skillFamily.variants[0].skillDef;
      SkillDef secondaryDef = skillLocator.secondary.skillFamily.variants[0].skillDef;
      SkillDef utilityDef = skillLocator.utility.skillFamily.variants[0].skillDef;
      SkillDef specialDef = skillLocator.special.skillFamily.variants[0].skillDef;

      primaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Disillusion));
      secondaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(BaseVacuumAttackState));
      secondaryDef.interruptPriority = EntityStates.InterruptPriority.Death;
      secondaryDef.baseRechargeInterval = 40f;
      utilityDef.activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeDesolate));
      utilityDef.baseRechargeInterval = 30f;
      specialDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Transpose));

      ProjectileSteerTowardTarget voidRaidMissiles = new FireMissiles().projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
      voidRaidMissiles.rotationSpeed = 180;
      Logger.LogInfo("Finished Adjusting P1 Skills");
    }

    private void CreateSpecial()
    {
      Logger.LogInfo("Creating Special");
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
      transpose.activationStateMachineName = "Body";
      transpose.baseMaxStock = 1;
      transpose.baseRechargeInterval = 20f;
      transpose.beginSkillCooldownOnSkillEnd = true;
      transpose.canceledFromSprinting = false;
      transpose.cancelSprintingOnActivation = false;
      transpose.fullRestockOnAssign = true;
      transpose.interruptPriority = EntityStates.InterruptPriority.Skill;
      transpose.isCombatSkill = true;
      transpose.mustKeyPress = false;
      transpose.rechargeStock = 1;
      transpose.requiredStock = 1;
      transpose.stockToConsume = 1;

      newFamily.variants[0] = new SkillFamily.Variant { skillDef = transpose };

      ContentAddition.AddSkillFamily(newFamily);
      skillLocator.special = skill;
      Logger.LogInfo("Finished Creating Special");
    }
    private void AdjustPhase2Stats()
    {
      Logger.LogInfo("Adjusting P2 Stats");
      CharacterBody voidRaidCrabBody = voidRaidCrabPhase2.GetComponent<CharacterBody>();
      voidRaidCrabBody.baseMaxHealth = 1250;
      voidRaidCrabBody.levelMaxHealth = 350;
      voidRaidCrabBody.baseAttackSpeed = 1.25f;
      voidRaidCrabBody.baseMoveSpeed = 90;
      voidRaidCrabBody.baseAcceleration = 45;
      voidRaidCrabBody.baseArmor = 30;
      Logger.LogInfo("Finished Adjusting P2 Stats");

      Logger.LogInfo("Adjusting P2 Skills");
      SkillLocator skillLocator = voidRaidCrabPhase2.GetComponent<SkillLocator>();
      SkillDef primaryDef = skillLocator.primary.skillFamily.variants[0].skillDef;
      SkillDef secondaryDef = skillLocator.secondary.skillFamily.variants[0].skillDef;
      SkillDef utilityDef = skillLocator.utility.skillFamily.variants[0].skillDef;
      SkillDef specialDef = skillLocator.special.skillFamily.variants[0].skillDef;

      primaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Disillusion));
      secondaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(VacuumEnter));
      secondaryDef.interruptPriority = EntityStates.InterruptPriority.Death;
      secondaryDef.baseRechargeInterval = 40f;
      utilityDef.activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeDesolate));
      utilityDef.baseRechargeInterval = 30f;
      specialDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Transpose));
      specialDef.baseRechargeInterval = 20f;
      specialDef.interruptPriority = EntityStates.InterruptPriority.Skill;
      Logger.LogInfo("Finished Adjusting P2 Skills");
    }
    private void AdjustPhase3Stats()
    {
      Logger.LogInfo("Adjusting P3 Stats");
      CharacterBody voidRaidCrabBody = voidRaidCrabPhase3.GetComponent<CharacterBody>();
      voidRaidCrabBody.baseMaxHealth = 1250;
      voidRaidCrabBody.levelMaxHealth = 350;
      voidRaidCrabBody.baseAttackSpeed = 1.25f;
      voidRaidCrabBody.baseMoveSpeed = 90;
      voidRaidCrabBody.baseAcceleration = 45;
      voidRaidCrabBody.baseArmor = 30;
      Logger.LogInfo("Finished Adjusting P3 Stats");
    }

    private void SpawnDeepVoidPortal()
    {
      DirectorPlacementRule placementRule = new DirectorPlacementRule();
      placementRule.placementMode = DirectorPlacementRule.PlacementMode.Direct;
      GameObject spawnedInstance = deepVoidCard.DoSpawn(new Vector3(-158.7f, -152.8f, -389f), Quaternion.identity, new DirectorSpawnRequest(deepVoidCard, placementRule, Run.instance.runRNG)).spawnedInstance;
      NetworkServer.Spawn(spawnedInstance);
    }

    private void RunStart(On.RoR2.Run.orig_Start orig, Run self)
    {
      orig(self);
      SetupProjectiles();
      AdjustPhase1Stats();
      AdjustPhase2Stats();
      AdjustPhase3Stats();
    }

    private void StageStart(On.RoR2.Stage.orig_Start orig, RoR2.Stage self)
    {
      orig(self);
      if (self.sceneDef.cachedName == "moon2")
        SpawnDeepVoidPortal();
    }

    private void MasterChanges(CharacterMaster master)
    {
      if (master.name == "MiniVoidRaidCrabMasterPhase1(Clone)" || master.name == "MiniVoidRaidCrabMasterPhase2(Clone)")
      {
        Logger.LogInfo("Editing P1 Special AISkillDriver");
        AISkillDriver aiSkillDriverPrimary = ((IEnumerable<AISkillDriver>)master.GetComponents<AISkillDriver>()).Where<AISkillDriver>((Func<AISkillDriver, bool>)(x => x.skillSlot == SkillSlot.Primary)).First<AISkillDriver>();
        AISkillDriver aiSkillDriverSpecial = ((IEnumerable<AISkillDriver>)master.GetComponents<AISkillDriver>()).Where<AISkillDriver>((Func<AISkillDriver, bool>)(x => x.skillSlot == SkillSlot.Special)).First<AISkillDriver>();
        aiSkillDriverSpecial.requiredSkill = voidRaidCrabPhase1.GetComponent<SkillLocator>().special.skillFamily.variants[0].skillDef;
        aiSkillDriverSpecial.activationRequiresAimConfirmation = aiSkillDriverPrimary.activationRequiresAimConfirmation;
        aiSkillDriverSpecial.activationRequiresAimTargetLoS = aiSkillDriverPrimary.activationRequiresAimTargetLoS;
        aiSkillDriverSpecial.activationRequiresTargetLoS = aiSkillDriverPrimary.activationRequiresTargetLoS;
        aiSkillDriverSpecial.minUserHealthFraction = aiSkillDriverPrimary.minUserHealthFraction;
        aiSkillDriverSpecial.maxUserHealthFraction = aiSkillDriverPrimary.maxUserHealthFraction;
        aiSkillDriverSpecial.maxTargetHealthFraction = aiSkillDriverPrimary.maxTargetHealthFraction;
        aiSkillDriverSpecial.minTargetHealthFraction = aiSkillDriverPrimary.minTargetHealthFraction;
        aiSkillDriverSpecial.requireSkillReady = aiSkillDriverPrimary.requireSkillReady;
        aiSkillDriverSpecial.maxDistance = float.PositiveInfinity;
        Logger.LogInfo("Finished Editing P1 Special AISkillDriver");
      }
      if (master.name == "MiniVoidRaidCrabMasterPhase1(Clone)" || master.name == "MiniVoidRaidCrabMasterPhase2(Clone)" || master.name == "MiniVoidRaidCrabMasterPhase3(Clone)")
        master.inventory.GiveItem(RoR2Content.Items.TeleportWhenOob);
    }
  }
}