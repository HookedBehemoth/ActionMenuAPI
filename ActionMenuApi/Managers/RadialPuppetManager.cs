using System;
using ActionMenuApi.Helpers;
using MelonLoader;
using UnityEngine;

using ActionMenu = Il2Cpp.MonoBehaviourPublicGaTeGaCaObGaCaLiOb1Unique;
using ActionMenuType = Il2Cpp.MonoBehaviourPublicCaObAc1BoSiBoObObObUnique.EnumNPublicSealedvaLeRi3vUnique;
using RadialPuppetMenu = Il2Cpp.MonoBehaviour2PublicObGaObTeGaFu2GaSiStUnique;
using PedalOption = Il2Cpp.MonoBehaviourPublicObSiObFuSi1ObBoSiAcUnique;

namespace ActionMenuApi.Managers;

internal static class RadialPuppetManager
{
    private static RadialPuppetImpl _leftImpl;
    private static RadialPuppetImpl _rightImpl;

    public static void Setup()
    {
        var driver = Utilities.GetDriver();
        _leftImpl = new RadialPuppetImpl(driver.GetLeftOpener().GetActionMenu(), InputManager.Left);
        _rightImpl = new RadialPuppetImpl(driver.GetRightOpener().GetActionMenu(), InputManager.Right);
    }

    public static void OpenRadialMenu(ActionMenuType hand, float startingValue, Action<float> onUpdate, Action onClose, string title, PedalOption pedalOption, bool restricted = false)
    {
        var impl = hand switch
        {
            ActionMenuType.Left => _leftImpl,
            ActionMenuType.Right => _rightImpl,
            _ => throw new ArgumentOutOfRangeException(""),
        };

        impl.OpenRadialMenu(startingValue, onUpdate, onClose, title, pedalOption, restricted);
    }

    public static void OnUpdate()
    {
        _leftImpl?.OnUpdate();
        _rightImpl?.OnUpdate();
    }

    public static void CloseRadialMenu()
    {
        _leftImpl?.CloseRadialMenu();
        _rightImpl?.CloseRadialMenu();
    }

    private class RadialPuppetImpl
    {
        private readonly ActionMenu _actionMenu;
        private readonly RadialPuppetMenu _current;
        private readonly InputManager _input;

        private bool restricted;
        private float currentValue;

        private Action<float> UpdateAction { get; set; }
        private Action CloseAction { get; set; }

        public RadialPuppetImpl(ActionMenu actionMenu, InputManager input)
        {
            _actionMenu = actionMenu;
            _current = Utilities.CloneChild(_actionMenu.transform, "RadialPuppetMenu").GetComponent<RadialPuppetMenu>();
            _input = input;

            // Note: Might want to look into how these are supposed to be used
            _current.transform.Find("Container/Limits").gameObject.SetActive(false);
            _current.transform.Find("Container/Marker").gameObject.SetActive(false);
        }

        public void OnUpdate()
        {
            //Probably a better more efficient way to do all this
            if (!_current.isActiveAndEnabled)
            {
                return;
            }

            if (_input.GetClicked())
            {
                CloseRadialMenu();
                return;
            }

            UpdateMathStuff();
            CallUpdateAction();
        }

        public void OpenRadialMenu(float startingValue, Action<float> onUpdate, Action onClose, string title, PedalOption pedalOption, bool restricted = false)
        {
            if (_current.isActiveAndEnabled) return;

            this.restricted = restricted;
            Input.ResetInputAxes();
            InputManager.ResetMousePos();
            _current.gameObject.SetActive(true);
            _current.GetFill().SetFillAngle(startingValue * 360); //Please dont break
            UpdateAction = onUpdate;
            CloseAction = onClose;
            currentValue = startingValue;

            _current.GetTitle().text = title;
            _current.GetCenterText().text = $"{Mathf.Round(startingValue * 100f)}%";
            _current.GetFill().UpdateGeometry();
            _current.transform.localPosition = pedalOption.GetActionButton().transform.localPosition; //new Vector3(-256f, 0, 0); 
            var angleOriginal = Utilities.ConvertFromEuler(startingValue * 360);
            var eulerAngle = Utilities.ConvertFromDegToEuler(angleOriginal);
            _actionMenu.DisableInput();
            _actionMenu.SetMainMenuOpacity(0.5f);
            _current.UpdateArrow(angleOriginal, eulerAngle);
        }

        public void CloseRadialMenu()
        {
            if (!_current.isActiveAndEnabled) return;
            CallUpdateAction();
            CallCloseAction();
            _current.gameObject.SetActive(false);
            _actionMenu.EnableInput();
            _actionMenu.SetMainMenuOpacity();
        }

        private void CallUpdateAction()
        {
            try
            {
                UpdateAction?.Invoke(_current.GetFill().GetFillAngle() / 360f);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Exception caught in onUpdate action passed to Radial Puppet", e);
            }
        }

        private void CallCloseAction()
        {
            try
            {
                CloseAction?.Invoke();
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Exception caught in onClose action passed to Radial Puppet", e);
            }
        }

        private void UpdateMathStuff()
        {
            var mousePos = _input.GetStick();
            _current.GetCursor().transform.localPosition = mousePos * 4;

            if (Vector2.Distance(mousePos, Vector2.zero) > 12)
            {
                var angleOriginal = Mathf.Round(Mathf.Atan2(mousePos.y, mousePos.x) * Constants.RAD_TO_DEG);
                var eulerAngle = Utilities.ConvertFromDegToEuler(angleOriginal);
                var normalisedAngle = eulerAngle / 360f;
                if (Math.Abs(normalisedAngle - currentValue) < 0.0001f) return;
                if (!restricted)
                {
                    _current.SetAngle(eulerAngle);
                    _current.UpdateArrow(angleOriginal, eulerAngle);
                }
                else
                {
                    var (euler, original, normalized) = (
                            currentValue > normalisedAngle,
                            currentValue - normalisedAngle < 0.5f,
                            normalisedAngle - currentValue < 0.5f) switch
                    {
                        (true, true, _) or (false, _, true) => (eulerAngle, angleOriginal, normalisedAngle),
                        (true, false, _) => (360, 90, 1),
                        (false, _, false) => (0, 90, 0),
                    };
                    _current.SetAngle(euler);
                    _current.UpdateArrow(original, euler);
                    currentValue = normalized;
                }
            }
        }
    }

}
