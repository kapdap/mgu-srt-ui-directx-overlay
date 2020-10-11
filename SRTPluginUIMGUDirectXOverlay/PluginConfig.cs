using SRTPluginProviderMGU.Models;
using System;

namespace SRTPluginUIMGUDirectXOverlay
{
    public class PluginConfig : BaseNotifyModel
    {
        private byte _opacity = 128;
        public byte Opacity
        {
            get => _opacity;
            set => SetField(ref _opacity, GetRange(value, 1, 255));
        }

        private float _scalingFactor = 1f;
        public float ScalingFactor
        {
            get => _scalingFactor;
            set => SetField(ref _scalingFactor, GetRange(value, 0.1f, 2f));
        }

        private bool _showTimer = true;
        public bool ShowTimer
        {
            get => _showTimer;
            set => SetField(ref _showTimer, value);
        }

        private bool _showInventory = true;
        public bool ShowInventory
        {
            get => _showInventory;
            set => SetField(ref _showInventory, value);
        }

        private bool _showEquipment = false;
        public bool ShowEquipment
        {
            get => _showEquipment;
            set => SetField(ref _showEquipment, value);
        }

        private bool _showEnemy = true;
        public bool ShowEnemy
        {
            get => _showEnemy;
            set => SetField(ref _showEnemy, value);
        }

        private bool _debug = false;
        public bool Debug
        {
            get => _debug;
            set => SetField(ref _debug, value);
        }

        private bool _debugCharacters = false;
        public bool DebugCharacters
        {
            get => _debugCharacters;
            set => SetField(ref _debugCharacters, value);
        }

        private bool _debugEnemy = false;
        public bool DebugEnemy
        {
            get => _debugEnemy;
            set => SetField(ref _debugEnemy, value);
        }

        private byte GetRange(byte value, int min, int max)
        {
            value = (byte)Math.Max(value, min);
            return (byte)Math.Min(value, max);
        }

        private float GetRange(float value, float min, float max)
        {
            value = Math.Max(value, min);
            return Math.Min(value, max);
        }
    }
}