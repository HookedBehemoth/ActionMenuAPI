using System;
using System.Collections;
using ActionMenuApi.Managers;
using MelonLoader;

using ActionMenuDriver = Il2Cpp.MonoBehaviourPublicObGaObAc1ObAcBoCoObUnique;

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
                MelonLogger.Error($"Patching failed with exception", e);
            }
        }

        private IEnumerator WaitForActionMenuInit()
        {
            // while (ActionMenuDriver.prop_MonoBehaviourPublicObGaObAcCoObMeEmObExUnique_0 == null) //VRCUIManager Init is too early 
            while (ActionMenuDriver.field_Public_Static_MonoBehaviourPublicObGaObAc1ObAcBoCoObUnique_0 == null) //VRCUIManager Init is too early 
                yield return null;
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
