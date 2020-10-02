using SRTPluginProviderMGU.Models;
using System;

namespace SRTPluginUIMGUDirectXOverlay
{
    public class PluginConfig : BaseNotifyModel
    {
        private byte _opacity = 128;
        public byte Opacity
        {
            get
            {
                SetField(ref _opacity, GetRange(_opacity, 1, 255));
                return _opacity;
            }
            set => SetField(ref _opacity, GetRange(value, 1, 255));
        }

        private float _scalingFactor = 1f;
        public float ScalingFactor
        {
            get => _scalingFactor;
            set => SetField(ref _scalingFactor, value);
        }

        private byte GetRange(byte value, int min, int max)
        {
            value = (byte)Math.Max(value, min);
            return (byte)Math.Min(value, max);
        }
    }
}