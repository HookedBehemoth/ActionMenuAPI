using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ActionMenuApi.Helpers;
using ActionMenuApi.Managers;
using ActionMenuApi.Pedals;
using HarmonyLib;
using MelonLoader;

namespace ActionMenuApi
{
    [SuppressMessage("ReSharper", "Unity.IncorrectMonoBehaviourInstantiation")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class Patches
    {
        public static List<PedalStruct>
            configPagePre = new(),
            configPagePost = new(),
            emojisPagePre = new(),
            emojisPagePost = new(),
            expressionPagePre = new(),
            expressionPagePost = new(),
            sdk2ExpressionPagePre = new(),
            sdk2ExpressionPagePost = new(),
            mainPagePre = new(),
            mainPagePost = new(),
            nameplatesPagePre = new(),
            nameplatesPagePost = new(),
            nameplatesVisibilityPagePre = new(),
            nameplatesVisibilityPagePost = new(),
            nameplatesSizePagePre = new(),
            nameplatesSizePagePost = new(),
            optionsPagePre = new(),
            optionsPagePost = new();

        private static readonly List<string> openConfigPageKeyWords = new() {"Avatar Overlay"}; // new(new[] {"Menu Size", "Menu Opacity"}); Those are in sub-functions now
        private static readonly List<string> openMainPageKeyWords = new(new[] {"Options", "Emojis"});
        private static readonly List<string> openEmojisPageKeyWords = new(new[] {" ", "_"});
        private static readonly List<string> openExpressionMenuKeyWords = new(new[] {"Reset Avatar", "Release Poses"});
        private static readonly List<string> openOptionsPageKeyWords = new(new[] {"Chatbox", "Nameplates"});
        private static readonly List<string> openSDK2ExpressionPageKeyWords = new(new[] {"EMOTE{0}"});

        private static readonly List<string> openNameplatesPageKeyWords = new(new[] {"Visibility", "Size"});

        private static readonly List<string> openNameplatesVisibilityPageKeyWords =
            new(new[] {"Nameplates Shown", "Icons Only", "Nameplates Hidden"});

        private static readonly List<string> openNameplatesSizePageKeyWords =
            new(new[] {"Large", "Medium", "Normal", "Small", "Tiny"});

        private static readonly List<string> openPhysbonesSettingsPageWords = new()
        {
            "None", "PhysBones Proximity", "PhysBones", "Contacts"
        };

        private static HarmonyLib.Harmony Harmony;

        public static void PatchAll(HarmonyLib.Harmony harmonyInstance)
        {

            // Il2Cpp.MonoBehaviourPublicIPhysBoneDebugDrawerObSiObCoSiLiObCo1SiUnique.EnumNPublicSealedvaNoPhCoPh5vUnique
            Harmony = harmonyInstance;
            PatchMethod(openExpressionMenuKeyWords, nameof(OpenExpressionMenuPre), nameof(OpenExpressionMenuPost));
            PatchMethod(openConfigPageKeyWords, nameof(OpenConfigPagePre), nameof(OpenConfigPagePost));
            PatchMethod(openMainPageKeyWords, nameof(OpenMainPagePre), nameof(OpenMainPagePost));
            PatchMethod(openEmojisPageKeyWords, nameof(OpenEmojisPagePre), nameof(OpenEmojisPagePost));
            PatchMethod(openNameplatesPageKeyWords, nameof(OpenNameplatesPagePre), nameof(OpenNameplatesPagePost));
            PatchMethod(openNameplatesSizePageKeyWords, nameof(OpenNameplatesSizePre), nameof(OpenNameplatesSizePost));
            PatchMethod(openNameplatesVisibilityPageKeyWords, nameof(OpenNameplatesVisibilityPre), nameof(OpenNameplatesVisibilityPost));
            PatchMethod(openSDK2ExpressionPageKeyWords, nameof(OpenSDK2ExpressionPre), nameof(OpenSDK2ExpressionPost));
            PatchMethod(openOptionsPageKeyWords, nameof(OpenOptionsPre), nameof(OpenOptionsPost));

            MelonLogger.Msg("Patches Applied");
        }

        public static void OpenConfigPagePre(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(configPagePre, __instance);
        }

        public static void OpenConfigPagePost(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(configPagePost, __instance);
        }

        public static void OpenMainPagePre(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(mainPagePre, __instance);
        }

        public static void OpenMainPagePost(ActionMenu __instance)
        {
            if (ModsFolderManager.mods.Count > 0) ModsFolderManager.AddMainPageButton(__instance);
            Utilities.AddPedalsInList(mainPagePost, __instance);
        }

        public static void OpenEmojisPagePre(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(emojisPagePre, __instance);
        }

        public static void OpenEmojisPagePost(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(emojisPagePost, __instance);
        }

        public static void OpenExpressionMenuPre(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(expressionPagePre, __instance);
        }

        public static void OpenExpressionMenuPost(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(expressionPagePost, __instance);
        }

        public static void OpenNameplatesPagePre(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(nameplatesPagePre, __instance);
        }

        public static void OpenNameplatesPagePost(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(nameplatesPagePost, __instance);
        }

        public static void OpenNameplatesVisibilityPre(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(nameplatesVisibilityPagePre, __instance);
        }

        public static void OpenNameplatesVisibilityPost(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(nameplatesVisibilityPagePost, __instance);
        }

        public static void OpenNameplatesSizePre(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(nameplatesSizePagePre, __instance);
        }

        public static void OpenNameplatesSizePost(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(nameplatesSizePagePost, __instance);
        }

        public static void OpenOptionsPre(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(optionsPagePre, __instance);
        }

        public static void OpenOptionsPost(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(optionsPagePost, __instance);
        }

        public static void OpenSDK2ExpressionPre(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(sdk2ExpressionPagePre, __instance);
        }

        public static void OpenSDK2ExpressionPost(ActionMenu __instance)
        {
            Utilities.AddPedalsInList(sdk2ExpressionPagePost, __instance);
        }

        private static MethodInfo FindAMMethod(List<string> keywords)
        {
            return typeof(ActionMenu).GetMethods()
                .First(m => m.Name.StartsWith("Method") && Utilities.CheckXref(m, keywords));
        }

        private static void PatchMethod(List<string> keywords, string preName, string postName)
        {
            try
            {
                Harmony.Patch(
                    FindAMMethod(keywords),
                    new HarmonyMethod(typeof(Patches).GetMethod(preName)),
                    new HarmonyMethod(typeof(Patches).GetMethod(postName))
                );
            }
            catch (Exception e)
            {
                MelonLogger.Warning($"Failed to Patch Method: {preName} <-> {postName} with {string.Join(", ", keywords)}: {e}");
            }
        }
    }
}