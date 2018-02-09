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
        Camera _camera;
        private int _width;
        private int _height;

        [DontSerialize]
        private QuadTree _quadTree;
       
        //Distance threshold
        private float _theta = 0.5f;

        private bool _showQuadTreeBorders = true;

        public Universe()
        {
            //Set some defaults
            Width = 500;
            Height = 500;
        }

        public void OnInit(InitContext context)
        {
            if (context == InitContext.Activate)
            {
                _camera = this.GameObj.ParentScene.FindComponent<Camera>();
                _quadTree = new QuadTree(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, Width, Height);
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
            List<Body> allBodies = _quadTree.Bodies;
            foreach (Body body in allBodies)
            {
                canvas.FillCircle(body.Node.Position.X, body.Node.Position.Y, 2);

                if (ShowDebug)
                {
                    canvas.DrawText("P: " + body.Node.Position.ToString() + " | M: " + body.Mass + " | F: " + body.Force.ToString(),
                        body.Node.Position.X, 
                        body.Node.Position.Y + 4);
                }
            }
        }

        private void DrawQuadTreeDebug(QuadTree quadTree, IDrawDevice device, Canvas canvas)
        {
            canvas.DrawRect(quadTree.Bounds.X, quadTree.Bounds.Y, quadTree.Bounds.W, quadTree.Bounds.H);
            MassDistribution dist = quadTree.Distribution();
            canvas.DrawText("CoM: " + dist.CenterOfMass.ToString() + " | M: " + dist.Mass,
                            quadTree.Bounds.X + 1, quadTree.Bounds.Y + 1);

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
            //Add a node when mouse is clicked
            if (DualityApp.Mouse.ButtonHit(MouseButton.Left))
            {
                Vector3 mouseObjPos = _camera.GetSpaceCoord(DualityApp.Mouse.Pos);
                Body newBody = new Body(mouseObjPos.X, mouseObjPos.Y, 100f);
                AddBody(newBody);
            }

            //Rebuild the tree each from to account for movement           
            List<Body> bodyBuffer = new List<Body>(_quadTree.Bodies);
            foreach (Body body in bodyBuffer)
            {
                RemoveBody(body);
                body.Node.Position = new Vector2(body.Node.Position.X + body.Force.X * Duality.Time.TimeMult,
                                                    body.Node.Position.Y + body.Force.Y * Duality.Time.TimeMult);
                AddBody(body);
            }
        }

        public void OnShutdown(ShutdownContext context)
        {
            
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
                return _showQuadTreeBorders;
            }

            set
            {
                _showQuadTreeBorders = value;
            }
        }
    }
}
