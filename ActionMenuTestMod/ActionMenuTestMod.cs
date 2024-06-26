﻿using System.IO;
using System.Reflection;
using ActionMenuApi.Api;
using ActionMenuApi.Pedals;
using MelonLoader;
using Il2CppInterop.Runtime;
using UnityEngine;

using VRCPlayer = Il2Cpp.MonoBehaviour1PublicOb_pOb_s_pBoGaOb_pStUnique;

[assembly: MelonInfo(typeof(ActionMenuTestMod.ActionMenuTestMod), "ActionMenuTestMod", "1.0.0", "gompo")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ActionMenuTestMod
{
    // Icons from https://uxwing.com/
    public partial class ActionMenuTestMod : MelonMod
    {
        private bool testBool2 = false;
        private bool riskyFunctionsAllowed = false;
        private static float x = 0;
        private static float y = 0;
        private static float z = 0;
        private static AssetBundle iconsAssetBundle = null;
        private static Texture2D toggleIcon;
        private static Texture2D radialIcon;
        private static Texture2D subMenuIcon;
        private static Texture2D buttonIcon;

        public override void OnInitializeMelon()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ActionMenuTestMod.customicons"))
            using (var tempStream = new MemoryStream((int)stream.Length))
            {
                stream.CopyTo(tempStream);
                iconsAssetBundle = AssetBundle.LoadFromMemory_Internal(tempStream.ToArray(), 0);
                iconsAssetBundle.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            }

            radialIcon = iconsAssetBundle.LoadAsset_Internal("Assets/Resources/Icons/sound-full.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
            radialIcon.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            toggleIcon = iconsAssetBundle.LoadAsset_Internal("Assets/Resources/Icons/zero.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
            toggleIcon.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            subMenuIcon = iconsAssetBundle.LoadAsset_Internal("Assets/Resources/Icons/file-transfer.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
            subMenuIcon.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            buttonIcon = iconsAssetBundle.LoadAsset_Internal("Assets/Resources/Icons/cloud-data-download.png", Il2CppType.Of<Texture2D>()).Cast<Texture2D>();
            buttonIcon.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            VRCActionMenuPage.AddButton(ActionMenuPage.Main, "Button", () => MelonLogger.Msg("Pressed Button"), buttonIcon);

            PedalSubMenu subMenu = VRCActionMenuPage.AddSubMenu(ActionMenuPage.Config, "Toggle", m => { }, null, true);
            subMenu.locked = true;

            AMUtils.AddToModsFolder(
                "Test Stuff",
                subMenu =>
                {
                    subMenu.AddToggle("Risky Functions", () => !riskyFunctionsAllowed, (b) =>
                    {
                        riskyFunctionsAllowed = !b;
                    });
                    //No properties here are saved because I'm lazy af
                    subMenu.AddToggle("Enable Hax", false, b => { }, buttonIcon, riskyFunctionsAllowed);
                    subMenu.AddRadialPuppet("Volume", f => { }, icon: buttonIcon, locked: riskyFunctionsAllowed);
                    subMenu.AddRestrictedRadialPuppet("Volume Restricted", f => { }, icon: buttonIcon, locked: riskyFunctionsAllowed);
                    subMenu.AddSubMenu("Whatever", m => { }, buttonIcon, riskyFunctionsAllowed);
                    subMenu.AddButton("Risky Function", () =>
                    {
                        MelonLogger.Msg("Locked Pedal Func ran");
                    }, buttonIcon, riskyFunctionsAllowed);
                    subMenu.AddFourAxisPuppet("Move", vector2 => { }, icon: toggleIcon, locked: riskyFunctionsAllowed);
                },
                subMenuIcon
            );

            AMUtils.AddToModsFolder(
                "New Cube Stuff",
                subMenu =>
                {
                    subMenu.AddFourAxisPuppet("Reposition cube X/Y", (v) => RePositionCubeXY(v), icon: buttonIcon);
                    subMenu.AddFourAxisPuppet("Reposition cube Z/Y", RePositionCubeZY, icon: toggleIcon);
                    subMenu.AddFourAxisPuppet("Reposition cube X/Z", RePositionCubeXZ, icon: toggleIcon);
                    subMenu.AddRadialPuppet("X", RotateCubeX, startingValue: x, icon: radialIcon); //Rotation a bit borked
                    subMenu.AddToggle("Test Toggle", () => testBool2, (b) => testBool2 = b);
                    subMenu.AddRadialPuppet("Y", RotateCubeY, startingValue: y, icon: radialIcon);
                    subMenu.AddRadialPuppet("Z", RotateCubeZ, startingValue: z, icon: radialIcon);
                    subMenu.AddButton("Spawn Cube", CreateCube, buttonIcon);
                    subMenu.AddButton("Tp Cube To Player", () => _controllingGameObject.transform.localPosition = VRCPlayer.field_Internal_Static_MonoBehaviour1PublicOb_pOb_s_pBoGaOb_pStUnique_0.transform.localPosition, buttonIcon);
                },
                subMenuIcon,
                false
            );


            for (int i = 0; i < 2; i++) //Set to a high number if you want to test the page functionality 
            {
                AMUtils.AddToModsFolder($"Example Mod {i + 2}", m => { }, subMenuIcon);
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyUp(KeyCode.P))
            {
                AMUtils.RefreshActionMenu();
            }

            if (Input.GetKeyUp(KeyCode.I))
            {
                AMUtils.ResetMenu();
            }
        }

        private static void CreateCube()
        {
            _controllingGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _controllingGameObject.GetComponent<Collider>().enabled = false;
            var eulerAngles = _controllingGameObject.transform.eulerAngles;
            x = eulerAngles.x * 360;
            y = eulerAngles.y * 360;
            z = eulerAngles.z * 360;
        }


        private static void RePositionCubeXY(Vector3 v)
        {
            _controllingGameObject.transform.localPosition += v / 25;
        }
        private static void RePositionCubeZY(Vector2 v)
        {
            _controllingGameObject.transform.localPosition += new Vector3(0, v.y / 25, v.x / 25);
        }
        private static void RePositionCubeXZ(Vector2 v)
        {
            _controllingGameObject.transform.localPosition += new Vector3(v.x / 25, 0, v.y / 25);
        }

        private static void RotateCubeX(float rotation)
        {
            //This is the incorrect way of rotating the gameobject and it breaks from 90-270 as quaternions and euler angle conversions are a bit fucky
            Vector3 old = _controllingGameObject.transform.eulerAngles;
            _controllingGameObject.transform.eulerAngles = new Vector3((rotation) * 360, old.y, old.z);
            x = rotation;
        }

        private static void RotateCubeY(float rotation)
        {
            Vector3 old = _controllingGameObject.transform.eulerAngles;
            //MelonLogger.Msg($"Old Angles: {old.ToString()}");
            _controllingGameObject.transform.eulerAngles = new Vector3(old.x, (rotation) * 360, old.z);
            y = rotation;
        }
        private static void RotateCubeZ(float rotation)
        {
            Vector3 old = _controllingGameObject.transform.eulerAngles;
            //MelonLogger.Msg($"Old Angles: {old.ToString()}");
            _controllingGameObject.transform.eulerAngles = new Vector3(old.x, old.y, (rotation) * 360);
            z = rotation;
        }

        private static GameObject _controllingGameObject;
    }
}