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
        private Region _topRegion;
       
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
                _topRegion = new Region(this.GameObj.Transform.Pos.X, this.GameObj.Transform.Pos.Y, Width, Height);
            }
        }

        public bool AddBody(Body body)
        {
            if (_topRegion.AddBody(body))
                return true;
            
            return false;
        }

        public bool RemoveBody(Body body)
        {
            if(_topRegion.RemoveBody(body))
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
            if (ShowRegionDebug)
            {
                Canvas canvas = new Canvas(device);

                //Draw the quadtree bounds
                DrawRegionDebug(_topRegion.QuadTree, device, canvas);

                //Draw the bodies
                List<Body> allBodies = _topRegion.Bodies;
                foreach (Body body in allBodies)
                {
                    canvas.FillCircle(body.Node.Position.X, body.Node.Position.Y, 2);
                }
            }
        }

        private void DrawRegionDebug(QuadTree quad, IDrawDevice device, Canvas canvas)
        {
            canvas.DrawRect(quad.Bounds.X, quad.Bounds.Y, quad.Bounds.W, quad.Bounds.H);
            

            //Recursively draw the children of this quad
            if (quad.NorthWest != null)
                DrawRegionDebug(quad.NorthWest, device, canvas);

            if (quad.NorthEast != null)
                DrawRegionDebug(quad.NorthEast, device, canvas);

            if (quad.SouthWest != null)
                DrawRegionDebug(quad.SouthWest, device, canvas);

            if (quad.SouthEast != null)
                DrawRegionDebug(quad.SouthEast, device, canvas);
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
            List<Body> bodyBuffer = new List<Body>(_topRegion.Bodies);
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

        public bool ShowRegionDebug
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
