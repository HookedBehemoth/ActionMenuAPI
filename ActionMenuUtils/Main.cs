using System;
using System.IO;
using System.Reflection;
using ActionMenuApi.Api;
using MelonLoader;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace ActionMenuUtils
{
    public partial class Main : MelonMod
    {
        private static AssetBundle iconsAssetBundle;
        private static Texture2D respawnIcon;
        private static Texture2D helpIcon;
        private static Texture2D goHomeIcon;
        private static Texture2D resetAvatarIcon;
        private static Texture2D rejoinInstanceIcon;

        public override void OnInitializeMelon()
        {
            try
            {
                //Adapted from knah's JoinNotifier mod found here: https://github.com/knah/VRCMods/blob/master/JoinNotifier/JoinNotifierMod.cs 
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ActionMenuUtils.icons"))
                {
                    using var tempStream = new MemoryStream((int)stream.Length);
                    stream.CopyTo(tempStream);

                    iconsAssetBundle = AssetBundle.LoadFromMemory_Internal(tempStream.ToArray(), 0);
                    iconsAssetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                }

                respawnIcon = iconsAssetBundle.LoadAsset_Internal("Assets/Resources/Refresh.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                respawnIcon.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                helpIcon = iconsAssetBundle.LoadAsset_Internal("Assets/Resources/Help.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                helpIcon.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                goHomeIcon = iconsAssetBundle.LoadAsset_Internal("Assets/Resources/Home.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                goHomeIcon.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                resetAvatarIcon = iconsAssetBundle.LoadAsset_Internal("Assets/Resources/Avatar.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                resetAvatarIcon.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                rejoinInstanceIcon = iconsAssetBundle.LoadAsset_Internal("Assets/Resources/Pin.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
                rejoinInstanceIcon.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            }
            catch (Exception e)
            {
                MelonLogger.Warning("Consider checking for newer version as mod possibly no longer working, Exception occured OnAppStart(): " + e.Message);
            }
            ModSettings.RegisterSettings();
            ModSettings.Apply();
            SetupAMAPIButtons();
        }

        public static GameObject UIXAvatarMenuButton;

        private static void SetupAMAPIButtons()
        {
            VRCActionMenuPage.AddSubMenu(ActionMenuPage.Options, "SOS", DoShit, helpIcon);
        }

        private static void DoShit(CustomSubMenu menu) {
            //Respawn
            if (ModSettings.confirmRespawn)
                menu.AddSubMenu("Respawn",
                    menu => menu.AddButton("Confirm Respawn", Utils.Respawn, respawnIcon),
                    respawnIcon
                );
            else
                menu.AddButton("Respawn", Utils.Respawn, respawnIcon);

            //Instance Rejoin
            if (ModSettings.confirmInstanceRejoin)
                menu.AddSubMenu("Rejoin Instance",
                    menu => menu.AddButton("Confirm Instance Rejoin", Utils.RejoinInstance, rejoinInstanceIcon),
                    rejoinInstanceIcon
                );
            else
                menu.AddButton("Rejoin Instance", Utils.RejoinInstance, rejoinInstanceIcon);

            //Go Home
            if (ModSettings.confirmGoHome)
                menu.AddSubMenu("Go Home",
                    menu => menu.AddButton("Confirm Go Home", Utils.Home, goHomeIcon),
                    goHomeIcon
                );
            else
                menu.AddButton("Go Home", Utils.Home, goHomeIcon);
        }

        public override void OnPreferencesLoaded() => ModSettings.Apply();
        public override void OnPreferencesSaved() => ModSettings.Apply();
    }
}