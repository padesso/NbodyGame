using System;
using System.Collections.Generic;
using System.Linq;

using Duality;
using DataStructures;
using Duality.Components;
using Duality.Drawing;
using Duality.Resources;
using System.Diagnostics;
using Duality.Input;

namespace NBody
{
    [RequiredComponent(typeof(Transform))]
    public class Universe : Component, ICmpInitializable, ICmpUpdatable, ICmpRenderer
	{
        [DontSerialize]
        Camera _camera;

        [DontSerialize]
        private int _width;

        [DontSerialize]
        private int _height;

        [DontSerialize]
        private QuadTree _quadTree;

        [DontSerialize]
        private Random _rng;

#if DEBUG
        [DontSerialize]
        Stopwatch _perfTimer;
#endif

        //Distance threshold
        public float Theta = 0.5f;

        //Time step modifier
        public float TimeStepModifier = 1.0f;

        private bool _showDebug = false;

        public Universe()
        {
            //Set some defaults
            Width = 1000;
            Height = 1000;
#if DEBUG
            _perfTimer = new Stopwatch();
#endif

            _rng = new Random(DateTime.Now.Millisecond);
        }

        public void OnInit(InitContext context)
        {
            if (context == InitContext.Activate)
            {
                _camera = this.GameObj.ParentScene.FindComponent<Camera>();                
                _quadTree = new QuadTree(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, Width, Height);
                _camera.GameObj.Transform.Pos = new Vector3(this.GameObj.Transform.Pos.X + Width / 2.0f,
                                                            this.GameObj.Transform.Pos.Y + Height / 2.0f,
                                                            -1 * Width);

                //Let's setup some test data
                //for (int rows = 2000; rows < 3000; rows += 100)
                //{
                //    for (int cols = 2000; cols < 3000; cols += 100)
                //    {
                //        float randMass = _rng.NextFloat(1f, 5f);
                //        Body newBody = new Body(this.GameObj.Transform.Pos.X + cols,
                //                                this.GameObj.Transform.Pos.Y + rows, randMass, 9.8f, 10f);
                //        //newBody.Velocity = new Vector2(1, 0);
                //        AddBody(newBody);
                //    }
                //}

                //Body newBody = new Body(0, -50, 100f, 9.8f, 10f);
                //newBody.Velocity = new Vector2(1, 0);
                //Body newBody1 = new Body(0, 50, 100f, 9.8f, 10f);
                //newBody1.Velocity = new Vector2(-1, 0);
                //AddBody(newBody);
                //AddBody(newBody1);

                //BENCHMARKING
                Stopwatch timer = new Stopwatch();
                timer.Start();
                for (int bodyIndex = 0; bodyIndex < 500; bodyIndex++)
                {
                    float randMass = _rng.NextFloat(1f, 5f);
                    Body newBody = new Body(this.GameObj.Transform.Pos.X + _rng.NextFloat(10, Width - 10),
                                            this.GameObj.Transform.Pos.Y + _rng.NextFloat(10, Height - 10),
                                            randMass, 9.8f, 10f);
                    //newBody.Velocity = new Vector2(1, 0);
                    AddBody(newBody);
                }
                timer.Stop();
                Debug.WriteLine("Insert time (ms): " + timer.ElapsedMilliseconds);

                timer.Restart();
                List<Body> allBodies = _quadTree.ToList();
                timer.Stop();
                Debug.WriteLine("Time to query all: (ms): " + timer.ElapsedMilliseconds);

                //timer.Restart();
                //foreach (Body body in allBodies)
                //{
                //    _quadTree.RemoveBody(body);
                //}
                //timer.Stop();
                //Debug.WriteLine("Time to remove all: (ms): " + timer.ElapsedMilliseconds);
            }
        }

        public bool AddBody(Body body)
        {
            if (_quadTree.AddBody(body))
                return true;
            
            return false;
        }

        public bool RemoveBody(Body body)
        {
            if(_quadTree.RemoveBody(body))
                return true;

            return false;
        }
                
        public bool IsVisible(IDrawDevice device)
        {
            bool anyGroupFlag =
                (device.VisibilityMask & VisibilityFlag.AllGroups)
                != VisibilityFlag.None;

            bool screenOverlayFlag =
                (device.VisibilityMask & VisibilityFlag.ScreenOverlay)
                != VisibilityFlag.None;

            if (!anyGroupFlag) return false;
            if (screenOverlayFlag) return false;

            return true;
        }

        public void Draw(IDrawDevice device)
        {
            Canvas canvas = new Canvas(device);
            
            if (ShowDebug)
            {
                //Draw the quadtree bounds
                DrawQuadTreeDebug(_quadTree, device, canvas);
            }

            //Draw the bodies
            List<Body> allBodies = _quadTree.ToList();
            foreach (Body body in allBodies)
            {
                canvas.FillCircle(body.Position.X, body.Position.Y, body.Radius);

                if (ShowDebug)
                {
                    canvas.DrawText("P: " + body.Position + " | M: " + body.Mass + " | A: " + body.Acceleration.ToString() + " | G: " + body.Gravity.ToString(),
                        body.Position.X,
                        body.Position.Y + 4);
                }
            }
        }

        private void DrawQuadTreeDebug(QuadTree quadTree, IDrawDevice device, Canvas canvas)
        {
            canvas.DrawRect(quadTree.Bounds.X, quadTree.Bounds.Y, quadTree.Bounds.W, quadTree.Bounds.H);
            canvas.DrawText("CoM: " + quadTree.CenterOfMass.ToString() + " | M: " + quadTree.Mass.ToString(),
                            quadTree.Bounds.CenterX, quadTree.Bounds.CenterY);

            //Recursively draw the children of this quad
            if (quadTree.NorthWest != null)
                DrawQuadTreeDebug(quadTree.NorthWest, device, canvas);

            if (quadTree.NorthEast != null)
                DrawQuadTreeDebug(quadTree.NorthEast, device, canvas);

            if (quadTree.SouthWest != null)
                DrawQuadTreeDebug(quadTree.SouthWest, device, canvas);

            if (quadTree.SouthEast != null)
                DrawQuadTreeDebug(quadTree.SouthEast, device, canvas);
        }

        public void OnUpdate()
        {
#if DEBUG
            VisualLog.Default.DrawText(new Vector2(3, 3), "Bodies: " + _quadTree.ToList().Count);
            VisualLog.Default.DrawText(new Vector2(3, 13), "FPS: " + Time.Fps.ToString());
#endif

            //Add a planet when mouse is clicked
            if (DualityApp.Mouse.ButtonHit(MouseButton.Left))
            {
                Vector3 mouseObjPos = _camera.GetSpaceCoord(DualityApp.Mouse.Pos);
                float massSize = _rng.NextFloat(10f, 2500f); 
                Body newBody = new Body(mouseObjPos.X, mouseObjPos.Y, 5.9736f, massSize, 10f);
                AddBody(newBody);
            }

            //Add a star
            if (DualityApp.Mouse.ButtonHit(MouseButton.Right))
            {
                Vector3 mouseObjPos = _camera.GetSpaceCoord(DualityApp.Mouse.Pos);
                float massSize = _rng.NextFloat(1000f, 50000f);
                Body newBody = new Body(mouseObjPos.X, mouseObjPos.Y, 59736f, massSize, 50f);
                AddBody(newBody);
            }

#if DEBUG
            _perfTimer.Restart();
#endif
            //Rebuild the tree to account for movement           
            List<Body> bodyBuffer = _quadTree.ToList();
            foreach (Body body in bodyBuffer)
            {
                ProcessBodies(body, _quadTree);
            }
#if DEBUG
            _perfTimer.Stop();
            VisualLog.Default.DrawText(new Vector2(3, 23), "Process Time: " + _perfTimer.ElapsedMilliseconds);
#endif

            _quadTree = new QuadTree(_quadTree.Bounds.TopLeft.X,
                                        _quadTree.Bounds.TopLeft.Y,
                                        _quadTree.Bounds.W,
                                        _quadTree.Bounds.H,
                                        bodyBuffer);
        }

        //TODO: is CoM being populated properly?
        private void ProcessBodies(Body body, QuadTree quadTree)
        {
            if (quadTree.Body != null) //external node
            {
                if (body == quadTree.Body) //Don't compare to self
                    return;

                body.Velocity += body.Acceleration * Time.TimeMult / TimeStepModifier;
                body.Position += body.Velocity * Time.TimeMult / TimeStepModifier;
                body.Acceleration = Vector2.Zero;

                Vector2 r = quadTree.Body.Position - body.Position;
                float dist = r.LengthSquared;
                Vector2 force = r / (float)(Math.Sqrt(dist) * dist);
                body.Acceleration += force * quadTree.Body.Mass;
                quadTree.Body.Acceleration -= force * body.Mass;
            }
            else if (quadTree.Bounds.W / Distance(body.Position, quadTree.CenterOfMass) < Theta)  //Treat the quadtree as a group of nodes
            {
                body.Velocity += body.Acceleration * Time.TimeMult / TimeStepModifier;
                body.Position += body.Velocity * Time.TimeMult / TimeStepModifier;
                body.Acceleration = Vector2.Zero;

                Vector2 r = quadTree.CenterOfMass - body.Position;
                float dist = r.LengthSquared;
                Vector2 force = r / (float)(Math.Sqrt(dist) * dist);
                body.Acceleration += force * quadTree.Mass;
                //quadTree.Body.Acceleration -= force * body.Mass;
            }
            else //Recurse down and repeat process
            {
                if (quadTree.NorthWest != null)
                    ProcessBodies(body, quadTree.NorthWest);

                if (quadTree.NorthEast != null)
                    ProcessBodies(body, quadTree.NorthEast);

                if (quadTree.SouthWest != null)
                    ProcessBodies(body, quadTree.SouthWest);

                if (quadTree.SouthEast != null)
                    ProcessBodies(body, quadTree.SouthEast);
            }
        }

        private float Distance(Vector2 position1, Vector2 position2)
        {
            //Vector2 r = position1 - position2;
            return (float)Math.Sqrt(Math.Pow(position2.X - position1.X, 2) + Math.Pow(position2.Y - position1.Y, 2));
        }

        public void OnShutdown(ShutdownContext context)
        {
            //TODO: clean up
        }

        public float BoundRadius
        {
            get
            {
                //half the hypotenuse of the rectangle
                return (float)(Math.Sqrt(Width * Width + Height * Height) / 2.0);
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }

            set
            {
                if (value <= 10)
                {
                    _width = 10;
                }
                else
                {
                    _width = value;
                }
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }

            set
            {
                if (value <= 10)
                {
                    _height = 10;
                }
                else
                {
                    _height = value;
                }
            }
        }

        public bool ShowDebug
        {
            get
            {
                return _showDebug;
            }

            set
            {
                _showDebug = value;
            }
        }
    }
}
