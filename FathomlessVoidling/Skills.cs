using RoR2;
using RoR2.Skills;
using R2API;
using UnityEngine;
using EntityStates.VoidRaidCrab;

namespace FathomlessVoidling
{
  public class Skills
  {
    public Skills()
    {
      CreateSkills();
    }

    private void CreateSkills()
    {
      for (int i = 0; i < 3; i++)
      {
        CreatePrimary(i);
        CreateSecondary(i);
        CreateUtility(i);
        CreateSpecial(i);
      }
    }

    private void CreatePrimary(int phase)
    {
      GameObject prefab = FathomlessVoidling.voidRaidCrabPhase1;
      if (phase == 1)
        prefab = FathomlessVoidling.voidRaidCrabPhase2;
      if (phase == 2)
        prefab = FathomlessVoidling.voidRaidCrabPhase3;

      SkillLocator skillLocator = prefab.GetComponent<SkillLocator>();
      GenericSkill skill = prefab.GetComponents<GenericSkill>()[0];
      skill.skillName = "Disillusion";
      SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
      (newFamily as ScriptableObject).name = FathomlessVoidling.voidRaidCrabBase.name + "DisillusionFamily";
      newFamily.variants = new SkillFamily.Variant[1];
      skill._skillFamily = newFamily;

      SkillDef disillusion = ScriptableObject.CreateInstance<SkillDef>();
      disillusion.activationState = new EntityStates.SerializableEntityStateType(typeof(Disillusion));
      disillusion.skillNameToken = "Disillusion";
      disillusion.activationStateMachineName = "Weapon";
      disillusion.baseMaxStock = 1;
      disillusion.baseRechargeInterval = ModConfig.primCD.Value;
      disillusion.beginSkillCooldownOnSkillEnd = true;
      disillusion.canceledFromSprinting = false;
      disillusion.cancelSprintingOnActivation = false;
      disillusion.fullRestockOnAssign = true;
      disillusion.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
      disillusion.isCombatSkill = true;
      disillusion.mustKeyPress = false;
      disillusion.rechargeStock = 1;
      disillusion.requiredStock = 1;
      disillusion.stockToConsume = 1;

      newFamily.variants[0] = new SkillFamily.Variant { skillDef = disillusion };

      ContentAddition.AddSkillFamily(newFamily);
      skillLocator.primary = skill;
    }

    private void CreateSecondary(int phase)
    {
      GameObject prefab = FathomlessVoidling.voidRaidCrabPhase1;
      if (phase == 1)
        prefab = FathomlessVoidling.voidRaidCrabPhase2;
      if (phase == 2)
        prefab = FathomlessVoidling.voidRaidCrabPhase3;

      SkillLocator skillLocator = prefab.GetComponent<SkillLocator>();
      GenericSkill skill = prefab.GetComponents<GenericSkill>()[1];
      skill.skillName = "Singularity";
      SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
      (newFamily as ScriptableObject).name = FathomlessVoidling.voidRaidCrabBase.name + "SingularityFamily";
      newFamily.variants = new SkillFamily.Variant[1];
      skill._skillFamily = newFamily;

      SkillDef singularity = ScriptableObject.CreateInstance<SkillDef>();
      singularity.activationState = new EntityStates.SerializableEntityStateType(typeof(VacuumEnter));
      singularity.skillNameToken = "Singularity";
      singularity.activationStateMachineName = "Body";
      singularity.baseMaxStock = 1;
      singularity.baseRechargeInterval = ModConfig.secCD.Value;
      singularity.beginSkillCooldownOnSkillEnd = true;
      singularity.canceledFromSprinting = true;
      singularity.cancelSprintingOnActivation = true;
      singularity.fullRestockOnAssign = true;
      singularity.interruptPriority = EntityStates.InterruptPriority.Pain;
      singularity.isCombatSkill = true;
      singularity.mustKeyPress = false;
      singularity.rechargeStock = 1;
      singularity.requiredStock = 1;
      singularity.stockToConsume = 1;

      newFamily.variants[0] = new SkillFamily.Variant { skillDef = singularity };

      ContentAddition.AddSkillFamily(newFamily);
      skillLocator.secondary = skill;
    }

    private void CreateUtility(int phase)
    {
      GameObject prefab = FathomlessVoidling.voidRaidCrabPhase1;
      if (phase == 1)
        prefab = FathomlessVoidling.voidRaidCrabPhase2;
      if (phase == 2)
        prefab = FathomlessVoidling.voidRaidCrabPhase3;

      SkillLocator skillLocator = prefab.GetComponent<SkillLocator>();
      GenericSkill skill = phase == 0 ? prefab.AddComponent<GenericSkill>() : prefab.GetComponents<GenericSkill>()[2];
      skill.skillName = "Transpose";
      SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
      (newFamily as ScriptableObject).name = FathomlessVoidling.voidRaidCrabBase.name + "Transpose" + "Family";
      newFamily.variants = new SkillFamily.Variant[1];
      skill._skillFamily = newFamily;

      SkillDef transpose = ScriptableObject.CreateInstance<SkillDef>();
      transpose.activationState = new EntityStates.SerializableEntityStateType(typeof(Transpose));
      transpose.skillNameToken = "Transpose";
      transpose.activationStateMachineName = "Body";
      transpose.baseMaxStock = 1;
      transpose.baseRechargeInterval = ModConfig.utilCD.Value;
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
      skillLocator.utility = skill;
    }

    private void CreateSpecial(int phase)
    {
      GameObject prefab = FathomlessVoidling.voidRaidCrabPhase1;
      if (phase == 1)
        prefab = FathomlessVoidling.voidRaidCrabPhase2;
      if (phase == 2)
        prefab = FathomlessVoidling.voidRaidCrabPhase3;

      SkillLocator skillLocator = prefab.GetComponent<SkillLocator>();
      GenericSkill skill = phase == 0 ? prefab.GetComponents<GenericSkill>()[2] : prefab.GetComponents<GenericSkill>()[3];
      skill.skillName = "OmegaBeam";
      SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
      (newFamily as ScriptableObject).name = FathomlessVoidling.voidRaidCrabBase.name + "OmegaBeamFamily";
      newFamily.variants = new SkillFamily.Variant[1];
      skill._skillFamily = newFamily;

      SkillDef omegaBeam = ScriptableObject.CreateInstance<SkillDef>();
      omegaBeam.activationState = new EntityStates.SerializableEntityStateType(typeof(Transpose));
      omegaBeam.skillNameToken = "OmegaBeam";
      omegaBeam.activationStateMachineName = "Body";
      omegaBeam.baseMaxStock = 1;
      omegaBeam.baseRechargeInterval = ModConfig.specCD.Value;
      omegaBeam.beginSkillCooldownOnSkillEnd = true;
      omegaBeam.canceledFromSprinting = false;
      omegaBeam.cancelSprintingOnActivation = false;
      omegaBeam.fullRestockOnAssign = true;
      omegaBeam.interruptPriority = EntityStates.InterruptPriority.PrioritySkill;
      omegaBeam.isCombatSkill = true;
      omegaBeam.mustKeyPress = false;
      omegaBeam.rechargeStock = 1;
      omegaBeam.requiredStock = 1;
      omegaBeam.stockToConsume = 1;

      newFamily.variants[0] = new SkillFamily.Variant { skillDef = omegaBeam };

      ContentAddition.AddSkillFamily(newFamily);
      skillLocator.special = skill;
    }
  }
}