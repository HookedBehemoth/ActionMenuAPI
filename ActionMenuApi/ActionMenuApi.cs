﻿using System;
using System.Collections;
using ActionMenuApi.Managers;
using MelonLoader;

using ActionMenuDriver = MonoBehaviourPublicObGaObAcMeObEmExObPeUnique;

#pragma warning disable 1591

namespace ActionMenuApi
{
    public partial class ActionMenuApi : MelonMod
    {
        
        public override void OnInitializeMelon()
        {
            ResourcesManager.LoadTextures();
            MelonCoroutines.Start(WaitForActionMenuInit());
            try
            {
                Patches.PatchAll(HarmonyInstance);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Patching failed with exception: {e.Message}");
            }
        }

        private IEnumerator WaitForActionMenuInit()
        {
            while (ActionMenuDriver.prop_MonoBehaviourPublicObGaObAcMeObEmExObPeUnique_0 == null) //VRCUIManager Init is too early 
                yield return null;
            if (string.IsNullOrEmpty(ID)) yield break;
            ResourcesManager.InitLockGameObject();
            RadialPuppetManager.Setup();
            FourAxisPuppetManager.Setup();
        }
        
        public override void OnUpdate()
        {
            RadialPuppetManager.OnUpdate();
            FourAxisPuppetManager.OnUpdate();
        }
    }
}
