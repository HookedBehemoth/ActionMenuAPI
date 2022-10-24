using System;
using ActionMenuApi.Helpers;
using ActionMenuApi.Managers;
using ActionMenuApi.Types;
using UnityEngine;

using ActionMenu = MonoBehaviourPublicGaTeGaCaObGaCaLiOb1Unique;
using PedalOption = MonoBehaviourPublicObSiObFuSi1ObBoSiAcUnique;

namespace ActionMenuApi.Pedals
{
    public abstract class PedalStruct
    {
        public string text { get; set; }
        public Texture2D icon { get; set; }
        public Action<ActionMenu> triggerEvent { get; protected set; }
        public PedalType Type { get; protected set; }
        public bool locked { get; set; }
        public bool shouldAdd { get; set; } = true;
    }

    public sealed class PedalButton : PedalStruct
    {
        public PedalButton(string text, Texture2D icon, Action triggerEvent, bool locked = false)
        {
            this.text = text;
            this.icon = icon;
            this.triggerEvent = delegate { triggerEvent(); };
            Type = PedalType.Button;
            this.locked = locked;
        }
    }

    public sealed class PedalFourAxis : PedalStruct
    {
        public PedalFourAxis(string text, Texture2D icon, Action<Vector2> onUpdate, string topButtonText,
            string rightButtonText, string downButtonText, string leftButtonText, bool locked = false)
        {
            this.text = text;
            this.icon = icon;
            triggerEvent = delegate
            {
                FourAxisPuppetManager.OpenFourAxisMenu(text, onUpdate, pedal);
                FourAxisPuppetManager.current.GetButtonUp().SetButtonText(topButtonText);
                FourAxisPuppetManager.current.GetButtonRight().SetButtonText(rightButtonText);
                FourAxisPuppetManager.current.GetButtonDown().SetButtonText(downButtonText);
                FourAxisPuppetManager.current.GetButtonLeft().SetButtonText(leftButtonText);
            };
            Type = PedalType.FourAxisPuppet;
            this.locked = locked;
        }

        public PedalOption pedal { get; set; }
    }

    public sealed class PedalRadial : PedalStruct
    {
        public float currentValue;

        public PedalRadial(string text, float startingValue, Texture2D icon, Action<float> onUpdate,
            bool locked = false, bool restricted = false)
        {
            this.text = text;
            currentValue = startingValue;
            this.icon = icon;
            triggerEvent = delegate
            {
                var combinedAction = (Action<float>) Delegate.Combine(new Action<float>(delegate(float f)
                {
                    startingValue = f;
                    pedal.SetButtonPercentText($"{Math.Round(startingValue * 100)}%");
                }), onUpdate);
                RadialPuppetManager.OpenRadialMenu(startingValue, combinedAction, text, pedal, restricted);
            };
            Type = PedalType.RadialPuppet;
            this.locked = locked;
            this.restricted = restricted;
        }

        public PedalOption pedal { get; set; }
        public bool restricted { get; }
    }

    public sealed class PedalSubMenu : PedalStruct
    {
        public PedalSubMenu(Action onSubMenuOpen, string text = null, Texture2D icon = null, Action onSubMenuClose = null, bool locked = false)
        {
            this.text = text;
            this.icon = icon;
            this.OnSubMenuOpen += onSubMenuOpen;
            this.OnSubMenuClose += onSubMenuClose;
            this.OnSubMenuClose += delegate
            {
                IsOpen = false;
            };
            triggerEvent = delegate(ActionMenu menu)
            {
                IsOpen = true;
                menu.PushPage(this._openFunc, this._closeFunc, icon, text);
            };
            Type = PedalType.SubMenu;
            this.locked = locked;
        }

        private Action _openFunc;
        /// <summary>
        /// Triggered when the submenu is opened *duh*
        /// </summary>
        public event Action OnSubMenuOpen
        {
            add
            {
                if (_openFunc is null)
                    _openFunc = value;
                else
                    _openFunc = (Action)Delegate.Combine(_openFunc, value);
            }
            remove
            {
                if (_openFunc is not null)
                    _openFunc = (Action)Delegate.Remove(_openFunc, value);
            }
        }

        private Action _closeFunc;
        
        /// <summary>
        /// Triggered when the sub menu is close *duh*
        /// </summary>
        public event Action OnSubMenuClose
        {
            add
            {
                if (_closeFunc is null)
                    _closeFunc = value;
                else
                    _closeFunc = (Action)Delegate.Combine(_closeFunc, value);
            }
            remove
            {
                if (_closeFunc is not null)
                    _closeFunc = (Action)Delegate.Remove(_closeFunc, value);
            }
        }

        public bool IsOpen { get; internal set; }
    }

    public sealed class PedalToggle : PedalStruct
    {
        public PedalToggle(string text, Action<bool> onToggle, bool toggled, Texture2D icon = null,
            bool locked = false)
        {
            this.text = text;
            this.toggled = toggled;
            this.icon = icon;
            triggerEvent = delegate
            {
                //MelonLogger.Msg($"Old state: {this.toggled}, New state: {!this.toggled}");
                this.toggled = !this.toggled;
                if (this.toggled)
                    pedal.SetPedalTypeIcon(Utilities.GetExpressionsIcons().typeToggleOn);
                else
                    pedal.SetPedalTypeIcon(Utilities.GetExpressionsIcons().typeToggleOff);
                onToggle.Invoke(toggled);
            };
            Type = PedalType.Toggle;
            this.locked = locked;
        }

        public bool toggled { get; set; }

        public PedalOption pedal { get; set; }
    }
}