﻿using System;
using System.Linq;
using System.Reflection;
using Il2CppInterop.Common.XrefScans;
using Il2CppInterop.Runtime.XrefScans;
using Il2CppVRC.SDKBase;

using VRCPlayer = Il2Cpp.MonoBehaviour1PublicOb_pOb_s_pBoGaOb_pStUnique;
using VRCFlowManager = Il2Cpp.MonoBehaviourPublicStObBoObLoInObLoAcGaUnique;
using VRCMotionState = Il2Cpp.MonoBehaviourPublicLaSiBoSiChBoObVeBoSiUnique;
using RoomManager = Il2Cpp.MonoBehaviourPublicBoApSiApBoObStApBo1Unique;
using UnityEngine;

namespace ActionMenuUtils
{
    internal static class Utils
    {
        //Gracefully taken from Advanced Invites https://github.com/Psychloor/AdvancedInvites/blob/master/AdvancedInvites/Utilities.cs#L356
        private static bool XRefScanFor(this MethodBase methodBase, string searchTerm)
        {
            return XrefScanner.XrefScan(methodBase).Any(
                xref => xref.Type == XrefType.Global && xref.ReadAsObject()?.ToString().IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static GoHomeDelegate GetGoHomeDelegate
        {
            get
            {
                if (goHomeDelegate != null) return goHomeDelegate;
                MethodInfo goHomeMethod = typeof(VRCFlowManager).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(
                    m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void) && m.XRefScanFor("Going to Home Location: "));

                goHomeDelegate = (GoHomeDelegate)Delegate.CreateDelegate(
                    typeof(GoHomeDelegate),
                    VRCFlowManager.prop_MonoBehaviourPublicStObBoObLoInObLoAcGaUnique_0,
                    goHomeMethod);
                return goHomeDelegate;
            }
        }

        private static void GoHome() => GetGoHomeDelegate();
        private static GoHomeDelegate goHomeDelegate;
        private delegate void GoHomeDelegate();

        private static RespawnDelegate GetRespawnDelegate
        {
            get
            {
                if (respawnDelegate != null) return respawnDelegate;
                MethodInfo respawnMethod = typeof(VRCPlayer).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Single(
                    m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void) && m.XRefScanFor("Respawned while not in a room."));

                respawnDelegate = (RespawnDelegate)Delegate.CreateDelegate(
                    typeof(RespawnDelegate),
                    VRCPlayer.field_Internal_Static_MonoBehaviour1PublicOb_pOb_s_pBoGaOb_pStUnique_0,
                    respawnMethod);
                return respawnDelegate;
            }
        }
        
        private static RespawnDelegate respawnDelegate;
        private delegate void RespawnDelegate();
        
        public static void Respawn()
        {
            GetRespawnDelegate();
            VRCPlayer.field_Internal_Static_MonoBehaviour1PublicOb_pOb_s_pBoGaOb_pStUnique_0.GetComponent<VRCMotionState>().Reset();
        }

        public static void RejoinInstance()
        {
            var instance = RoomManager.field_Private_Static_ApiWorldInstance_0;
            Networking.GoToRoom($"{instance.world.id}:{instance.instanceId}");
        }

        public static void Home() => GetGoHomeDelegate();
    }
}