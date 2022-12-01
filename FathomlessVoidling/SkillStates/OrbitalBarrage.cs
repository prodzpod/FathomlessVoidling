using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using EntityStates;
using EntityStates.VagrantMonster.Weapon;
using EntityStates.BrotherMonster;
using EntityStates.VoidRaidCrab;
using EntityStates.VoidRaidCrab.Weapon;
using EntityStates.BrotherMonster.Weapon;
using System.Collections.Generic;

namespace FathomlessVoidling
{
  public class OrbitalBarrage : BaseState
  {
    private float stopwatch;
    private float missileStopwatch;
    public static float baseDuration = 4;
    public static string muzzleString = BaseMultiBeamState.muzzleName;
    public static float missileSpawnFrequency = 6;
    public static float missileSpawnDelay = 0;
    public static float damageCoefficient;
    public static float maxSpread = 1;
    public static GameObject projectilePrefab;
    public static GameObject muzzleflashPrefab;
    private ChildLocator childLocator;
    static System.Random rand = new();
    GameObject meteor = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab").WaitForCompletion();
    GameObject meteorGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulderGhost.prefab").WaitForCompletion();
    GameObject vagrantOrb = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantCannon.prefab").WaitForCompletion();
    GameObject portal = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabSpawnEffect.prefab").WaitForCompletion();
    Material boulderMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Grandparent/matGrandparentBoulderProjectile.mat").WaitForCompletion();
    Material voidAffixMat = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/EliteVoid/matEliteVoidOverlay.mat").WaitForCompletion();

    public override void OnEnter()
    {
      base.OnEnter();

      ProjectileController meteorController = vagrantOrb.GetComponent<ProjectileController>();
      meteorController.cannotBeDeleted = true;
      meteor.transform.localScale = new Vector3(2, 2, 2);
      meteorGhost.transform.localScale = new Vector3(2, 2, 2);
      meteorGhost.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials = new Material[] { boulderMat, voidAffixMat };
      this.missileStopwatch -= missileSpawnDelay;
      if ((bool)(Object)this.sfxLocator && this.sfxLocator.barkSound != "")
      {
        int num1 = (int)Util.PlaySound(this.sfxLocator.barkSound, this.gameObject);
      }
      Transform modelTransform = this.GetModelTransform();
      if (!(bool)(Object)modelTransform)
        return;
      this.childLocator = modelTransform.GetComponent<ChildLocator>();
      if (!(bool)(Object)this.childLocator)
        return;
      int num2 = (bool)(Object)this.childLocator.FindChild(muzzleString) ? 1 : 0;
    }
    private void FireBlob(Ray projectileRay, float bonusPitch, float bonusYaw)
    {
      GameObject portalInstance = GameObject.Instantiate(portal, projectileRay.origin, Util.QuaternionSafeLookRotation(projectileRay.direction));
      // portalInstance.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
      NetworkServer.Spawn(portalInstance);
      FireProjectileInfo noblePhantasm = new FireProjectileInfo()
      {
        position = projectileRay.origin,
        rotation = Util.QuaternionSafeLookRotation(projectileRay.direction),
        crit = Util.CheckRoll(this.critStat, this.characterBody.master),
        damage = this.damageStat * FistSlam.waveProjectileDamageCoefficient,
        owner = this.gameObject,
        force = EntityStates.BrotherMonster.FistSlam.waveProjectileForce,
        speedOverride = 200,
        projectilePrefab = meteor
      };
      ProjectileManager.instance.FireProjectile(noblePhantasm);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      this.stopwatch += Time.fixedDeltaTime;
      this.missileStopwatch += Time.fixedDeltaTime;
      if ((double)this.missileStopwatch >= 1.0 / (double)missileSpawnFrequency)
      {
        this.missileStopwatch -= 1f / missileSpawnFrequency;
        Transform child = this.childLocator.FindChild(muzzleString);
        if ((bool)(Object)child)
        {
          Ray aimRay = this.GetAimRay();
          Ray projectileRay = new Ray();
          projectileRay.direction = aimRay.direction;
          float maxDistance = 1000f;
          float randX = UnityEngine.Random.Range(-100f, 100f);
          float randY = UnityEngine.Random.Range(0f, 50f);
          float randZ = UnityEngine.Random.Range(-100f, 100f);
          Vector3 randVector = new Vector3(randX, randY, randZ);
          Vector3 position = new Vector3(child.position.x, child.position.y, child.position.z) + randVector;
          projectileRay.origin = position;
          RaycastHit hitInfo;
          {
            if (Physics.Raycast(aimRay, out hitInfo, maxDistance, (int)LayerIndex.world.mask))
              projectileRay.direction = hitInfo.point - projectileRay.origin;
            this.FireBlob(projectileRay, 0.0f, 0.0f);
          }
        }
        if ((double)this.stopwatch < (double)baseDuration || !this.isAuthority)
          return;
        this.outer.SetNextStateToMain();
      }
    }
  }
}