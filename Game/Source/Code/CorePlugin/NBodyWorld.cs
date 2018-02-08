using System;
using System.Collections.Generic;
using System.Linq;

using Duality;
using Output.DataStructures;
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

        [DontSerialize]
        private Transform _transform;

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
                _transform = this.GameObj.GetComponent<Transform>();
                _quadTree = new QuadTree(new Rect(_transform.Pos.X, _transform.Pos.Y, Width, Height));

                //Random rand = new Random(DateTime.Now.Millisecond);
                //for (int i = 0; i < 100; i++)
                //{
                //    _quadTree.Insert(new Node(rand.Next(-100, 100), rand.Next(-100, 100)));
                //}

                //_quadTree.Insert(new Node(5, 5));
                //_quadTree.Insert(new Node(25, 25));
                //_quadTree.Insert(new Node(55, 55));                
                //_quadTree.Insert(new Node(175, 75));
                //_quadTree.Insert(new Node(75, 175));
                //_quadTree.Insert(new Node(375, 375));
            }
        }

        public void OnShutdown(ShutdownContext context)
        {
            _quadTree = null;
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
            if (ShowQuadTreeBorders)
            {
                Canvas canvas = new Canvas(device);

                //Draw the quadtree bounds
                DrawQuadTreeBounds(_quadTree, device, canvas);

                //TODO: move this into the draw for the Body object
                List<Node> allNodes = _quadTree.QueryBounds(new Rect(_transform.Pos.X, _transform.Pos.Y, Width, Height));
                foreach (Node node in allNodes)
                {
                    canvas.FillCircle(node.Position.X, node.Position.Y, 2);
                }
            }
        }

        private void DrawQuadTreeBounds(QuadTree quad, IDrawDevice device, Canvas canvas)
        {
            canvas.DrawRect(quad.Bounds.X, quad.Bounds.Y, quad.Bounds.W, quad.Bounds.H);

            //Recursively draw the children of this quad
            if (quad.NorthWest != null)
                DrawQuadTreeBounds(quad.NorthWest, device, canvas);

            if (quad.NorthEast != null)
                DrawQuadTreeBounds(quad.NorthEast, device, canvas);

            if (quad.SouthWest != null)
                DrawQuadTreeBounds(quad.SouthWest, device, canvas);

            if (quad.SouthEast != null)
                DrawQuadTreeBounds(quad.SouthEast, device, canvas);
        }

        public void OnUpdate()
        {
            //Add a node when mouse is clicked
            if (DualityApp.Mouse.ButtonHit(MouseButton.Left))
            {
                Vector3 mouseObjPos = _camera.GetSpaceCoord(DualityApp.Mouse.Pos);
                _quadTree.Insert(new Node(mouseObjPos.X, mouseObjPos.Y));
            }

            //Randomly move nodes around
            //Random rand = new Random(DateTime.Now.Millisecond);
            //List<Node> allNodes = _quadTree.QueryBounds(new Rect(_transform.Pos.X, _transform.Pos.Y, Width, Height));
            //foreach (Node node in allNodes)
            //{
            //    _quadTree.Delete(node);
            //    node.Position = new Vector2(node.Position.X + rand.NextFloat(-0.5f, 0.5f) * Duality.Time.TimeMult,
            //                                 node.Position.Y + rand.NextFloat(-0.5f, 0.5f) * Duality.Time.TimeMult);
            //    _quadTree.Insert(node);
            //}
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

        public bool ShowQuadTreeBorders
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
