#region Using Statements
using System;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.UI;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Resources;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.UI;
using WaveEngine.Spine;
#endregion

namespace SpineSkeletalAnimation
{
    public class MyScene : Scene
    {
        protected override void CreateScene()
        {
            this.Load(WaveContent.Scenes.MyScene);

            var skeleton = EntityManager.Find("skeleton");

            #region UI
            Slider slider1 = new Slider()
            {
                Margin = new Thickness(10, 40, 0, 0),
                Width = 500,
                Minimum = -25,
                Maximum = 25,
                Value = 0
            };

            slider1.RealTimeValueChanged += (s, e) =>
            {
                var entity = EntityManager.Find("Light0");
                var component = skeleton.FindComponent<SkeletalAnimation>();
                component.Speed = e.NewValue / 10f;
            };

            EntityManager.Add(slider1);

            ToggleSwitch debugMode = new ToggleSwitch()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 10, 0),
                IsOn = false,
                OnText = "Debug On",
                OffText = "Debug Off",
                Width = 200
            };

            debugMode.Toggled += (s, o) =>
            {
                RenderManager.DebugLines = ((ToggleSwitch)s).IsOn;
            };

            EntityManager.Add(debugMode);

            CheckBox debugBones = new CheckBox()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 60, 10, 0),
                Text = "Bones",
                Width = 150,
            };

            debugBones.Checked += (s, o) =>
            {
                if (o.Value)
                {
                    skeleton.FindComponent<SkeletalRenderer>().ActualDebugMode |= SkeletalRenderer.DebugMode.Bones;
                }
                else
                {
                    skeleton.FindComponent<SkeletalRenderer>().ActualDebugMode &= SkeletalRenderer.DebugMode.Quads;
                }
            };

            EntityManager.Add(debugBones);

            CheckBox debugQuads = new CheckBox()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 110, 10, 0),
                Text = "Quads",
                Width = 150,
            };

            debugQuads.Checked += (s, o) =>
            {
                if (o.Value)
                {
                    skeleton.FindComponent<SkeletalRenderer>().ActualDebugMode |= SkeletalRenderer.DebugMode.Quads;
                }
                else
                {
                    skeleton.FindComponent<SkeletalRenderer>().ActualDebugMode &= SkeletalRenderer.DebugMode.Bones;
                }
            };

            EntityManager.Add(debugQuads);
            #endregion
        }
    }
}
