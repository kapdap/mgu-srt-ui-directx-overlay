using GameOverlay.Drawing;
using GameOverlay.Windows;
using SRTPluginBase;
using SRTPluginProviderMGU;
using SRTPluginProviderMGU.Enumerations;
using SRTPluginProviderMGU.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SRTPluginUIMGUDirectXOverlay
{
    public class PluginUI : IPluginUI
    {
        internal static PluginInfo _info = new PluginInfo();
        public IPluginInfo Info => _info;
        public string RequiredProvider => "SRTPluginProviderMGU";

        private IPluginHostDelegates _hostDelegates;
        private IGameMemoryMGU _gameMemory;

        // DirectX Overlay-specific.
        public static OverlayWindow _window;
        public static Graphics _graphics;
        public static SharpDX.Direct2D1.WindowRenderTarget _device;
        public static IntPtr _gameWindowHandle;

        private Font _consolas32Bold; // IGT
        private Font _consolas14Bold; // HP
        private Font _consolas16Bold; // Default

        private SolidBrush _black;
        private SolidBrush _white;
        private SolidBrush _green;
        private SolidBrush _grey;
        private SolidBrush _darkergrey;
        private SolidBrush _darkred;
        private SolidBrush _red;
        private SolidBrush _lawngreen;
        private SolidBrush _gold;
        private SolidBrush _goldenrod;
        private SolidBrush _violet;

        private IReadOnlyDictionary<CharacterEnumeration, SharpDX.Mathematics.Interop.RawRectangleF> _characterToImageTranslation;
        private SharpDX.Direct2D1.Bitmap _characterSheet;

        private int CHR_SLOT_WIDTH;
        private int CHR_SLOT_HEIGHT;

        private int _opacity = 128;
        private double _scale = 1d;

        private bool _isOverlayReady;

        [STAThread]
        public int Startup(IPluginHostDelegates hostDelegates)
        {
            _hostDelegates = hostDelegates;

            CHR_SLOT_WIDTH = (int)Math.Round(38d * _scale, MidpointRounding.AwayFromZero); // Individual character portrait slot width.
            CHR_SLOT_HEIGHT = (int)Math.Round(38d * _scale, MidpointRounding.AwayFromZero); // Individual character portrait slot height.

            GenerateClipping();
            InitializeOverlay();

            return 0;
        }

        public int Shutdown()
        {
            _black?.Dispose();
            _white?.Dispose();
            _green?.Dispose();
            _grey?.Dispose();
            _darkergrey?.Dispose();
            _darkred?.Dispose();
            _red?.Dispose();
            _lawngreen?.Dispose();
            _gold?.Dispose();
            _goldenrod?.Dispose();
            _violet?.Dispose();

            _consolas14Bold?.Dispose();
            _consolas16Bold?.Dispose();
            _consolas32Bold?.Dispose();

            _characterSheet?.Dispose();
            _characterToImageTranslation = null;

            _device = null; // We didn't create this object so we probably shouldn't be the one to dispose of it. Just set the variable to null so the reference isn't held.
            _graphics?.Dispose(); // This should technically be the one to dispose of the _device object since it was pulled from this instance.
            _graphics = null;
            _window?.Dispose();
            _window = null;

            _isOverlayReady = false;

            return 0;
        }

        public int ReceiveData(object gameMemory)
        {
            try
            {
                _gameMemory = (IGameMemoryMGU)gameMemory;

                if (_isOverlayReady)
                {
                    UpdateOverlay();
                    RenderOverlay();
                }
                else
                    CreateOverlay();
            }
            finally
            {
                if (_graphics != null && _graphics.IsInitialized)
                    _graphics.EndScene();
            }

            return 0;
        }

        private void InitializeOverlay()
        {
            DEVMODE devMode = default;
            devMode.dmSize = (short)Marshal.SizeOf<DEVMODE>();
            PInvoke.EnumDisplaySettings(null, -1, ref devMode);

            _window = new OverlayWindow(0, 0, devMode.dmPelsWidth, devMode.dmPelsHeight)
            {
                IsTopmost = true,
                IsVisible = true
            };

            _graphics = new Graphics()
            {
                MeasureFPS = false,
                PerPrimitiveAntiAliasing = false,
                TextAntiAliasing = true,
                UseMultiThreadedFactories = false,
                VSync = false
            };
        }

        private bool CreateOverlay()
        {
            if (!_isOverlayReady && _gameMemory.Process.WindowHandle != 0)
            {
                _gameWindowHandle = new IntPtr(_gameMemory.Process.WindowHandle);

                _window?.Create();
                _window?.FitTo(_gameWindowHandle, true);

                _graphics.Width = _window.Width;
                _graphics.Height = _window.Height;
                _graphics.WindowHandle = _window.Handle;
                _graphics.Setup();

                _window.SizeChanged += (object sender, OverlaySizeEventArgs e) =>
                    _graphics.Resize(_window.Width, _window.Height);

                _window.VisibilityChanged += (object sender, OverlayVisibilityEventArgs e) =>
                    WindowHelper.EnableBlurBehind(_gameWindowHandle);

                // Get a refernence to the underlying RenderTarget from SharpDX. This'll be used to draw portions of images.
                _device = (SharpDX.Direct2D1.WindowRenderTarget)typeof(Graphics).GetField("_device", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_graphics);

                _consolas14Bold = _graphics?.CreateFont("Consolas", 14, true);
                _consolas16Bold = _graphics?.CreateFont("Consolas", 16, true);
                _consolas32Bold = _graphics?.CreateFont("Consolas", 32, true);

                _black = _graphics?.CreateSolidBrush(0, 0, 0, _opacity);
                _white = _graphics?.CreateSolidBrush(255, 255, 255, _opacity);
                _green = _graphics?.CreateSolidBrush(0, 128, 0, _opacity);
                _grey = _graphics?.CreateSolidBrush(128, 128, 128, _opacity);
                _darkergrey = _graphics?.CreateSolidBrush(60, 60, 60, _opacity);
                _darkred = _graphics?.CreateSolidBrush(139, 0, 0, _opacity);
                _red = _graphics?.CreateSolidBrush(255, 0, 0, _opacity);
                _lawngreen = _graphics?.CreateSolidBrush(124, 252, 0, _opacity);
                _gold = _graphics?.CreateSolidBrush(255, 215, 0, _opacity);
                _goldenrod = _graphics?.CreateSolidBrush(218, 165, 32, _opacity);
                _violet = _graphics?.CreateSolidBrush(238, 130, 238, _opacity);

                _characterSheet = ImageLoader.LoadBitmap(_device, Properties.Resources.portraits);

                _isOverlayReady = true;
            }

            return _isOverlayReady;
        }

        private void UpdateOverlay()
        {
            if (_window != null && _window.IsInitialized)
                _window.PlaceAbove(_gameWindowHandle);

            if (_graphics != null && _graphics.IsInitialized)
            {
                _graphics.BeginScene();
                _graphics.ClearScene();
            }
        }

        private void RenderOverlay()
        {
            Point textSize;

            int alignX = _graphics.Width - 216;
            int alignY = 0;

            int baseX = alignX - 10;
            int baseY = alignY + 10;

            int YOffset = baseY;
            int YHeight = 29;
            int YSpace = 9;

            for (int i = 0; i < _gameMemory.Characters.Length; ++i)
            {
                CharacterEntry character = _gameMemory.Characters[i];

                SolidBrush healthBrush;

                if (!character.IsAlive)
                    healthBrush = _red;
                else if (character.IsPoison)
                    healthBrush = _violet;
                else if (character.IsCaution)
                    healthBrush = _gold;
                else if (character.IsDanger)
                    healthBrush = _red;
                else
                    healthBrush = _green;

                int imageX = baseX;
                int imageY = YOffset += i > 0 ? CHR_SLOT_HEIGHT : 0;

                int textX = imageX + CHR_SLOT_WIDTH + 2;
                int textY = imageY + 1;

                SharpDX.Mathematics.Interop.RawRectangleF drawRegion = new SharpDX.Mathematics.Interop.RawRectangleF(imageX, imageY, CHR_SLOT_WIDTH, CHR_SLOT_HEIGHT);
                SharpDX.Mathematics.Interop.RawRectangleF imageRegion;

                if (_characterToImageTranslation.ContainsKey(character.Character))
                    imageRegion = _characterToImageTranslation[character.Character];
                else
                    imageRegion = new SharpDX.Mathematics.Interop.RawRectangleF(0, 0, CHR_SLOT_WIDTH, CHR_SLOT_HEIGHT);

                imageRegion.Right += imageRegion.Left;
                imageRegion.Bottom += imageRegion.Top;

                drawRegion.Right += drawRegion.Left;
                drawRegion.Bottom += drawRegion.Top;

                if (_characterToImageTranslation.ContainsKey(character.Character))
                    _device?.DrawBitmap(_characterSheet, drawRegion, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear, imageRegion);

                DrawProgressBar(_darkergrey, healthBrush, textX, textY, 172, 36, character.CurrentHP, character.MaximumHP);
                _graphics?.DrawText(_consolas14Bold, _white, textX + 5, textY + 10, character.HealthMessage);
            }

            int timerX = baseX + 3;
            int timerY = YOffset += CHR_SLOT_HEIGHT + YSpace;

            _graphics?.DrawText(_consolas32Bold, _white, timerX, timerY, _gameMemory.IGT.FormattedString);
            textSize = _graphics.MeasureString(_consolas32Bold, _gameMemory.IGT.FormattedString);

            int headerX = baseX + 3;
            int headerY = YOffset += (int)textSize.Y + YSpace;

            _graphics?.DrawText(_consolas16Bold, _red, headerX, headerY, "Enemy HP");
            textSize = _graphics.MeasureString(_consolas16Bold, "Enemy HP");

            YOffset += (int)textSize.Y - YHeight + 6;

            for (int i = 0; i < _gameMemory.Enemy.Length; ++i)
            {
                EnemyEntry enemy = _gameMemory.Enemy[i];

                if (!enemy.IsAlive) continue;

                int healthX = baseX - 2;
                int healthY = YOffset += i > 0 ? YHeight : 0;

                DrawProgressBar(_darkergrey, _darkred, healthX, healthY, 216, YHeight, enemy.CurrentHP, enemy.MaximumHP);
                _graphics?.DrawText(_consolas14Bold, _red, healthX + 5, healthY + 5, enemy.HealthMessage);
            }
        }

        private void DrawProgressBar(SolidBrush backBrush, SolidBrush foreBrush, float x, float y, float width, float height, float value, float maximum = 100)
        {
            // Draw FG.
            Rectangle foreRect = new Rectangle(
                x,
                y,
                x + (width * value / maximum),
                y + height
            );

            // Draw BG.
            Rectangle backRect = new Rectangle(
                x + foreRect.Width,
                y,
                x + width,
                y + height
            );

            _graphics?.FillRectangle(backBrush, backRect);
            _graphics?.FillRectangle(foreBrush, foreRect);
        }

        public void GenerateClipping()
        {
            if (_characterToImageTranslation == null)
                _characterToImageTranslation = new Dictionary<CharacterEnumeration, SharpDX.Mathematics.Interop.RawRectangleF>()
                {
                    { CharacterEnumeration.Martin, new SharpDX.Mathematics.Interop.RawRectangleF(0, CHR_SLOT_HEIGHT * 0, CHR_SLOT_WIDTH, CHR_SLOT_HEIGHT) },
                    { CharacterEnumeration.Uji,    new SharpDX.Mathematics.Interop.RawRectangleF(0, CHR_SLOT_HEIGHT * 1, CHR_SLOT_WIDTH, CHR_SLOT_HEIGHT) },
                    { CharacterEnumeration.Diane,  new SharpDX.Mathematics.Interop.RawRectangleF(0, CHR_SLOT_HEIGHT * 2, CHR_SLOT_WIDTH, CHR_SLOT_HEIGHT) }
                };
        }
    }
}
