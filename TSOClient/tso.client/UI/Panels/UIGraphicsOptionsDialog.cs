﻿using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Client.UI.Screens;
using FSO.Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Utils;
using FSO.LotView;

namespace FSO.Client.UI.Panels
{
    public class UIGraphicsOptionsDialog : UIDialog
    {

        public UIButton AntiAliasCheckButton { get; set; }
        public UIButton ShadowsCheckButton { get; set; }
        public UIButton LightingCheckButton { get; set; }
        public UIButton UIEffectsCheckButton { get; set; }
        public UIButton EdgeScrollingCheckButton { get; set; }
        public UIButton Wall3DButton { get; set; }

        // High-Medium-Low detail buttons:

        public UIButton TerrainDetailLowButton { get; set; }
        public UIButton TerrainDetailMedButton { get; set; }
        public UIButton TerrainDetailHighButton { get; set; }

        public UIButton CharacterDetailLowButton { get; set; }
        public UIButton CharacterDetailMedButton { get; set; }
        public UIButton CharacterDetailHighButton { get; set; }

        public UIButton AALowButton { get; set; }
        public UIButton AAMedButton { get; set; }
        public UIButton AAHighButton { get; set; }

        public UIButton SwitchModeButton { get; set; }

        public UILabel UIEffectsLabel { get; set; }
        public UILabel AntiAliasLabel { get; set; }
        public UILabel CharacterDetailLabel { get; set; }
        public UILabel ShadowsLabel { get; set; }
        public UILabel LightingLabel { get; set; }
        public UILabel EdgeScrollingLabel { get; set; }

        public UILabel LowLabel { get; set; }
        public UILabel MediumLabel { get; set; }
        public UILabel HighLabel { get; set; }

        public UILabel TerrainDetailLabel { get; set; }
        public UILabel Wall3DLabel { get; set; }
        
        public UIButton DirectionButton { get; set; }
        public UILabel DirectionLabel { get; set; }

        public UIButton CompressionButton { get; set; }
        public UILabel CompressionLabel { get; set; }

        public UIButton AdvancedButton { get; set; }
        public UILabel AdvancedLabel { get; set; }

        public UIButton DPIButton { get; set; }
        public UISlider LightingSlider;
        private bool InternalChange;

        public UIGraphicsOptionsDialog() : base(UIDialogStyle.OK, true)
        {
            SetSize(460, 300);
            var script = this.RenderScript("graphicspanel.uis");

            UIEffectsLabel.Caption = GameFacade.Strings.GetString("f103", "2");
            UIEffectsLabel.Alignment = TextAlignment.Middle;
            CharacterDetailLabel.Caption = GameFacade.Strings.GetString("f103", "4");
            TerrainDetailLabel.Caption = GameFacade.Strings.GetString("f103", "1");
            ShadowsLabel.Caption = GameFacade.Strings.GetString("f103", "6");
            LightingLabel.Caption = GameFacade.Strings.GetString("f103", "20");

            ShadowsCheckButton.Tooltip = ShadowsLabel.Caption;
            LightingCheckButton.Tooltip = LightingLabel.Caption;
            UIEffectsCheckButton.Tooltip = UIEffectsLabel.Caption;

            CharacterDetailLowButton.OnButtonClick += new ButtonClickDelegate(ChangeShadowDetail);
            CharacterDetailMedButton.OnButtonClick += new ButtonClickDelegate(ChangeShadowDetail);
            CharacterDetailHighButton.OnButtonClick += new ButtonClickDelegate(ChangeShadowDetail);

            TerrainDetailLowButton.OnButtonClick += new ButtonClickDelegate(ChangeSurroundingDetail);
            TerrainDetailMedButton.OnButtonClick += new ButtonClickDelegate(ChangeSurroundingDetail);
            TerrainDetailHighButton.OnButtonClick += new ButtonClickDelegate(ChangeSurroundingDetail);

            TerrainDetailLowButton.Tooltip = GameFacade.Strings.GetString("f103", "8");
            TerrainDetailMedButton.Tooltip = GameFacade.Strings.GetString("f103", "9");
            TerrainDetailHighButton.Tooltip = GameFacade.Strings.GetString("f103", "10");

            var moveItems = new UIElement[] {
                TerrainDetailLowButton,TerrainDetailMedButton,TerrainDetailHighButton,
                CharacterDetailLowButton,CharacterDetailMedButton,CharacterDetailHighButton,
                CharacterDetailLabel, TerrainDetailLabel,
                HighLabel, MediumLabel, LowLabel,
            };
            foreach (var item in moveItems) item.Position += new Vector2(57, 27);

            var aa = CloneDetail(new Vector2(0, 23*2));
            AALowButton = aa.Item1;
            AAMedButton = aa.Item2;
            AAHighButton = aa.Item3;

            AALowButton.OnButtonClick += new ButtonClickDelegate(ChangeAA);
            AAMedButton.OnButtonClick += new ButtonClickDelegate(ChangeAA);
            AAHighButton.OnButtonClick += new ButtonClickDelegate(ChangeAA);
            aa.Item4.Caption = AntiAliasLabel.Caption;

            var clone = CloneCheckbox();
            Wall3DButton = clone.Item1; Wall3DLabel = clone.Item2;
            Wall3DLabel.Caption = GameFacade.Strings.GetString("f103", "12");

            clone = CloneCheckbox();
            DirectionButton = clone.Item1; DirectionLabel = clone.Item2;
            //DirectionButton.Visible = FSOEnvironment.Enable3D;
            //DirectionLabel.Visible = FSOEnvironment.Enable3D;
            DirectionLabel.Caption = GameFacade.Strings.GetString("f103", "18");

            clone = CloneCheckbox();
            AdvancedButton = clone.Item1; AdvancedLabel = clone.Item2;
            AdvancedLabel.Caption = GameFacade.Strings.GetString("f103", "26");
            AdvancedLabel.Tooltip = GameFacade.Strings.GetString("f103", "27");
            AdvancedButton.Tooltip = AdvancedLabel.Tooltip;

            clone = CloneCheckbox();
            CompressionButton = clone.Item1; CompressionLabel = clone.Item2;
            CompressionLabel.Caption = GameFacade.Strings.GetString("f103", "23");
            CompressionLabel.Tooltip = GameFacade.Strings.GetString("f103", "24");
            CompressionButton.Tooltip = CompressionLabel.Tooltip;
            CompressionButton.Disabled = !FSOEnvironment.TexCompressSupport;

            AntiAliasCheckButton.Disabled = !FSOEnvironment.MSAASupport;

            AntiAliasCheckButton.Visible = false;
            AntiAliasLabel.Visible = false;
            var toggles = new Dictionary<UIButton, UILabel>()
            {
                { ShadowsCheckButton, ShadowsLabel },
                { LightingCheckButton, LightingLabel },
                { UIEffectsCheckButton, UIEffectsLabel },
                { EdgeScrollingCheckButton, EdgeScrollingLabel },
                { CompressionButton, CompressionLabel },
                { Wall3DButton, Wall3DLabel },
                { DirectionButton, DirectionLabel },
                { AdvancedButton, AdvancedLabel },
            };

            int i = 0;
            foreach (var item in toggles)
            {
                item.Key.Position = new Vector2(23, 65 + 22 * (i++));
                item.Value.Alignment = TextAlignment.Left;
                item.Value.Position = item.Key.Position + new Vector2(24, 0);
                item.Key.OnButtonClick += new ButtonClickDelegate(FlipSetting);
            }

            //switch lighting and uieffects label. replace lighting check with a slider

            LightingSlider = new UISlider();
            LightingSlider.Orientation = 0;
            LightingSlider.Texture = GetTexture(0x42500000001);
            LightingSlider.MinValue = 0f;
            LightingSlider.MaxValue = 3f;
            LightingSlider.AllowDecimals = false;
            LightingSlider.Position = new Vector2(184, 167+10);
            LightingSlider.SetSize(240f, 0f);
            Add(LightingSlider);
            //LightingLabel.X -= 24;

            DPIButton = new UIButton();
            DPIButton.Size = new Vector2(150, 35);
            DPIButton.Caption = GameFacade.Strings.GetString("f103", "13");
            DPIButton.Position = new Vector2(40, 250);
            DPIButton.OnButtonClick += DPIButton_OnButtonClick;
            Add(DPIButton);

            SwitchModeButton = new UIButton();
            SwitchModeButton.Size = new Vector2(175, 35);
            SwitchModeButton.Caption = GameFacade.Strings.GetString("f103", "41");
            SwitchModeButton.Tooltip = GameFacade.Strings.GetString("f103", "40");
            SwitchModeButton.Position = new Vector2(210, 250);
            SwitchModeButton.OnButtonClick += SwitchModeButton_OnButtonClick;
            Add(SwitchModeButton);

            var style = TextStyle.DefaultTitle.Clone();
            style.Size = 12;

            var toggle = new UILabel();
            toggle.CaptionStyle = style;
            toggle.Caption = GameFacade.Strings.GetString("f103", "19");
            toggle.Position = new Vector2(23, 35);
            Add(toggle);

            var detail = new UILabel();
            detail.CaptionStyle = style;
            detail.Caption = GameFacade.Strings.GetString("f103", "22");
            detail.Position = new Vector2(180, 35);
            Add(detail);

            var adv = new UILabel();
            adv.CaptionStyle = style;
            adv.Caption = GameFacade.Strings.GetString("f103", "16");
            adv.Position = new Vector2(180, 117+10);
            Add(adv);

            var types = new UILabel();
            types.Caption = GameFacade.Strings.GetString("f103", "17");
            types.Position = new Vector2(180, 145+10);
            types.Size = new Vector2(240, 0);
            types.Alignment = TextAlignment.Center;
            Add(types);

            Caption = GameFacade.Strings.GetString("f103", "21");

            SettingsChanged();

            LightingSlider.OnChange += (elem) =>
            {
                if (InternalChange) return;
                var settings = GlobalSettings.Default;
                settings.LightingMode = (int)LightingSlider.Value;
                GlobalSettings.Default.Save();
                SettingsChanged();
            };

            OKButton.OnButtonClick += (btn) =>
            {
                UIScreen.RemoveDialog(this);
            };

            GraphicsModeControl.ModeChanged += UpdateModeText;
        }

        private void SwitchModeButton_OnButtonClick(UIElement button)
        {
            if (GraphicsModeControl.Mode == LotView.Model.GlobalGraphicsMode.Hybrid2D)
            {
                GraphicsModeControl.ChangeMode(LotView.Model.GlobalGraphicsMode.Full3D);
            }
            else if (GraphicsModeControl.Mode == LotView.Model.GlobalGraphicsMode.Full3D)
            {
                GraphicsModeControl.ChangeMode(LotView.Model.GlobalGraphicsMode.Hybrid2D);
            }
        }

        private void DPIButton_OnButtonClick(UIElement button)
        {
            UIScreen.GlobalShowDialog(new UIDPIScaleDialog(), true);
        }

        public Tuple<UIButton, UIButton, UIButton, UILabel> CloneDetail(Vector2 posOffset)
        {
            var check = new UIButton(TerrainDetailLowButton.Texture) { Position = TerrainDetailLowButton.Position + posOffset };
            Add(check);
            var check2 = new UIButton(TerrainDetailLowButton.Texture) { Position = TerrainDetailMedButton.Position + posOffset };
            Add(check2);
            var check3 = new UIButton(TerrainDetailLowButton.Texture) { Position = TerrainDetailHighButton.Position + posOffset };
            Add(check3);
            var label = new UILabel();
            label.CaptionStyle = TerrainDetailLabel.CaptionStyle;
            label.Position = TerrainDetailLabel.Position + posOffset;
            label.Size = TerrainDetailLabel.Size;
            label.Alignment = TerrainDetailLabel.Alignment;
            Add(label);
            return new Tuple<UIButton, UIButton, UIButton, UILabel>(check, check2, check3, label);
        }

        public Tuple<UIButton, UILabel> CloneCheckbox()
        {
            var check = new UIButton(AntiAliasCheckButton.Texture);
            Add(check);
            var label = new UILabel();
            label.CaptionStyle = UIEffectsLabel.CaptionStyle;
            label.Position = check.Position + new Microsoft.Xna.Framework.Vector2(34, 0);
            Add(label);
            return new Tuple<UIButton, UILabel>(check, label);
        }

        private void ShowRestartWarning()
        {
            UIAlert alert = null;
            alert = UIScreen.GlobalShowAlert(new UIAlertOptions()
            {
                Message = GameFacade.Strings.GetString("f103", "25"),
                Buttons = UIAlertButton.Ok(x => {
                    UIScreen.RemoveDialog(alert);
                })
            }, true);
        }

        private void FlipSetting(UIElement button)
        {
            var settings = GlobalSettings.Default;
            if (button == AntiAliasCheckButton) settings.AntiAlias = settings.AntiAlias ^ 1;
            else if (button == ShadowsCheckButton) settings.EnableTransitions = !(settings.EnableTransitions);
            else if (button == LightingCheckButton) settings.Weather = !(settings.Weather);
            else if (button == UIEffectsCheckButton) settings.CityShadows = !(settings.CityShadows);
            else if (button == EdgeScrollingCheckButton) settings.EdgeScroll = !(settings.EdgeScroll);
            else if (button == DirectionButton) settings.DirectionalLight3D = !(settings.DirectionalLight3D);
            else if (button == AdvancedButton) settings.ComplexShaders = !(settings.ComplexShaders);
            else if (button == CompressionButton)
            {
                settings.TexCompression = (((settings.TexCompression) & 1) ^ 1) | 2;
                ShowRestartWarning();
            }
            else if (button == Wall3DButton)
            {
                settings.CitySkybox = !settings.CitySkybox;
            }
            GlobalSettings.Default.Save();
            SettingsChanged();
        }

        private void ChangeShadowDetail(UIElement button)
        {
            var settings = GlobalSettings.Default;
            if (button == CharacterDetailLowButton) settings.ShadowQuality = 512;
            else if (button == CharacterDetailMedButton) settings.ShadowQuality = 1024;
            else if (button == CharacterDetailHighButton) settings.ShadowQuality = 2048;
            GlobalSettings.Default.Save();
            SettingsChanged();
        }

        private void ChangeSurroundingDetail(UIElement button)
        {
            var settings = GlobalSettings.Default;
            if (button == TerrainDetailLowButton) settings.SurroundingLotMode = 0;
            else if (button == TerrainDetailMedButton) settings.SurroundingLotMode = 1;
            else if (button == TerrainDetailHighButton) settings.SurroundingLotMode = 2;
            GlobalSettings.Default.Save();
            SettingsChanged();
        }

        private void ChangeAA(UIElement button)
        {
            var settings = GlobalSettings.Default;
            if (button == AALowButton) settings.AntiAlias = 0;
            else if (button == AAMedButton) settings.AntiAlias = 1;
            else if (button == AAHighButton) settings.AntiAlias = 2;
            GlobalSettings.Default.Save();
            SettingsChanged();
        }

        private void UpdateModeText(LotView.Model.GlobalGraphicsMode mode)
        {
            switch (mode)
            {
                case LotView.Model.GlobalGraphicsMode.Full2D:
                    SwitchModeButton.Visible = false;
                    break;
                case LotView.Model.GlobalGraphicsMode.Full3D:
                    SwitchModeButton.Visible = true;
                    SwitchModeButton.Caption = GameFacade.Strings.GetString("f103", "42");
                    break;
                case LotView.Model.GlobalGraphicsMode.Hybrid2D:
                    SwitchModeButton.Visible = true;
                    SwitchModeButton.Caption = GameFacade.Strings.GetString("f103", "41");
                    break;
            }
            Invalidate();
        }

        private void SettingsChanged()
        {
            var settings = GlobalSettings.Default;
            AntiAliasCheckButton.Selected = settings.AntiAlias > 0; //antialias for render targets
            ShadowsCheckButton.Selected = settings.EnableTransitions;
            LightingCheckButton.Selected = settings.Weather;
            UIEffectsCheckButton.Selected = settings.CityShadows; //instead of being able to disable UI transparency, you can toggle City Shadows.
            EdgeScrollingCheckButton.Selected = settings.EdgeScroll;
            DirectionButton.Selected = settings.DirectionalLight3D;
            AdvancedButton.Selected = settings.ComplexShaders;

            // Character detail changed for city shadow detail.
            CharacterDetailLowButton.Selected = (settings.ShadowQuality <= 512);
            CharacterDetailMedButton.Selected = (settings.ShadowQuality > 512 && settings.ShadowQuality <= 1024);
            CharacterDetailHighButton.Selected = (settings.ShadowQuality > 1024);

            //not used right now! We need to determine if this should be ingame or not... It affects the density of grass blades on the simulation terrain.
            TerrainDetailLowButton.Selected = (settings.SurroundingLotMode == 0);
            TerrainDetailMedButton.Selected = (settings.SurroundingLotMode == 1);
            TerrainDetailHighButton.Selected = (settings.SurroundingLotMode == 2);

            AALowButton.Selected = (settings.AntiAlias == 0);
            AAMedButton.Selected = (settings.AntiAlias == 1);
            AAHighButton.Selected = (settings.AntiAlias == 2);

            InternalChange = true;
            LightingSlider.Value = settings.LightingMode;
            InternalChange = false;

            Wall3DButton.Selected = settings.CitySkybox;
            FSOEnvironment.TexCompress = (settings.TexCompression & 1) > 0;
            CompressionButton.Selected = FSOEnvironment.TexCompress;

            UpdateModeText(GraphicsModeControl.Mode);

            var oldSurrounding = LotView.WorldConfig.Current.SurroundingLots;
            LotView.WorldConfig.Current = new LotView.WorldConfig()
            {
                LightingMode = settings.LightingMode,
                SmoothZoom = settings.SmoothZoom,
                SurroundingLots = settings.SurroundingLotMode,
                AA = settings.AntiAlias,
                Weather = settings.Weather,
                Directional = settings.DirectionalLight3D,
                Complex = settings.ComplexShaders,
                EnableTransitions = settings.EnableTransitions
            };

            var vm = ((IGameScreen)GameFacade.Screens.CurrentUIScreen)?.vm;
            if (vm != null)
            {
                vm.Context.World.ChangedWorldConfig(GameFacade.GraphicsDevice);
                if (oldSurrounding != settings.SurroundingLotMode)
                {
                    SimAntics.Utils.VMLotTerrainRestoreTools.RestoreSurroundings(vm, vm.HollowAdj);
                }
            }
        }

        public override void Removed()
        {
            base.Removed();
            GraphicsModeControl.ModeChanged -= UpdateModeText;
        }
    }

    public class UIDPIScaleDialog : UIDialog
    {
        public UILabel DPILabel;
        public UISlider DPISlider;
        public UIDPIScaleDialog() : base(UIDialogStyle.OK, true) {

            DPILabel = new UILabel();
            DPILabel.Position = new Vector2(25, 50);
            DPILabel.Size = new Vector2(350f, 0f);
            DPILabel.Alignment = TextAlignment.Center;
            DynamicOverlay.Add(DPILabel);

            DPISlider = new UISlider();
            DPISlider.Orientation = 0;
            DPISlider.Texture = GetTexture(0x42500000001);
            DPISlider.MinValue = 4f;
            DPISlider.MaxValue = 12f;
            DPISlider.AllowDecimals = false;
            DPISlider.Position = new Vector2(25, 80);
            DPISlider.SetSize(350f, 0f);

            DPISlider.Value = FSOEnvironment.DPIScaleFactor * 4;
            DynamicOverlay.Add(DPISlider);

            DPISlider.OnChange += DPISlider_OnChange;

            SetSize(400, 150);

            OKButton.OnButtonClick += (btn) =>
            {
                UIScreen.RemoveDialog(this);
            };
        }

        private void DPISlider_OnChange(UIElement element)
        {
            GameThread.NextUpdate((cb) =>
            {
                FSOEnvironment.DPIScaleFactor = DPISlider.Value / 4f;
                GlobalSettings.Default.DPIScaleFactor = FSOEnvironment.DPIScaleFactor;

                var width = Math.Max(1, GameFacade.Game.Window.ClientBounds.Width);
                var height = Math.Max(1, GameFacade.Game.Window.ClientBounds.Height);

                UIScreen.Current.ScaleX = UIScreen.Current.ScaleY = FSOEnvironment.DPIScaleFactor;

                GlobalSettings.Default.GraphicsWidth = (int)(width / FSOEnvironment.DPIScaleFactor);
                GlobalSettings.Default.GraphicsHeight = (int)(height / FSOEnvironment.DPIScaleFactor);

                UIScreen.Current.GameResized();
                GlobalSettings.Default.Save();
            });
        }

        public override void Update(UpdateState state)
        {
            ScaleX = 1f / FSOEnvironment.DPIScaleFactor;
            ScaleY = 1f / FSOEnvironment.DPIScaleFactor;
            DPILabel.Caption = (FSOEnvironment.DPIScaleFactor * 100).ToString() + "%";
            base.Update(state);
            Position = Vector2.Zero;
        }
    }
}
