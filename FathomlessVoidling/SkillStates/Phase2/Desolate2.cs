using RoR2;
using RoR2.Projectile;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.BrotherHaunt;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.BrotherMonster;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using EntityStates.BrotherMonster.Weapon;
using System.Collections.Generic;

namespace FathomlessVoidling
{
  public class Desolate2 : BaseState
  {
    private float stopwatch;
    private float missileStopwatch;
    public float baseDuration = 6;
    public static string muzzleString = BaseMultiBeamState.muzzleName;
    public static float missileSpawnFrequency = 2;
    public static float missileSpawnDelay = 0;
    public static float damageCoefficient;
    public static float maxSpread = 1;
    public static float blastDamageCoefficient = 1;
    public static float blastForceMagnitude = 3000;
    public static float blastRadius = 6;
    public static Vector3 blastBonusForce = new Vector3(0.0f, 100.0f, 0.0f);
    public static string muzzleName = "EyeProjectileCenter";
    public static string animationLayerName = "Gesture";
    public static string animationStateName = "FireMultiBeamFinale";
    public static string animationPlaybackRateParam = "MultiBeam.playbackRate";
    public static GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/TracerVoidRaidCrabTripleBeamSmall.prefab").WaitForCompletion();
    public static GameObject explosionEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamExplosion.prefab").WaitForCompletion();
    public static GameObject muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamMuzzleflash.prefab").WaitForCompletion();
    private float duration;
    private static System.Random rand = new();

    // Animate Voidling spinbeam OnEnter
    // Fire random lasers from ground
    // Remove lasers
    // Animate Voidling spinbeam OnExit

    protected Transform muzzleTransform { get; private set; }

    // Play_voidRaid_snipe_shoot_final
    public override void OnEnter()
    {
      base.OnEnter();
      this.missileStopwatch -= missileSpawnDelay;
      this.duration = this.baseDuration / this.attackSpeedStat;
      this.muzzleTransform = this.FindModelChild(Desolate.muzzleName);
    }
    private void FireProjectile()
    {
      NodeGraph groundNodes = SceneInfo.instance.groundNodes;
      if (!(bool)(Object)groundNodes)
        return;
      Transform donutCenter = VoidRaidGauntletController.instance.currentDonut.root.transform.Find("ReflectionProbe, Center");
      if (!(bool)donutCenter)
        return;
      List<NodeGraph.NodeIndex> withFlagConditions = groundNodes.FindNodesInRangeWithFlagConditions(donutCenter.position, 0, 400, HullMask.Golem, NodeFlags.None, NodeFlags.NoCharacterSpawn, true);
      NodeGraph.NodeIndex nodeIndex = withFlagConditions[Random.Range(0, withFlagConditions.Count)];
      Vector3 position;
      groundNodes.GetNodePosition(nodeIndex, out position);
      GameObject spinBeamVFXInstance = GameObject.Instantiate(FathomlessVoidling.spinBeamVFX, position, Quaternion.LookRotation(Vector3.up));
      NetworkServer.Spawn(spinBeamVFXInstance);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      this.stopwatch += Time.fixedDeltaTime;
      this.missileStopwatch += Time.fixedDeltaTime;
      if ((double)this.missileStopwatch >= 1.0 / (double)missileSpawnFrequency)
      {
        this.missileStopwatch -= 1f / missileSpawnFrequency;
        this.FireProjectile();
        if ((double)this.stopwatch < (double)this.baseDuration || !this.isAuthority)
          return;
        this.outer.SetNextStateToMain();
      }
    }
  }
}