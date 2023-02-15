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
  [BepInPlugin("com.Nuxlar.FathomlessVoidling", "FathomlessVoidling", "0.9.0")]
  [BepInDependency("com.bepis.r2api.content_management", BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency("com.bepis.r2api.prefab", BepInDependency.DependencyFlags.HardDependency)]
  [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.HardDependency)]

  public class FathomlessVoidling : BaseUnityPlugin
  {
    public static GameObject voidRaidCrabBase = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyBase.prefab").WaitForCompletion();
    public static GameObject voidRaidCrabPhase1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase1.prefab").WaitForCompletion();
    public static GameObject voidRaidCrabPhase2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase2.prefab").WaitForCompletion();
    public static GameObject voidRaidCrabPhase3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion();
    public static GameObject meteor = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab").WaitForCompletion(), "VoidMeteor");
    private static GameObject meteorGhost = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab").WaitForCompletion(), "VoidMeteorGhost");
    public static GameObject portal = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabSpawnEffect.prefab").WaitForCompletion();
    public static GameObject bombPrefab3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerDeathBombProjectile.prefab").WaitForCompletion();
    public static GameObject bombPrefab2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierDeathBombProjectile.prefab").WaitForCompletion();
    public static GameObject bombPrefab1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombProjectile.prefab").WaitForCompletion();
    private static Material boulderMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Grandparent/matGrandparentBoulderProjectile.mat").WaitForCompletion();
    private static Material voidAffixMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/EliteVoid/matEliteVoidOverlay.mat").WaitForCompletion();
    public static GameObject deathBombPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombExplosion.prefab").WaitForCompletion();
    public static GameObject spawnEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpawnEffect.prefab").WaitForCompletion();
    public static GameObject spinBeamVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpinBeamVFX.prefab").WaitForCompletion();
    // Alt Moon
    public static GameObject r2wCauldron = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, RedToWhite Variant.prefab").WaitForCompletion();
    public static GameObject g2rCauldron = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, GreenToRed Variant.prefab").WaitForCompletion();
    public static GameObject w2gCauldron = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarCauldrons/LunarCauldron, WhiteToGreen.prefab").WaitForCompletion();
    public static SpawnCard locusPortalCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/DLC1/PortalVoid/iscVoidPortal.asset").WaitForCompletion();

    public static SceneDef voidRaid = Addressables.LoadAssetAsync<SceneDef>("RoR2/DLC1/voidraid/voidraid.asset").WaitForCompletion();
    private static AnimationCurve singularityDeathCurve = null;
    private static AnimationCurve singularityPullCurve = null;
    public void Awake()
    {
      ModConfig.InitConfig(Config);
      On.RoR2.Run.Start += RunStart;
      On.RoR2.Stage.Start += StageStart;
      On.RoR2.VoidStageMissionController.RequestFog += RequestFog;
      On.RoR2.TeleporterInteraction.AttemptToSpawnAllEligiblePortals += TeleporterInteraction_AttemptToSpawnAllEligiblePortals;
      CharacterMaster.onStartGlobal += MasterChanges;
      On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
      On.EntityStates.VoidRaidCrab.BaseVacuumAttackState.OnEnter += BaseVacuumAttackStateOnEnter;
      voidRaid.blockOrbitalSkills = false;
      new Skills();
      SetupProjectiles();
      AddContent();
    }
    /** 
        Base Stats
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

    private void StageStart(On.RoR2.Stage.orig_Start orig, RoR2.Stage self)
    {
      orig(self);
      if (self.sceneDef.cachedName == "voidstage" && ModConfig.enableAltMoon.Value)
        SpawnLocusCauldrons();
    }

    private void SpawnLocusCauldrons()
    {
      GameObject cauldron1 = Instantiate(r2wCauldron, new Vector3(-142.67f, 29.94f, 242.74f), Quaternion.identity);
      cauldron1.transform.eulerAngles = new Vector3(0.0f, 66f, 0.0f);
      NetworkServer.Spawn(cauldron1);
      GameObject cauldron2 = Instantiate(g2rCauldron, new Vector3(-136.76f, 29.94f, 246.51f), Quaternion.identity);
      cauldron2.transform.eulerAngles = new Vector3(0.0f, 66f, 0.0f);
      NetworkServer.Spawn(cauldron2);
      GameObject cauldron3 = Instantiate(g2rCauldron, new Vector3(-149.74f, 29.93f, 239.7f), Quaternion.identity);
      cauldron3.transform.eulerAngles = new Vector3(0.0f, 66f, 0.0f);
      NetworkServer.Spawn(cauldron3);
      GameObject cauldron4 = Instantiate(w2gCauldron, new Vector3(-157.41f, 29.97f, 237.12f), Quaternion.identity);
      cauldron4.transform.eulerAngles = new Vector3(0.0f, 66f, 0.0f);
      NetworkServer.Spawn(cauldron4);
      GameObject cauldron5 = Instantiate(w2gCauldron, new Vector3(-126.63f, 29.93f, 249.1f), Quaternion.identity);
      cauldron5.transform.eulerAngles = new Vector3(0.0f, 66f, 0.0f);
      NetworkServer.Spawn(cauldron5);
    }

    private VoidStageMissionController.FogRequest RequestFog(On.RoR2.VoidStageMissionController.orig_RequestFog orig, RoR2.VoidStageMissionController self, IZone zone)
    {
      if (ModConfig.enableVoidFog.Value)
        return orig(self, zone);
      else
        return null;
    }

    private void TeleporterInteraction_AttemptToSpawnAllEligiblePortals(On.RoR2.TeleporterInteraction.orig_AttemptToSpawnAllEligiblePortals orig, TeleporterInteraction self)
    {
      if (SceneCatalog.mostRecentSceneDef == SceneCatalog.GetSceneDefFromSceneName("skymeadow") && ModConfig.enableAltMoon.Value)
      {
        var list = self.portalSpawners.toList();
        var locus = list.Find(x => x.portalSpawnCard == locusPortalCard);
        if (locus != null) 
        {
          list.Remove(locus);
          self.portalSpawners = list.toArray();
        }
        SpawnLocusPortal(self.transform, self.rng);
      }
      orig(self);
    }

    private void SpawnLocusPortal(Transform transform, Xoroshiro128Plus rng)
    {
      DirectorCore instance = DirectorCore.instance;
      DirectorPlacementRule placementRule = new();
      placementRule.minDistance = 10f;
      placementRule.maxDistance = 40f;
      placementRule.placementMode = DirectorPlacementRule.PlacementMode.Approximate;
      placementRule.position = transform.position;
      placementRule.spawnOnTarget = transform;
      DirectorSpawnRequest directorSpawnRequest = new(locusPortalCard, placementRule, rng);
      GameObject gameObject = instance.TrySpawnObject(directorSpawnRequest);
      if ((bool)gameObject)
        NetworkServer.Spawn(gameObject);
    }

    private void BaseVacuumAttackStateOnEnter(On.EntityStates.VoidRaidCrab.BaseVacuumAttackState.orig_OnEnter orig, EntityStates.VoidRaidCrab.BaseVacuumAttackState self)
    {
      orig(self);
      if (self.characterBody.name == "MiniVoidRaidCrabBodyPhase2(Clone)")
      {
        Transform donutCenter = VoidRaidGauntletController.instance.currentDonut.root.transform.Find("ReflectionProbe, Center");
        if ((bool)donutCenter)
          self.vacuumOrigin = donutCenter;
      }
    }

    private void AddContent()
    {
      ContentAddition.AddEntityState<ChargeRend>(out _);
      ContentAddition.AddEntityState<Rend>(out _);
      ContentAddition.AddEntityState<Reap>(out _);
      ContentAddition.AddEntityState<ChargeCrush>(out _);
      ContentAddition.AddEntityState<Crush>(out _);
      ContentAddition.AddEntityState<Disillusion>(out _);
      ContentAddition.AddEntityState<Transpose>(out _);
      ContentAddition.AddProjectile(meteor);
    }

    private void SetupProjectiles()
    {
      Logger.LogInfo("Setting Up Projectiles");
      ProjectileController meteorController = meteor.GetComponent<ProjectileController>();
      meteorController.cannotBeDeleted = true;
      meteor.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
      meteorGhost.transform.localScale = new Vector3(2, 2, 2);
      meteorGhost.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials = new Material[] { boulderMat, voidAffixMat };
      meteorController.ghost = meteorGhost.GetComponent<ProjectileGhostController>();
      meteorController.ghostPrefab = meteorGhost;
      Logger.LogInfo("Finished Setting Up Projectiles");
    }

    private void AdjustPhase1Stats()
    {
      Logger.LogInfo("Adjusting P1 Stats");
      CharacterBody body = voidRaidCrabPhase1.GetComponent<CharacterBody>();
      body.subtitleNameToken = "Augur of the Abyss";
      body.baseMaxHealth = ModConfig.baseHealth.Value;
      body.levelMaxHealth = ModConfig.levelHealth.Value;
      body.baseDamage = ModConfig.baseDamage.Value;
      body.levelDamage = ModConfig.levelDamage.Value;
      body.baseAttackSpeed = ModConfig.baseAtkSpd.Value;
      body.baseMoveSpeed = ModConfig.baseSpd.Value;
      body.baseAcceleration = ModConfig.acceleration.Value;
      body.baseArmor = ModConfig.baseArmor.Value;
      Logger.LogInfo("Finished Adjusting P1 Stats");
    }

    private void AdjustPhase2Stats()
    {
      Logger.LogInfo("Adjusting P2 Stats");
      CharacterBody body = voidRaidCrabPhase2.GetComponent<CharacterBody>();
      body.subtitleNameToken = "Augur of the Abyss";
      body.baseMaxHealth = ModConfig.baseHealth.Value;
      body.levelMaxHealth = ModConfig.levelHealth.Value;
      body.baseDamage = ModConfig.baseDamage.Value;
      body.levelDamage = ModConfig.levelDamage.Value;
      body.baseAttackSpeed = ModConfig.baseAtkSpd.Value;
      body.baseMoveSpeed = ModConfig.baseSpd.Value;
      body.baseAcceleration = ModConfig.acceleration.Value;
      body.baseArmor = ModConfig.baseArmor.Value;
      Logger.LogInfo("Finished Adjusting P2 Stats");
    }

    private void AdjustPhase3Stats()
    {
      Logger.LogInfo("Adjusting P3 Stats");
      CharacterBody body = voidRaidCrabPhase3.GetComponent<CharacterBody>();
      body.subtitleNameToken = "Augur of the Abyss";
      body.baseMaxHealth = ModConfig.baseHealth.Value;
      body.levelMaxHealth = ModConfig.levelHealth.Value;
      body.baseDamage = ModConfig.baseDamage.Value;
      body.levelDamage = ModConfig.levelDamage.Value;
      body.baseAttackSpeed = ModConfig.baseAtkSpd.Value;
      body.baseMoveSpeed = ModConfig.baseSpd.Value;
      body.baseAcceleration = ModConfig.acceleration.Value;
      body.baseArmor = ModConfig.baseArmor.Value;
      Logger.LogInfo("Finished Adjusting P3 Stats");
    }
    private void AdjustPhase1Skills(CharacterBody body)
    {
      Logger.LogInfo("Adjusting P1 Skills");
      SkillLocator skillLocator = body.skillLocator;
      SkillDef primaryDef = skillLocator.primary.skillFamily.variants[0].skillDef;
      SkillDef secondaryDef = skillLocator.secondary.skillFamily.variants[0].skillDef;
      SkillDef utilityDef = skillLocator.utility.skillFamily.variants[0].skillDef;
      SkillDef specialDef = skillLocator.special.skillFamily.variants[0].skillDef;

      primaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Disillusion));
      secondaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(VacuumEnter));
      utilityDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Transpose));
      specialDef.activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeRend));

      ProjectileSteerTowardTarget voidRaidMissiles = new FireMissiles().projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
      voidRaidMissiles.rotationSpeed = 180;
      Logger.LogInfo("Finished Adjusting P1 Skills");
    }
    private void AdjustPhase2Skills(CharacterBody body)
    {
      Logger.LogInfo("Adjusting P2 Skills");
      SkillLocator skillLocator = body.skillLocator;
      SkillDef primaryDef = skillLocator.primary.skillFamily.variants[0].skillDef;
      SkillDef secondaryDef = skillLocator.secondary.skillFamily.variants[0].skillDef;
      SkillDef utilityDef = skillLocator.utility.skillFamily.variants[0].skillDef;
      SkillDef specialDef = skillLocator.special.skillFamily.variants[0].skillDef;

      primaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Disillusion));
      secondaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(VacuumEnter));
      utilityDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Transpose));
      specialDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SpinBeamEnter));
      Logger.LogInfo("Finished Adjusting P2 Skills");
    }
    private void AdjustPhase3Skills(CharacterBody body)
    {
      Logger.LogInfo("Adjusting P3 Skills");
      SkillLocator skillLocator = body.skillLocator;
      SkillDef primaryDef = skillLocator.primary.skillFamily.variants[0].skillDef;
      SkillDef secondaryDef = skillLocator.secondary.skillFamily.variants[0].skillDef;
      SkillDef utilityDef = skillLocator.utility.skillFamily.variants[0].skillDef;
      SkillDef specialDef = skillLocator.special.skillFamily.variants[0].skillDef;

      primaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Disillusion));
      secondaryDef.activationState = new EntityStates.SerializableEntityStateType(typeof(ChargeCrush));
      utilityDef.activationState = new EntityStates.SerializableEntityStateType(typeof(Transpose));
      specialDef.activationState = new EntityStates.SerializableEntityStateType(typeof(SpinBeamEnter));
      Logger.LogInfo("Finished Adjusting P3 Skills");
    }

    private void RunStart(On.RoR2.Run.orig_Start orig, Run self)
    {
      orig(self);
      AdjustPhase1Stats();
      AdjustPhase2Stats();
      AdjustPhase3Stats();
    }

    private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, RoR2.CharacterMaster self, CharacterBody body)
    {
      orig(self, body);
      if (body.name == "MiniVoidRaidCrabBodyPhase1(Clone)")
        AdjustPhase1Skills(body);
      if (body.name == "MiniVoidRaidCrabBodyPhase2(Clone)")
        AdjustPhase2Skills(body);
      if (body.name == "MiniVoidRaidCrabBodyPhase3(Clone)")
        AdjustPhase3Skills(body);
    }

    private void MasterChanges(CharacterMaster master)
    {
      if (master.name == "MiniVoidRaidCrabMasterPhase1(Clone)" || master.name == "MiniVoidRaidCrabMasterPhase2(Clone)" || master.name == "MiniVoidRaidCrabMasterPhase3(Clone)")
      {
        Logger.LogInfo("Editing AISkillDrivers");
        AISkillDriver aiSkillDriverPrimary = ((IEnumerable<AISkillDriver>)master.GetComponents<AISkillDriver>()).Where<AISkillDriver>((Func<AISkillDriver, bool>)(x => x.skillSlot == SkillSlot.Primary)).First<AISkillDriver>();
        AISkillDriver aiSkillDriverSecondary = ((IEnumerable<AISkillDriver>)master.GetComponents<AISkillDriver>()).Where<AISkillDriver>((Func<AISkillDriver, bool>)(x => x.skillSlot == SkillSlot.Secondary)).First<AISkillDriver>();
        AISkillDriver aiSkillDriverUtility = ((IEnumerable<AISkillDriver>)master.GetComponents<AISkillDriver>()).Where<AISkillDriver>((Func<AISkillDriver, bool>)(x => x.skillSlot == SkillSlot.Utility)).First<AISkillDriver>();
        AISkillDriver aiSkillDriverSpecial = ((IEnumerable<AISkillDriver>)master.GetComponents<AISkillDriver>()).Where<AISkillDriver>((Func<AISkillDriver, bool>)(x => x.skillSlot == SkillSlot.Special)).First<AISkillDriver>();
        aiSkillDriverSecondary.movementType = AISkillDriver.MovementType.Stop;
        aiSkillDriverSecondary.maxUserHealthFraction = 0.9f;
        aiSkillDriverSecondary.minUserHealthFraction = float.NegativeInfinity;
        aiSkillDriverSecondary.requiredSkill = null;
        aiSkillDriverSecondary.maxDistance = 200;
        aiSkillDriverUtility.maxUserHealthFraction = float.PositiveInfinity;
        aiSkillDriverUtility.minUserHealthFraction = float.NegativeInfinity;
        aiSkillDriverUtility.requiredSkill = null;
        aiSkillDriverUtility.maxDistance = 400;
        aiSkillDriverUtility.minDistance = 0;
        aiSkillDriverSpecial.maxUserHealthFraction = 0.8f;
        aiSkillDriverSpecial.minUserHealthFraction = float.NegativeInfinity;
        aiSkillDriverSpecial.requiredSkill = null;
        aiSkillDriverSpecial.maxDistance = 400;
        aiSkillDriverSpecial.minDistance = 0;
        Logger.LogInfo("Finished Editing AISkillDrivers");
      }
      if (master.name == "MiniVoidRaidCrabMasterPhase1(Clone)" || master.name == "MiniVoidRaidCrabMasterPhase2(Clone)" || master.name == "MiniVoidRaidCrabMasterPhase3(Clone)")
        master.inventory.GiveItem(RoR2Content.Items.TeleportWhenOob);
    }
  }
}
