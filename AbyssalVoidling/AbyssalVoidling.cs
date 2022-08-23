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
    [BepInPlugin("com.Nuxlar.AbyssalVoidling", "AbyssalVoidling", "1.0.0")]

    public class AbyssalVoidling : BaseUnityPlugin
    {
        GameObject voidRaidCrabPhase1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase1.prefab").WaitForCompletion();
        GameObject voidRaidCrabPhase2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase2.prefab").WaitForCompletion();
        GameObject voidRaidCrabPhase3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion();

        public void Awake()
        {
            On.RoR2.Run.Start += OnRunStart;
        }

        private void AdjustPhase1Stats()
        {
            CharacterBody voidRaidCrabBody = voidRaidCrabPhase1.GetComponent<CharacterBody>();

            voidRaidCrabBody.baseMaxHealth = 1400;

            voidRaidCrabBody.baseAttackSpeed = 1.25f;
            voidRaidCrabBody.baseMoveSpeed = 100;
            voidRaidCrabBody.baseAcceleration = 40;
            ProjectileSteerTowardTarget voidRaidMissiles = new FireMissiles().projectilePrefab.GetComponent<ProjectileSteerTowardTarget>();
            voidRaidMissiles.rotationSpeed = 180;

            /** 
                accel 20
                armor 20
                attkspd 1
                dmg 15
                HP 2000
                movespd 45
                jump 0
                projectile rotation 180
            **/
        }

        private void OnRunStart(On.RoR2.Run.orig_Start orig, Run self)
        {
            AdjustPhase1Stats();
            orig(self);
        }

    }
}