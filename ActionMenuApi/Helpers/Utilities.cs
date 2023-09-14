using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ActionMenuApi.Pedals;
using ActionMenuApi.Types;
using HarmonyLib;
using MelonLoader;
using Il2CppInterop.Common.XrefScans;
using Il2CppInterop.Runtime.XrefScans;
using UnityEngine;
using Object = UnityEngine.Object;

using ActionMenuDriver = Il2Cpp.MonoBehaviourPublicObGaObAc1ObAcBoCoObUnique;
using ActionMenuOpener = Il2Cpp.MonoBehaviourPublicCaObAc1BoSiBoObObObUnique;
using ActionMenu = Il2Cpp.MonoBehaviourPublicGaTeGaCaObGaCaLiOb1Unique;
using Il2CppInterop.Runtime.Runtime;

namespace ActionMenuApi.Helpers
{
    internal static class Utilities
    {
        private static RefreshAMDelegate refreshAMDelegate;

        private static RefreshAMDelegate GetRefreshAMDelegate
        {
            get
            {
                //Build 1296 menu.Method_Public_Void_PDM_2
                if (refreshAMDelegate != null) return refreshAMDelegate;
                var refreshAMMethod = typeof(ActionMenu).GetMethods().First(
                    m =>
                        m.Name.StartsWith("Method_Public_Void_PDM_")
                        && !m.HasStringLiterals()
                        && m.SameClassMethodCallCount(1)
                        && m.HasMethodCallWithName("ThrowArgumentOutOfRangeException")
                        && !m.HasMethodWithDeclaringType(typeof(ActionMenuDriver))
                );
                refreshAMDelegate = (RefreshAMDelegate)Delegate.CreateDelegate(
                    typeof(RefreshAMDelegate),
                    null,
                    refreshAMMethod);
                return refreshAMDelegate;
            }
        }

        public static bool CheckXref(MethodBase m, params string[] keywords)
        {
            try
            {
                foreach (var keyword in keywords)
                    if (!XrefScanner.XrefScan(m).Any(
                        instance => instance.Type == XrefType.Global && instance.ReadAsObject() != null && instance
                            .ReadAsObject().ToString()
                            .Equals(keyword, StringComparison.OrdinalIgnoreCase)))
                        return false;

                return true;
            }
            catch
            {
            }

            return false;
        }

        public static bool CheckXref(MethodBase m, List<string> keywords)
        {
            try
            {
                foreach (var keyword in keywords)
                    if (!XrefScanner.XrefScan(m).Any(
                        instance => instance.Type == XrefType.Global && instance.ReadAsObject() != null && instance
                            .ReadAsObject().ToString()
                            .Equals(keyword, StringComparison.OrdinalIgnoreCase)))
                        return false;

                    // if (!XrefScanner.XrefScan(m).Any(
                    //     instance => {
                    //         // if(instance.Type != XrefType.Global) return false;
                    //         // if (instance.Pointer == IntPtr.Zero) return false;
                    //         // var obj = instance.ReadAsObject(); 
                    //         // if (obj == null) return false;
                    //         // var klazz = Il2CppInterop.Runtime.IL2CPP.il2cpp_object_get_class(obj.Pointer);
                    //         // if (klazz != Il2CppInterop.Runtime.Il2CppClassPointerStore<Il2CppSystem.String>.NativeClassPtr) return false;
                    //         // var str = obj.TryCast<Il2CppSystem.String>();
                    //         // if (str == null) return false;
                    //         // Il2CppInterop.Runtime.IL2CPP.Il2CppObjectBaseToPtr()
                    //         // var str2 = (string)new Il2CppSystem.String(obj.Pointer);
                    //         // return str2.Equals(keyword, StringComparison.OrdinalIgnoreCase);
                    //         return false;
                    //     }))
                    //     return false;
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Msg(e);
            }

            return false;
        }

        public static void AddPedalsInList(List<PedalStruct> list, ActionMenu instance)
        {
            foreach (var pedalStruct in list)
            {
                if (!pedalStruct.shouldAdd) continue;
                var pedalOption = instance.AddOption();
                pedalOption.SetText(pedalStruct.text);
                if (!pedalStruct.locked) pedalOption.SetPedalAction(delegate { pedalStruct.triggerEvent.Invoke(instance); });
                else pedalOption.Lock();
                //Additional setup for pedals
                switch (pedalStruct.Type)
                {
                    /*case PedalType.SubMenu:
                        pedalOption.SetPedalTypeIcon(GetExpressionsIcons().typeFolder);
                        break;*/
                    case PedalType.RadialPuppet:
                        var pedalRadial = (PedalRadial)pedalStruct;
                        pedalOption.SetPedalTypeIcon(GetExpressionsIcons().typeRadial);
                        pedalOption.SetButtonPercentText($"{Math.Round(pedalRadial.currentValue)}%");
                        pedalRadial.pedal = pedalOption;
                        pedalOption.SetBackgroundIcon(pedalStruct.icon);
                        break;
                    case PedalType.Toggle:
                        var pedalToggle = (PedalToggle)pedalStruct;
                        if (pedalToggle.toggled)
                            pedalOption.SetPedalTypeIcon(GetExpressionsIcons().typeToggleOn);
                        else
                            pedalOption.SetPedalTypeIcon(GetExpressionsIcons().typeToggleOff);
                        pedalToggle.pedal = pedalOption;
                        pedalOption.SetBackgroundIcon(pedalStruct.icon);
                        break;
                    case PedalType.FourAxisPuppet:
                        pedalOption.SetPedalTypeIcon(GetExpressionsIcons().typeAxis);
                        pedalOption.SetBackgroundIcon(pedalStruct.icon);
                        break;
                    default:
                        pedalOption.SetForegroundIcon(pedalStruct.icon);
                        break;
                }
            }
        }

        public static float ConvertFromDegToEuler(float angle)
        {
            //TODO: Rewrite/Remove Unnecessary Addition/Subtraction
            if (angle >= 0 && angle <= 90) return 90 - angle;
            if (angle > 90 && angle <= 180) return 360 - (angle - 90);
            if (angle <= -90 && angle >= -180) return 270 - (angle + 180);
            if (angle <= 0 && angle >= -90) return 180 - (angle + 180) + 90;
            return 0;
        }

        public static float ConvertFromEuler(float angle)
        {
            //TODO: Rewrite/Remove Unnecessary Addition/Subtraction
            if (angle >= 90 && angle <= 270) return (angle - 90) * -1;
            if (angle <= 360 && angle > 270) return 180 - (angle - 270);
            if (angle < 90 && angle >= 0) return 90 - angle;
            return 0;
        }

        public static GameObject CloneActionMenuGameObject(string pathToGameObject, string pathToParent)
        {
            var driver = GetDriver().transform;
            var go = driver.Find(pathToGameObject);
            var parent = driver.Find(pathToParent);
            return Object
                .Instantiate(go, parent)
                .gameObject;
        }

        public static ActionMenuDriver.ExpressionIcons GetExpressionsIcons()
        {
            var driver = GetDriver();
            return driver.field_Public_ExpressionIcons_0;
        }

        // Didnt know what to name this function
        public static ActionMenuHand GetActionMenuHand()
        {
            var driver = GetDriver();
            if (!driver.GetLeftOpener().isOpen() &&
                driver.GetRightOpener().isOpen())
                return ActionMenuHand.Right;

            if (driver.GetLeftOpener().isOpen() &&
                !driver.GetRightOpener().isOpen())
                return ActionMenuHand.Left;

            return ActionMenuHand.Invalid;
        }

        public static ActionMenuOpener GetActionMenuOpener()
        {
            var driver = GetDriver();
            if (!driver.GetLeftOpener().isOpen() &&
                driver.GetRightOpener().isOpen())
                return driver.GetRightOpener();

            if (driver.GetLeftOpener().isOpen() &&
                !driver.GetRightOpener().isOpen())
                return driver.GetLeftOpener();

            return null;
            /*
            else if (ActionMenuDriver._instance.openerL.isOpen() && ActionMenuDriver._instance.openerR.isOpen())
            {
                return null; //Which one to return ¯\_(ツ)_/¯ Mystery till I figure something smart out
            }
            */
        }

        public static void ScanMethod(MethodInfo m)
        {
            MelonLogger.Msg($"Scanning: {m.FullDescription()}");
            foreach (var instance in XrefScanner.XrefScan(m))
                try
                {
                    if (instance.Type == XrefType.Global && instance.ReadAsObject() != null)
                        try
                        {
                            MelonLogger.Msg($"   Found String: {instance.ReadAsObject().ToString()}");
                        }
                        catch
                        {
                        }
                    else if (instance.Type == XrefType.Method && instance.TryResolve() != null)
                        try
                        {
                            MelonLogger.Msg($"   Found Method: {instance.TryResolve().FullDescription()}");
                        }
                        catch
                        {
                        }
                }
                catch
                {
                }

            foreach (var instance in XrefScanner.UsedBy(m))
                try
                {
                    if (instance.Type == XrefType.Method && instance.TryResolve() != null)
                        try
                        {
                            MelonLogger.Msg($"   Found Used By Method: {instance.TryResolve().FullDescription()}");
                        }
                        catch
                        {
                        }
                }
                catch
                {
                }
        }

        public static void RefreshAM()
        {
            var driver = GetDriver();
            if (driver == null)
            {
                Logger.LogWarning("Refresh called before driver init");
                return;
            }

            var leftOpener = driver.GetLeftOpener();
            GetRefreshAMDelegate(leftOpener.GetActionMenu());
            var rightOpener = driver.GetRightOpener();
            GetRefreshAMDelegate(rightOpener.GetActionMenu());
        }

        public static void ResetMenu()
        {
            var driver = GetDriver();
            if (driver == null)
            {
                Logger.LogWarning("Reset called before driver init");
                return;
            }

            var leftOpener = driver.GetLeftOpener();
            leftOpener.GetActionMenu().Reset();

            var rightOpener = driver.GetRightOpener();
            rightOpener.GetActionMenu().Reset();

        }

        public static ActionMenuDriver GetDriver()
        {
            // return ActionMenuDriver.field_Public_Static_MonoBehaviourPublicObGaObAcCoObMeEmObExUnique_0;
            return ActionMenuDriver.field_Public_Static_MonoBehaviourPublicObGaObAc1ObAcBoCoObUnique_0;
        }

        public static (double x1, double y1, double x2, double y2) GetIntersection(float x, float y, float r)
        {
            var tmp = Math.Pow(y / x, 2);
            var c4 = -Math.Pow(r, 2) * -4;
            var x1 = Math.Sqrt(c4 + c4 * tmp) / (2 + 2 * tmp);
            var x2 = -x1;
            return (x1, x1 * (y / x), x2, x2 * (y / x));
        }

        private delegate void RefreshAMDelegate(ActionMenu actionMenu);
    }
}