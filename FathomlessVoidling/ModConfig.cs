using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;

namespace FathomlessVoidling
{
  internal class ModConfig
  {
    // General
    public static ConfigEntry<bool> enableAltMoon;
    public static ConfigEntry<bool> enableVoidFog;
    // Stats
    public static ConfigEntry<float> baseHealth;
    public static ConfigEntry<float> levelHealth;
    public static ConfigEntry<float> baseDamage;
    public static ConfigEntry<float> levelDamage;
    public static ConfigEntry<float> baseArmor;
    public static ConfigEntry<float> baseAtkSpd;
    public static ConfigEntry<float> baseSpd;
    public static ConfigEntry<float> acceleration;
    // Skills
    public static ConfigEntry<int> primCD;
    public static ConfigEntry<int> secCD;
    public static ConfigEntry<int> utilCD;
    public static ConfigEntry<int> specCD;



    public static void InitConfig(ConfigFile config)
    {
      enableAltMoon = config.Bind("General", "Alt Moon", true, "Toggle Void Locus as an alternative to the moon. Adds stage 5 Locus portal and item cauldrons.");
      enableVoidFog = config.Bind("General", "Pillar Fog", false, "Toggle void pillar fog.");

      baseHealth = config.Bind("Stats", "Base Health", 1250f, "Vanilla: 2000");
      levelHealth = config.Bind("Stats", "Level Health", 350f, "Health gained per level. Vanilla: 600");
      baseDamage = config.Bind("Stats", "Base Damage", 15f, "Vanilla: 15");
      levelDamage = config.Bind("Stats", "Level Damage", 3f, "Damage gained per level. Vanilla: 3");
      baseArmor = config.Bind("Stats", "Base Armor", 30f, "Vanilla: 20");
      baseAtkSpd = config.Bind("Stats", "Base Attack Speed", 1.25f, "Vanilla: 1");
      baseSpd = config.Bind("Stats", "Base Move Speed", 90f, "Vanilla: 45");
      acceleration = config.Bind("Stats", "Acceleration", 45f, "Vanilla: 20");

      primCD = config.Bind("Skills", "Primary Cooldown", 10, "Cooldown for Disillusion (Main missile attack).");
      secCD = config.Bind("Skills", "Secondary Cooldown", 40, "Cooldown for Secondary (Vacuum, Singularity, Crush).");
      utilCD = config.Bind("Skills", "Util Cooldown", 20, "Cooldown for Transpose (Blink).");
      specCD = config.Bind("Skills", "Special Cooldown", 30, "Cooldown for Special (Rend, SpinBeam, Reap).");

      // Risk Of Options Setup
      // General
      ModSettingsManager.AddOption(new CheckBoxOption(enableAltMoon));
      ModSettingsManager.AddOption(new CheckBoxOption(enableVoidFog));
      // Stats
      ModSettingsManager.AddOption(new StepSliderOption(baseHealth, new StepSliderConfig() { min = 1000, max = 2000, increment = 50f }));
      ModSettingsManager.AddOption(new StepSliderOption(levelHealth, new StepSliderConfig() { min = 100, max = 500, increment = 25f }));
      ModSettingsManager.AddOption(new StepSliderOption(baseDamage, new StepSliderConfig() { min = 10, max = 20, increment = 0.5f }));
      ModSettingsManager.AddOption(new StepSliderOption(levelDamage, new StepSliderConfig() { min = 1, max = 6f, increment = 0.25f }));
      ModSettingsManager.AddOption(new StepSliderOption(baseArmor, new StepSliderConfig() { min = 20, max = 60, increment = 5f }));
      ModSettingsManager.AddOption(new StepSliderOption(baseAtkSpd, new StepSliderConfig() { min = 0.5f, max = 2, increment = 0.25f }));
      ModSettingsManager.AddOption(new StepSliderOption(baseSpd, new StepSliderConfig() { min = 45, max = 135, increment = 5f }));
      ModSettingsManager.AddOption(new StepSliderOption(acceleration, new StepSliderConfig() { min = 20, max = 70, increment = 5f }));
      // Skills
      ModSettingsManager.AddOption(new IntSliderOption(primCD, new IntSliderConfig() { min = 1, max = 10 }));
      ModSettingsManager.AddOption(new IntSliderOption(secCD, new IntSliderConfig() { min = 30, max = 50 }));
      ModSettingsManager.AddOption(new IntSliderOption(utilCD, new IntSliderConfig() { min = 10, max = 30 }));
      ModSettingsManager.AddOption(new IntSliderOption(specCD, new IntSliderConfig() { min = 20, max = 40 }));

      ModSettingsManager.SetModDescription("Voidling without fathoms");
    }
  }
}
