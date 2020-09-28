using SRTPluginBase;
using System;

namespace SRTPluginUIMGUDirectXOverlay
{
    internal class PluginInfo : IPluginInfo
    {
        public string Name => "DirectX Overlay UI (Martian Gothic: Unification)";

        public string Description => "A DirectX-based Overlay User Interface for displaying Martian Gothic: Unification game memory values.";

        public string Author => "Kapdap";

        public Uri MoreInfoURL => new Uri("https://github.com/Kapdap/mgu-srt-ui-directx-overlay");

        public int VersionMajor => assemblyFileVersion.ProductMajorPart;

        public int VersionMinor => assemblyFileVersion.ProductMinorPart;

        public int VersionBuild => assemblyFileVersion.ProductBuildPart;

        public int VersionRevision => assemblyFileVersion.ProductPrivatePart;

        private System.Diagnostics.FileVersionInfo assemblyFileVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }
}
