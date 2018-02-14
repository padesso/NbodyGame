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

        //Distance threshold
        public float Theta = 0.5f;

        //Time step modifier
        public float TimeStepModifier = 5.0f;

        private bool _showDebug = true;

        public Universe()
        {
            //Set some defaults
            Width = 500;
            Height = 500;
            
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
                //for (int rows = 0; rows < Height; rows += Width / 10)
                //{
                //    for(int cols = 0; cols < Width; cols += Height / 10)
                //    {
                //        float randMass = _rng.NextFloat(10000f, 5000000f);
                //        Body newBody = new Body(this.GameObj.Transform.Pos.X + cols, 
                //                                this.GameObj.Transform.Pos.Y + rows, 98f, randMass, 10f);
                //        AddBody(newBody);
                //    }
                //}
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
            //Debug.WriteLine("FPS: " + Time.Fps.ToString());

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
                    //canvas.DrawText("M: " + body.Mass + " | A: " + body.Acceleration.ToString() + " | G: " + body.Gravity.ToString(),
                    //    body.Position.X,
                    //    body.Position.Y + 4);

                    //TODO: why isn't this drawing?
                    //VisualLog.Default.DrawText(new Vector3(body.Position.X + 1, body.Position.Y + 4, 0), "M: " + body.Mass + " | A: " + body.Acceleration.ToString() + " | G: " + body.Gravity.ToString());
                }
            }
        }

        private void DrawQuadTreeDebug(QuadTree quadTree, IDrawDevice device, Canvas canvas)
        {
            canvas.DrawRect(quadTree.Bounds.X, quadTree.Bounds.Y, quadTree.Bounds.W, quadTree.Bounds.H);
            canvas.DrawText("CoM: " + CalculateCenterOfMass(quadTree).ToString() + " | M: " + CalculateMass(quadTree).ToString(),
                            quadTree.Bounds.X + 1, quadTree.Bounds.Y + 1);

            Vector3 screenPos = _camera.GetScreenCoord(new Vector3(quadTree.Bounds.X + 1, quadTree.Bounds.Y + 1, 0));
            //VisualLog.Default.DrawText(screenPos.X, screenPos.Y, "CoM: " + CalculateCenterOfMass(quadTree).ToString() + " | M: " + CalculateMass(quadTree).ToString());

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

        public double CalculateMass(QuadTree quadTree)
        {
            double totalMass = 0;

            if (quadTree.Body != null)
            {
                totalMass = quadTree.Body.Mass;
            }
            else
            {
                if (quadTree.NorthWest != null)
                    totalMass += CalculateMass(quadTree.NorthWest);

                if (quadTree.NorthEast != null)
                    totalMass += CalculateMass(quadTree.NorthEast);

                if (quadTree.SouthWest != null)
                    totalMass += CalculateMass(quadTree.SouthWest);

                if (quadTree.SouthEast != null)
                    totalMass += CalculateMass(quadTree.SouthEast);
            }

            return totalMass;
        }

        public Vector2 CalculateCenterOfMass(QuadTree quadTree)
        {
            /*
            x = m1*x1 + m2*x2 + ... / m1 + m2  + ...
            y = m1*y1 + m2*y2 + ... / m1 + m2  + ...
            */

            List<Body> treeBodies = quadTree.ToList();

            float comX = 0f;
            float comY = 0f;

            foreach (Body body in treeBodies)
            {
                comX += body.Mass * body.Position.X;
                comY += body.Mass * body.Position.Y;
            }

            float totalMass = (float)CalculateMass(quadTree);

            Vector2 centerOfMass = new Vector2(comX / totalMass, comY / totalMass);

            if (quadTree.Body != null)
            {
                centerOfMass = quadTree.Body.Position;
            }
            else
            {
                if (quadTree.NorthWest != null)
                    centerOfMass = CalculateCenterOfMass(quadTree.NorthWest);

                if (quadTree.NorthEast != null)
                    centerOfMass = CalculateCenterOfMass(quadTree.NorthEast);

                if (quadTree.SouthWest != null)
                    centerOfMass = CalculateCenterOfMass(quadTree.SouthWest);

                if (quadTree.SouthEast != null)
                    centerOfMass = CalculateCenterOfMass(quadTree.SouthEast);
            }

            return centerOfMass;
        }

        public void OnUpdate()
        {
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
                float massSize = _rng.NextFloat(100000f, 5000000f);
                Body newBody = new Body(mouseObjPos.X, mouseObjPos.Y, 59736f, massSize, 50f);
                AddBody(newBody);
            }

            //Rebuild the tree each from to account for movement           
            List<Body> bodyBuffer = _quadTree.ToList();
            foreach (Body body in bodyBuffer)
            {
                RemoveBody(body);
                ProcessBodies(body, bodyBuffer);
                AddBody(body);                
            }

            //TODO: remove mass calc???
            //CalculateMass(_quadTree);
            //CalculateCenterOfMass(_quadTree);
        }

        private void ProcessBodies(Body body, List<Body> bodies)
        {
            //brute force...  implement tree pruning here
            foreach(Body otherBody in bodies)
            {
                if (body == otherBody)
                    continue;

                // TODO: barnes-hut tree pruning
                body.Velocity += body.Acceleration * Time.TimeMult / TimeStepModifier;
                body.Position += body.Velocity * Time.TimeMult / TimeStepModifier;
                //body.Acceleration = Vector2.Zero;

                Vector2 r = otherBody.Position - body.Position;
                float dist = r.LengthSquared;
                Vector2 force = r / (float)(Math.Sqrt(dist) * dist);
                body.Acceleration = force * otherBody.Mass;
                otherBody.Acceleration -= force * body.Mass;
            }
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
