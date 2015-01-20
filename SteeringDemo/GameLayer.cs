using System;
using System.Collections.Generic;
using System.Linq;
using CocosSharp;
using CocosSharpSteer;
using SteeringDemo.PlugIns;
using SteeringDemo.PlugIns.Arrival;
using SteeringDemo.PlugIns.Boids;
using SteeringDemo.PlugIns.MultiplePursuit;
using SteeringDemo.PlugIns.OneTurning;
using SteeringDemo.PlugIns.LowSpeedTurn;
using SteeringDemo.PlugIns.MeshPathFollowing;
using SteeringDemo.PlugIns.Wanderer;
using SteeringDemo.PlugIns.Ctf;

namespace SteeringDemo
{

    public class Arrow : CCDrawNode
    {
        CCPoint[] arrowPoints = {new CCPoint(-1.0f, 0.6f),
            new CCPoint(1.0f, 0.0f),
            new CCPoint(-1.0f, -0.6f),
        };

        float radius = .20f;

        public Arrow()
            : base()
        {
            ContentSize = new CCSize(25, 25);
            Color = CCColor3B.White;
            Opacity = 255;

            var arrow = new CCPoint[arrowPoints.Length];
            for (int a = 0; a < arrowPoints.Length; a++)
            {
                arrow[a] = (CCPoint)(ContentSize * (CCSize)arrowPoints[a]);
            }

            DrawPolygon(arrow,
                3, CCColor4B.White, 0, CCColor4B.Transparent);

            DrawSolidCircle(CCPoint.Zero - new CCPoint(radius / 2, radius / 2), ContentSize.Width * radius, CCColor4B.Red);
        }

        //        public override void OnEnter()
        //        {
        //            base.OnEnter();
        //
        //            var bounds = VisibleBoundsWorldspace;
        //
        //
        //            var arrow = new CCPoint[arrowPoints.Length];
        //            for (int a = 0; a < arrowPoints.Length; a++)
        //            {
        //                arrow[a] = (CCPoint)(ContentSize * (CCSize)arrowPoints[a]);
        //            }
        //
        //            DrawPolygon(arrow, 
        //                3, CCColor4B.White, 0, CCColor4B.Transparent);
        //
        //            DrawSolidCircle(CCPoint.Zero - new CCPoint(radius / 2, radius / 2), ContentSize.Width * radius, CCColor4B.Red);
        //        }
    }


    public class GameLayer : CCLayerColor
    {
        Arrow arrow;
        SimpleVehicle vehicle;
        Annotation annotation = new Annotation();
        // currently selected plug-in (user can choose or cycle through them)
        private static PlugIn _selectedPlugIn = null;

        // currently selected vehicle.  Generally the one the camera follows and
        // for which additional information may be displayed.  Clicking the mouse
        // near a vehicle causes it to become the Selected Vehicle.
        public static IVehicle SelectedVehicle = null;

        public static readonly Clock Clock = new Clock();

        public static CCLabelTtf statusLabel;
        public static CCLabelTtf pluginLabel;
        static float mouseX, mouseY;

        public GameLayer()
            : base(CCCameraProjection.Projection2D, CCColor4B.Black)
        {

            // Load and instantate your assets here

            // Make any renderable node objects (e.g. sprites) children of this layer
            arrow = new Arrow();
            //arrow.AnchorPoint = CCPoint.AnchorMiddle;
            AddChild(arrow);

            Annotation.Drawer = new Drawing();

            new ArrivalPlugIn(this, annotation);
            new CtfPlugIn(this, annotation);
            new BoidsPlugIn(this, annotation);
            new MpPlugIn(this, annotation);
            new OneTurningPlugIn(this, annotation);
            new LowSpeedTurnPlugIn(this, annotation);
            new MeshPathFollowingPlugin(this, annotation);
            new WandererPlugIn(this, annotation);

            statusLabel = new CCLabelTtf()
            {
                FontName = "MarkerFelt",
                FontSize = 12,
                HorizontalAlignment = CCTextAlignment.Left,
                AnchorPoint = CCPoint.AnchorMiddleLeft
            };

            AddChild(statusLabel, 10, 100);

            pluginLabel = new CCLabelTtf()
            {
                FontName = "MarkerFelt",
                FontSize = 12,
                HorizontalAlignment = CCTextAlignment.Left,
                AnchorPoint = CCPoint.AnchorMiddleLeft
            };

            AddChild(pluginLabel, 10, 101);

            // Register for touch events
            var touchListener = new CCEventListenerTouchAllAtOnce();
            touchListener.OnTouchesEnded = OnTouchesEnded;

            var mouseListener = new CCEventListenerMouse();
            mouseListener.OnMouseMove = OnMouseMoved;

            var keyboarListener = new CCEventListenerKeyboard();
            keyboarListener.OnKeyPressed = OnKeyPressed;

            AddEventListener(touchListener);
            AddEventListener(mouseListener);
            AddEventListener(keyboarListener);

            Schedule();
        }

        protected override void AddedToScene()
        {
            base.AddedToScene();

            // Use the bounds to layout the positioning of our drawable assets
            CCRect bounds = VisibleBoundsWorldspace;

            statusLabel.PositionX = 20;
            statusLabel.PositionY = bounds.Size.Height - 50;

            //arrow.Position = bounds.Center;
            Globals.HomeBaseCenter = bounds.Center;

            SelectDefaultPlugIn();
            OpenSelectedPlugIn();

            pluginLabel.PositionX = 20;
            pluginLabel.PositionY = bounds.Size.Height - pluginLabel.ContentSize.Height / 2;

        }

        void OnKeyPressed(CCEventKeyboard keyEvent)
        {
            if (keyEvent.Keys == CCKeys.R)
                ResetSelectedPlugIn();
            if (keyEvent.Keys == CCKeys.S)
                SelectNextVehicle();
            if (keyEvent.Keys == CCKeys.A)
                annotation.IsEnabled = !annotation.IsEnabled;
            if (keyEvent.Keys == CCKeys.Space)
                Clock.TogglePausedState();
            //            if (keyEvent.Keys == CCKeys.F)
            //                SelectNextPresetFrameRate();
            if (keyEvent.Keys == CCKeys.Tab)
            {
                Globals.HomeBaseCenter = VisibleBoundsWorldspace.Center;
                SelectNextPlugin();
            }

            //if (keyEvent.Keys >= CCKeys.F1 && keyEvent.Keys <= CCKeys.F10)
            //{
                _selectedPlugIn.HandleKeys(keyEvent.Keys);
            //}
        }

        void OnMouseMoved(CCEventMouse mouse)
        {
            var mouseC = new CCPoint(mouse.CursorX, mouse.CursorY);
            mouseC = this.ScreenToWorldspace(mouseC);
            mouseX = mouseC.X;
            mouseY = mouseC.Y;
            //if (_selectedPlugIn.Name == "Boids")
            //    Globals.HomeBaseCenter = mouseC;

        }

        void OnTouchesEnded(List<CCTouch> touches, CCEvent touchEvent)
        {
            if (touches.Count > 0)
            {

                Globals.HomeBaseCenter = touches[0].Location;
            }
        }

        private static void SelectNextPlugin()
        {
            CloseSelectedPlugIn();
            _selectedPlugIn = _selectedPlugIn.Next();
            OpenSelectedPlugIn();
        }

        // select the "next" plug-in, cycling through "plug-in selection order"
        static void SelectDefaultPlugIn()
        {
            PlugIn.SortBySelectionOrder();
            _selectedPlugIn = PlugIn.FindDefault();
        }

        // open the currently selected plug-in
        static void OpenSelectedPlugIn()
        {
            //Camera.Reset();
            SelectedVehicle = null;
            _selectedPlugIn.Open();
            pluginLabel.Text = "Plugin: " + _selectedPlugIn.Name;
        }

        static void ResetSelectedPlugIn()
        {
            _selectedPlugIn.Reset();
        }

        static void CloseSelectedPlugIn()
        {
            _selectedPlugIn.Close();
            SelectedVehicle = null;
            pluginLabel.Text = string.Empty;
            statusLabel.Text = string.Empty;
        }

        // draws a colored circle (perpendicular to view axis) around the center
        // of a given vehicle.  The circle's radius is the vehicle's radius times
        // radiusMultiplier.
        public static void DrawCircleHighlightOnVehicle(IVehicle v, float radiusMultiplier, CCColor4B color)
        {
            if (v != null)
            {
                CCVector2 cPosition = SelectedVehicle.Position;
                Drawing.Draw3dCircle(
                    v.Radius * radiusMultiplier,  // adjusted radius
                    v.Position,                   // center
                    v.Position - cPosition,       // view axis
                    color,                        // drawing color
                    20);                          // circle segments
            }
        }


        // Find the AbstractVehicle whose screen position is nearest the current the
        // mouse position.  Returns NULL if mouse is outside this window or if
        // there are no AbstractVehicle.
        internal static IVehicle VehicleNearestToMouse()
        {
            return FindVehicleNearestScreenPosition((int)mouseX, (int)mouseY);
        }

        // Find the AbstractVehicle whose screen position is nearest the given window
        // coordinates, typically the mouse position.  Returns NULL if there are no
        // AbstractVehicles.
        //
        // This works by constructing a line in 3d space between the camera location
        // and the "mouse point".  Then it measures the distance from that line to the
        // centers of each AbstractVehicle.  It returns the AbstractVehicle whose
        // distance is smallest.
        //
        // xxx Issues: Should the distanceFromLine test happen in "perspective space"
        // xxx or in "screen space"?  Also: I think this would be happy to select a
        // xxx vehicle BEHIND the camera location.
        internal static IVehicle FindVehicleNearestScreenPosition(int x, int y)
        {
            // find the direction from the camera position to the given pixel
            CCVector2 direction = new CCVector2(x, y);//DirectionFromCameraToScreenPosition(x, y);

            // iterate over all vehicles to find the one whose center is nearest the
            // "eye-mouse" selection line
            float minDistance = float.MaxValue;       // smallest distance found so far
            IVehicle nearest = null;   // vehicle whose distance is smallest
            IEnumerable<IVehicle> vehicles = AllVehiclesOfSelectedPlugIn();
            foreach (IVehicle vehicle in vehicles)
            {
                // distance from this vehicle's center to the selection line:
                float d = CCVector2.Distance(vehicle.Position, direction);//CCVector2Helpers.DistanceFromLine(vehicle.Position, Camera.Position, direction);

                // if this vehicle-to-line distance is the smallest so far,
                // store it and this vehicle in the selection registers.
                if (d < minDistance)
                {
                    minDistance = d;
                    nearest = vehicle;
                }
            }

            return nearest;
        }

        // ground plane grid-drawing utility used by several plug-ins
        public static void GridUtility(CCVector2 gridTarget)
        {
            // Math.Round off target to the nearest multiple of 2 (because the
            // checkboard grid with a pitch of 1 tiles with a period of 2)
            // then lower the grid a bit to put it under 2d annotation lines
            //CCVector2 gridCenter = new CCVector2((float)(Math.Round(gridTarget.X * 0.5f) * 2),
            //                       (float)(Math.Round(gridTarget.Y * 0.5f) * 2) - .05f,
            //                       (float)(Math.Round(gridTarget.Z * 0.5f) * 2));
            CCVector2 gridCenter = new CCVector2((float)(Math.Round(gridTarget.X * 0.5f) * 2),
                                   //(float)(Math.Round(gridTarget.Y * 0.5f) * 2) - .05f,
                                   (float)(Math.Round(gridTarget.Y * 0.5f) * 2));

            gridCenter = CCVector2.Zero;

            // colors for checkboard
            CCColor4B gray1 = CCColor4B.LightGray; // new Color4B(0.27f);
            CCColor4B gray2 = CCColor4B.Gray; // new Color4B(0.30f);

            // draw 50x50 checkerboard grid with 50 squares along each side
            //Drawing.DrawXYCheckerboardGrid(50, 50, gridCenter, gray1, gray2);

            // alternate style
            //Bnoerj.AI.Steering.Draw.drawXZLineGrid(50, 50, gridCenter, Color.Black);
        }
        // return a group (an STL vector of AbstractVehicle pointers) of all
        // vehicles(/agents/characters) defined by the currently selected PlugIn
        static IEnumerable<IVehicle> AllVehiclesOfSelectedPlugIn()
        {
            return _selectedPlugIn.Vehicles;
        }

        // select the "next" vehicle: the one listed after the currently selected one
        // in allVehiclesOfSelectedPlugIn
        static void SelectNextVehicle()
        {
            if (SelectedVehicle != null)
            {
                // get a container of all vehicles
                IVehicle[] all = AllVehiclesOfSelectedPlugIn().ToArray();

                // find selected vehicle in container
                int i = Array.FindIndex(all, v => v != null && v == SelectedVehicle);
                if (i >= 0 && i < all.Length)
                {
                    if (i == all.Length - 1)
                    {
                        // if we are at the end of the container, select the first vehicle
                        SelectedVehicle = all[0];
                    }
                    else
                    {
                        // normally select the next vehicle in container
                        SelectedVehicle = all[i + 1];
                    }
                }
                else
                {
                    // if the search failed, use NULL
                    SelectedVehicle = null;
                }
            }
        }


        static void UpdateSelectedPlugIn(float currentTime, float elapsedTime)
        {
            // switch to Update phase
            PushPhase(Phase.Update);

            // service queued reset request, if any
            DoDelayedResetPlugInXXX();

            // if no vehicle is selected, and some exist, select the first one
            if (SelectedVehicle == null)
            {
                IVehicle[] all = AllVehiclesOfSelectedPlugIn().ToArray();
                if (all.Length > 0)
                    SelectedVehicle = all[0];
            }

            // invoke selected PlugIn's Update method
            _selectedPlugIn.Update(currentTime, elapsedTime);

            // return to previous phase
            PopPhase();
        }

        static bool _delayedResetPlugInXXX = false;
        internal static void QueueDelayedResetPlugInXXX()
        {
            _delayedResetPlugInXXX = true;
        }

        static void DoDelayedResetPlugInXXX()
        {
            if (_delayedResetPlugInXXX)
            {
                ResetSelectedPlugIn();
                _delayedResetPlugInXXX = false;
            }
        }

        public override void Update(float dt)
        {


            base.Update(dt);
            // update global simulation clock
            Clock.Update();

            //  start the phase timer (XXX to accurately measure "overhead" time this
            //  should be in displayFunc, or somehow account for time outside this
            //  routine)
            InitPhaseTimers();

            // run selected PlugIn (with simulation's current time and step size)
            UpdateSelectedPlugIn(Clock.TotalSimulationTime, Clock.ElapsedSimulationTime);

        }


        // redraw graphics for the currently selected plug-in
        static void RedrawSelectedPlugIn(float currentTime, float elapsedTime)
        {

            // switch to Draw phase
            PushPhase(Phase.Draw);

            // invoke selected PlugIn's Draw method
            _selectedPlugIn.Redraw(currentTime, elapsedTime);

            // draw any annotation queued up during selected PlugIn's Update method
            Drawing.AllDeferredLines();
            Drawing.AllDeferredCirclesOrDisks();

            // return to previous phase
            PopPhase();
        }

        protected override void Draw()
        {
            base.Draw();

            // redraw selected PlugIn (based on real time)
            RedrawSelectedPlugIn(Clock.TotalRealTime, Clock.ElapsedRealTime);

        }

        static void PushPhase(Phase newPhase)
        {
            // update timer for current (old) phase: add in time since last switch
            UpdatePhaseTimers();

            // save old phase
            _phaseStack[_phaseStackIndex++] = _phase;

            // set new phase
            _phase = newPhase;

            // check for stack overflow
            if (_phaseStackIndex >= PHASE_STACK_SIZE)
            {
                throw new InvalidOperationException("Phase stack has overflowed");
            }
        }

        static void PopPhase()
        {
            // update timer for current (old) phase: add in time since last switch
            UpdatePhaseTimers();

            // restore old phase
            _phase = _phaseStack[--_phaseStackIndex];
        }

        private enum Phase
        {
            Overhead,
            Update,
            Draw,
            Count
        }
        static Phase _phase;
        const int PHASE_STACK_SIZE = 5;
        static readonly Phase[] _phaseStack = new Phase[PHASE_STACK_SIZE];
        static int _phaseStackIndex = 0;
        static readonly float[] _phaseTimers = new float[(int)Phase.Count];
        static float _phaseTimerBase = 0;

        // draw text showing (smoothed, rounded) "frames per second" rate
        // (and later a bunch of related stuff was dumped here, a reorg would be nice)
        static float _smoothedTimerDraw = 0;
        static float _smoothedTimerUpdate = 0;
        static float _smoothedTimerOverhead = 0;

        public static bool IsDrawPhase
        {
            get { return _phase == Phase.Draw; }
        }

        static float PhaseTimerDraw
        {
            get { return _phaseTimers[(int)Phase.Draw]; }
        }

        static float PhaseTimerUpdate
        {
            get { return _phaseTimers[(int)Phase.Update]; }
        }
        // XXX get around shortcomings in current implementation, see note
        // XXX in updateSimulationAndRedraw
#if IGNORE
        float phaseTimerOverhead
        {
        get { return phaseTimers[(int)Phase.overheadPhase]; }
        }
#else
        static float PhaseTimerOverhead
        {
            get { return Clock.ElapsedRealTime - (PhaseTimerDraw + PhaseTimerUpdate); }
        }
#endif

        static void InitPhaseTimers()
        {
            _phaseTimers[(int)Phase.Draw] = 0;
            _phaseTimers[(int)Phase.Update] = 0;
            _phaseTimers[(int)Phase.Overhead] = 0;
            _phaseTimerBase = Clock.TotalRealTime;
        }

        static void UpdatePhaseTimers()
        {
            float currentRealTime = Clock.RealTimeSinceFirstClockUpdate();
            _phaseTimers[(int)_phase] += currentRealTime - _phaseTimerBase;
            _phaseTimerBase = currentRealTime;
        }

        readonly List<TextEntry> _texts;
        public void AddText(TextEntry text)
        {
            _texts.Add(text);
        }
    }
}
