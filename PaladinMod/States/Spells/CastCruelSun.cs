﻿using PaladinMod.Misc;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace PaladinMod.States.Spell
{
    public class CastCruelSun : BaseCastChanneledSpellState
    {
        protected GameObject sunInstance;
        private Vector3? sunSpawnPosition;

        public override void OnEnter()
        {
            this.baseDuration = this.overrideDuration = StaticValues.cruelSunDuration;
            this.muzzleflashEffectPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/ExplosionSolarFlare");
            this.projectilePrefab = null;
            this.castSoundString = Modules.Sounds.CastTorpor;

            base.OnEnter();

            if (NetworkServer.active)
            {
                this.sunSpawnPosition = this.characterBody.corePosition + new Vector3(0f, 10f, 0f);
                if (this.sunSpawnPosition != null) this.sunInstance = this.SpawnPaladinSun(this.sunSpawnPosition.Value);
            }

            //What does this do??? It's VFX but
            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = this.baseDuration;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = RoR2.LegacyResourcesAPI.Load<Material>("Materials/matGrandparentTeleportOutBoom");
                temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }

            //borked, no idea why
            //base.camParamsOverrideHandle = Modules.CameraParams.OverridePaladinCameraParams(base.cameraTargetParams, PaladinCameraParams.CRUEL_SUN, 1f);
        }

        public override void FixedUpdate()
        {
            if (base.isAuthority && base.inputBank && base.fixedAge >= 0.2f)
            {
                if (base.inputBank.sprint.wasDown)
                {
                    base.characterBody.isSprinting = true;
                    this.outer.SetNextStateToMain();
                    return;
                }
            }

            base.FixedUpdate();
        }

        protected override void PlayCastAnimation()
        {
            base.PlayAnimation("Gesture, Override", "CastSun", "Spell.playbackRate", 0.25f);
        }

        public override void OnExit()
        {
            if (NetworkServer.active && this.sunInstance)
            {
                this.sunInstance.GetComponent<GenericOwnership>().ownerObject = null;
                this.sunInstance = null;
            }

            base.OnExit();

            base.PlayAnimation("Gesture, Override", "CastSunEnd", "Spell.playbackRate", 0.8f);
        }

        private GameObject SpawnPaladinSun(Vector3 spawnPosition)
        {
            GameObject sun = UnityEngine.Object.Instantiate<GameObject>(Modules.Assets.paladinSunPrefab, spawnPosition, Quaternion.identity, this.characterBody.transform);
            sun.GetComponent<GenericOwnership>().ownerObject = base.gameObject;
            NetworkServer.Spawn(sun);
            return sun;
        }
    }
}