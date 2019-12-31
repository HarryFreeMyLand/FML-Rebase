﻿/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.Content;
using FSO.Client.UI.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using FSO.Client.Rendering;
using FSO.Client.Utils;
using FSO.Common.Rendering.Framework.Model;
using FSO.Common.Rendering.Framework;
using FSO.Vitaboy;
using FSO.Common.Rendering.Framework.Camera;
using FSO.LotView.Utils;
using FSO.LotView;
using FSO.Client.UI.Framework.Parser;
using FSO.Common;

namespace FSO.Client.UI.Controls
{
    /// <summary>
    /// Renders a sim in the UI, this class just helps translate the UI world
    /// into the 3D world for sim rendering
    /// </summary>
    public class UISim : UIElement
    {
        private _3DTargetScene Scene;
        private WorldCamera Camera;
        public AdultVitaboyModel Avatar { get; internal set; }

        /** 45 degrees in either direction **/
        public float RotationRange = 45;
        public float RotationStartAngle = 45;
        public float RotationSpeed = new TimeSpan(0, 0, 10).Ticks;
        public bool AutoRotate = true;
        public float TimeOffset;
        
        protected string m_Timestamp;
        public float HeadXPos = 0.0f, HeadYPos = 0.0f;

        private WorldZoom Zoom = WorldZoom.Near;

        /// <summary>
        /// When was this character last cached by the client?
        /// </summary>
        public string Timestamp
        {
            get { return m_Timestamp; }
            set { m_Timestamp = value; }
        }
        
        private void UISimInit()
        {
            Vitaboy.Avatar.DefaultTechnique = (GlobalSettings.Default.Lighting) ? 3 : 0;
            Camera = new WorldCamera(GameFacade.GraphicsDevice);
            Camera.Zoom = Zoom;
            Camera.CenterTile = new Vector3(-1, -1, 0)*FSOEnvironment.DPIScaleFactor;
            Scene = new _3DTargetScene(GameFacade.GraphicsDevice, Camera, 
                new Point((int)(140 * FSOEnvironment.DPIScaleFactor), (int)(200 * FSOEnvironment.DPIScaleFactor)), 
                (GlobalSettings.Default.AntiAlias > 0)?8:0);
            Scene.ID = "UISim";

            GameFacade.Game.GraphicsDevice.DeviceReset += new EventHandler<EventArgs>(GraphicsDevice_DeviceReset);

            Avatar = new AdultVitaboyModel();
            Avatar.Scene = Scene;
            var scale = FSOEnvironment.DPIScaleFactor;
            Avatar.Scale = new Vector3(scale, scale, scale);
            
            Scene.Add(Avatar);
        }

        private Vector2 _Size;
        private Vector2 _SimScale;
        [UIAttribute("size")]
        public override Vector2 Size
        {
            get
            {
                return _Size;
            }

            set
            {
                _Size = value;
                _SimScale = new Vector2(1, 1) * (value.Y / 200f);
            }
        }

        public void SetZoom(WorldZoom zoom)
        {
            Zoom = zoom;
            if (Camera != null) Camera.Zoom = zoom;
        }

        public UISim() : this(true)
        {
        }

        public override void GameResized()
        {
            base.GameResized();
            Camera.CenterTile = new Vector3(-1, -1, 0) * FSOEnvironment.DPIScaleFactor;
            Scene.SetSize(new Point((int)(140 * FSOEnvironment.DPIScaleFactor), (int)(200 * FSOEnvironment.DPIScaleFactor)));
            var scale = FSOEnvironment.DPIScaleFactor;
            Avatar.Scale = new Vector3(scale, scale, scale);
            Camera.ProjectionDirty();
        }

        public override void Removed()
        {
            GameFacade.Scenes.RemoveExternal(Scene);
            Scene.Dispose();
        }

        public UISim(bool AddScene)
        {
            UISimInit();
            if (AddScene)
                GameFacade.Scenes.AddExternal(Scene);
        }

        private void GraphicsDevice_DeviceReset(object sender, EventArgs e)
        {
            Scene.DeviceReset(GameFacade.GraphicsDevice);
        }

        public override void Update(UpdateState state)
        {
            base.Update(state);
            if (AutoRotate)
            {
                var startAngle = RotationStartAngle;
                var time = state.Time.TotalGameTime.Ticks + TimeOffset;
                var phase = (time % RotationSpeed) / RotationSpeed;
                var multiplier = Math.Sin((Math.PI * 2) * phase);
                var newAngle = startAngle + (RotationRange * multiplier);
                Avatar.RotationY = (float)MathUtils.DegreeToRadian(newAngle);
            }
        }

        public override void PreDraw(UISpriteBatch batch)
        {
            if (!Visible) return;
            base.PreDraw(batch);
            if (!UISpriteBatch.Invalidated)
            {
                if (!_3DScene.IsInvalidated)
                {
                    batch.Pause();
                    Scene.Draw(GameFacade.GraphicsDevice);
                    batch.Resume();
                    DrawLocalTexture(batch, Scene.Target, new Vector2());
                }
            }
        }

        public override void Draw(UISpriteBatch batch)
        {
            if (!Visible) return;
            DrawLocalTexture(batch, Scene.Target, null, new Vector2((_Size.X - 140 * _SimScale.X) /2, 0), new Vector2(1f/FSOEnvironment.DPIScaleFactor, 1f/FSOEnvironment.DPIScaleFactor)*_SimScale);
        }
    }
}
