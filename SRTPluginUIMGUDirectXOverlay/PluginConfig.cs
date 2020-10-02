using SRTPluginProviderMGU.Models;

namespace SRTPluginUIMGUDirectXOverlay
{
    public class PluginConfig : BaseNotifyModel
    {
        internal int _opacity;
        public int Opacity
        {
            get => _opacity;
            set => SetField(ref _opacity, value);
        }
        internal float _scalingFactor;
        public float ScalingFactor
        {
            get => _scalingFactor;
            set => SetField(ref _scalingFactor, value);
        }

        public PluginConfig()
        {
            Opacity = 128;
            ScalingFactor = 1f;
        }
    }
}
