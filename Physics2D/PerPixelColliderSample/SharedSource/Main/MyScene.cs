#region Using Statements
using System;
using System.Collections.Generic;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Animation;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Animation;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics2D;
using WaveEngine.Framework.Resources;
using WaveEngine.Framework.Services;
#endregion

namespace PerPixelColliderSample
{
    public enum SampleState
    {
        Playing,
        Waiting
    }

    public class MyScene : Scene
    {
        public const int MAXOBSTACLES = 6;
        public const int WAITINGTIME = 2000;
        public const float SCROLLACCELERATION = -3;
        public const float SCROLLSPEED = -150;
        public const float SCROLLWIDTH = 1800;

        public IList<Entity> obstacles;
        public Entity ship;
        public Entity ground, ground2, ground3;
        public Entity explosion;
        private Animation2D explodeAnimation;

        private SampleState state;

        public int countDown;

        public SampleState State
        {
            get
            {
                return this.state;
            }

            set
            {
                this.state = value;

                if (state == SampleState.Playing)
                {
                    // Sets playing initial state
                    int i = 0;
                    float step = (SCROLLWIDTH / MAXOBSTACLES);
                    foreach (var obstacle in this.obstacles)
                    {
                        obstacle.IsActive = true;
                        var transform = obstacle.FindComponent<Transform2D>();
                        transform.X = step * (i + 3);
                        transform.Y = (float)WaveServices.Random.NextDouble() * WaveServices.Platform.ScreenHeight;
                        i++;
                    }

                    ScrollBehavior.ScrollSpeed = SCROLLSPEED;
                    this.ground.IsActive = true;
                    this.ground2.IsActive = true;
                    this.ground3.IsActive = true;
                    this.ship.Enabled = true;
                    this.ship.FindComponent<Transform2D>().Y = 0;
                    this.ship.FindComponent<ShipBehavior>().Reset();
                    this.explosion.Enabled = false;
                }
                else if (state == SampleState.Waiting)
                {
                    // Sets waiting initial state
                    foreach (var obstacles in this.obstacles)
                    {
                        obstacles.IsActive = false;
                    }

                    this.ship.Enabled = false;
                    this.ground.IsActive = false;
                    this.ground2.IsActive = false;
                    this.ground3.IsActive = false;

                    this.countDown = 0;
                }
            }
        }

        protected override void CreateScene()
        {
            this.Load(WaveContent.Scenes.MyScene);

            this.CreateShip();
            this.CreateExplosion();

            this.CreateGrounds();
            this.CreateObstacles();

            this.state = SampleState.Playing;
            ScrollBehavior.ScrollSpeed = SCROLLSPEED;

            this.AddSceneBehavior(new CollisionSceneBehavior(), SceneBehavior.Order.PostUpdate);
        }

        protected override void Start()
        {
            base.Start();

            WaveServices.Layout.PerformLayout();
        }
       
        private void CreateObstacles()
        {
            this.obstacles = new List<Entity>();
            float x;
            float step = SCROLLWIDTH / MAXOBSTACLES;
            for (int i = 0; i < MAXOBSTACLES; i++)
            {
                x = step * (i + 3);
                float y = (float)(WaveServices.Random.NextDouble() * this.VirtualScreenManager.VirtualHeight / this.VirtualScreenManager.RatioY);
                float scale = ((float)WaveServices.Random.NextDouble() * 2f) + 0.5f;
                var obstacle = new Entity("obstacle_" + i)
                .AddComponent(new Transform2D() { X = x, Y = y, XScale = scale, YScale = scale, Origin = new WaveEngine.Common.Math.Vector2(0.5f, 0.5f) })
                .AddComponent(new PerPixelCollider2D(WaveContent.Assets.asteroid_png, 0))
                .AddComponent(new Sprite(WaveContent.Assets.asteroid_png))
                .AddComponent(new SpriteRenderer(DefaultLayers.Alpha))
                .AddComponent(new ScrollBehavior(SCROLLWIDTH, true, true));

                this.obstacles.Add(obstacle);

                this.EntityManager.Add(obstacle);
            }
        }

        private void CreateGrounds()
        {
            this.ground = EntityManager.Find("ground1");
            this.ground2 = EntityManager.Find("ground2");
            this.ground3 = EntityManager.Find("ground3");

            this.UpdateGround(this.ground, this.VirtualScreenManager.TopEdge);
            this.UpdateGround(this.ground2, this.VirtualScreenManager.TopEdge + 1024);
            this.UpdateGround(this.ground3, this.VirtualScreenManager.TopEdge + 2048);
        }

        private void UpdateGround(Entity ground, float x)
        {
            var transform = ground.FindComponent<Transform2D>();
            transform.X = x;
            transform.Y = this.VirtualScreenManager.BottomEdge - 128;
        }

        private void CreateShip()
        {
            this.ship = EntityManager.Find("ship");

            var shipBurst = this.ship.FindChild("shipBurst");
            shipBurst.Enabled = false;
        }

        private void CreateExplosion()
        {
            this.explosion = EntityManager.Find("boom");
            this.explosion.Enabled = false;
            this.explodeAnimation = this.explosion.FindComponent<Animation2D>();

            this.explodeAnimation.AddKeyFrameEvent("boom", 14);

            this.explodeAnimation.OnKeyFrameEvent += this.explodeAnimation_OnKeyFrameEvent;
        }

        public void explodeAnimation_OnKeyFrameEvent(object sender, WaveEngine.Common.Helpers.StringEventArgs e)
        {
            this.explosion.IsVisible = false;
        }

        public void Explosion()
        {
            this.explosion.IsVisible = true;
            // Creates the explosions and adjusts to the ship position.
            this.explosion.Enabled = true;

            var explosionTransform = this.explosion.FindComponent<Transform2D>();
            var shipTransform = this.ship.FindComponent<Transform2D>();

            explosionTransform.X = shipTransform.X;
            explosionTransform.Y = shipTransform.Y;

            this.explodeAnimation.PlayAnimation("boom", false);
        }
    }
}
