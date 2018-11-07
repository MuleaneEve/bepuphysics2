#if TODO // TODO: OscillatorQuadruped
using System;
using System.Collections.Generic;
using System.Linq;
using BepuPhysics;
using BepuPhysics.Collidables;

namespace OscillatorQuadruped
{
    internal class Quadruped
    {
        private const int NUMBER_TESTING_SIZES = 1;

        private const float FIXED_LEG_SCALE = 1 / 0.35037037037037033f;//was 3.25. leg length for fixed training also used as the length to create the controller when in "display" mode04

        private static readonly float[] LEG_SIZES = { 0.30f };

        //In "display" mode this is the scale of the quadruped
        private const float LEG_SCALE = 0.35037037037037033f;

        //5.4, 2,3, 7.8
        private const float SCALE_FACTOR = 3.0f; //was 3    3, 3.5, 4.0
        private const float FOOTX_SZ = 0.55f / SCALE_FACTOR; //was 0.65
        private const float FOOTY_SZ = 0.15f / SCALE_FACTOR;
        private const float FOOTZ_SZ = 0.55f / SCALE_FACTOR;
        private const float LLEG_LEN = 1.0f; //was 1.0
        private const float LLEG_RAD = 0.2f / SCALE_FACTOR;  //was 0.2
        private const float ULEG_LEN = 1.0f;//was 1.0
        private const float ULEG_RAD = 0.2f / SCALE_FACTOR; //was 0.2
        private const float TORSO_LEN = 1.3f / SCALE_FACTOR; //was 1.3
        private const float TORSO_RAD = 0.3f / SCALE_FACTOR; //was 0.3
        private const float LEG_POS = 0.2f / SCALE_FACTOR;
        private const float TORSO_HEIGHT = 1.2f / SCALE_FACTOR;
        private const float TORSO_LEN2 = 3.5f / SCALE_FACTOR; //was 3.0
        private const float ORIG_HEIGHT = (TORSO_RAD / 2.0f + (ULEG_LEN / 4.0f) + (LLEG_LEN / 4.0f) + FOOTZ_SZ);  //SET TO SMALLEST POSSIBLE SIZE
        private const float LEG_MASS = 0.5f; //was 0.5  
        private const float TORSO_MASS = 0.0f; //was 0.3 and before 1.0 and before 0.5
        private const float FOOT_MASS = 0.1f;
        private const float MAXTORQUE_FOOT = 10.0f; //was 10.0
        private const float MAXTORQUE_KNEE = 5.0f; //was 5.0
        private const float MAXTORQUE_HIPMINOR = 5.0f;  // 5.0
        private const float MAXTORQUE_HIPMAJOR = 5.0f; //5.0
        private const float P_CONSTANT = 9.0f; //was 9.0
        private const float D_CONSTANT = 0.0f;
        private const float FOOTFACTOR = 5.0f;

        public readonly HashSet<Body> BodiesOnGround = new HashSet<Body>();
        private readonly float[] current_angles;
        private readonly float[] lo_limit;
        private readonly float[] hi_limit;
        private double[] sensors;
        private readonly float[] desired_angvel;
        private readonly float[] delta_angles;
        private readonly float[] p_terms;
        private readonly float[] d_terms;

        private int step;
        private readonly float[] orig_quat = new float[15];
        private bool resetting;

        private int reset_counter;

        private Vector3 orig_com;
        private readonly float[] orig_left = new float[4];
        private readonly float[] orig_right = new float[4];
        public Vector3 CurrentCom { get; private set; }
        private readonly float[] last_com = new float[4];
        private double last_distance;
        private bool log;

        private readonly List<int> lft;
        private readonly List<float> lfx;
        private readonly List<float> lfy;
        private readonly List<int> rft;
        private readonly List<float> rfx;
        private readonly List<float> rfy;

        private bool leftdown;
        private bool rightdown;
        private bool leftdownback;
        private bool rightdownback;
        private bool leftrigid;
        private bool rightrigid;
        private int lastdown;
        private double old_distance;
        private readonly float[] footdown;

        private int timeCounter; // for clune architecture
        private const int cluneWavelength = 100;

        private readonly Controller _controller;
        private readonly Simulation _simulation;

        private Box _torso;
        private Sphere _leftFoot;
        private Geom _leftLowerLeg;
        private Geom _leftUpperLeg;
        private Fixed _leftFootLegJoint;
        private Hinge _leftKneeLowerLegJoint;
        private Universal _leftHipUpperLegJoint;
        private Sphere _rightFoot;
        private Geom _rightLowerLeg;
        private Geom _rightUpperLeg;
        private Fixed _rightFootLegJoint;
        private Hinge _rightKneeLowerLegJoint;
        private Universal _rightHipUpperLegJoint;
        private Sphere _leftBackFoot;
        private Geom _leftBackLowerLeg;
        private Geom _leftBackUpperLeg;
        private Fixed _leftBackFootLegJoint;
        private Hinge _leftBackKneeLowerLegJoint;
        private Universal _leftBackHipUpperLegJoint;
        private Sphere _rightBackFoot;
        private Geom _rightBackLowerLeg;
        private Geom _rightBackUpperLeg;
        private Fixed _rightBackFootLegJoint;
        private Hinge _rightBackKneeLowerLegJoint;
        private Universal _rightBackHipUpperLegJoint;

        public Quadruped(Controller controller, Simulation simulation)
        {
            _controller = controller;
            _simulation = simulation;

            const int numJoints = 12;
            lo_limit = new float[numJoints];
            hi_limit = new float[numJoints];

            resetting = false;
            leftdown = false;
            rightdown = false;
            reset_counter = 0;
            old_distance = 0;
            last_distance = -1;
            leftdownback = false;
            rightdownback = false;
            leftrigid = false;
            rightrigid = false;
            lastdown = 0;

            if (log) { } // GWM - No log file for now

            // GWM - lines moved out of for loop
            current_angles = new float[numJoints];
            delta_angles = new float[numJoints];
            desired_angvel = new float[numJoints];

            p_terms = new float[numJoints];
            d_terms = new float[numJoints];

            for (int x = 0; x < numJoints; x++)
            {
                p_terms[x] = P_CONSTANT;
                d_terms[x] = D_CONSTANT;
                lo_limit[x] = 0.0f;
                hi_limit[x] = 0.0f;
            }

            sensors = new double[4];
            footdown = new float[4];

            lft = new List<int>();
            lfx = new List<float>();
            lfy = new List<float>();
            rft = new List<int>();
            rfx = new List<float>();
            rfy = new List<float>();
        }

        public void Initialize()
        {
            var xAxis = new Vector3 { X = 1 };
            var nxAxis = new Vector3 { X = -1 };
            var yAxis = new Vector3 { Y = 1 };
            //var zAxis = new Vector3 { Z = 1 };

            const float fr = 0.4f; //was 0.2
            const float mi = 0.0f;

            var torsoPos = new Vector3(0.5f, (TORSO_LEN + ULEG_RAD) / 2.0f, (LEG_SCALE) + (LEG_SCALE) + FOOTZ_SZ);

            float[] leftLegPos = { LEG_POS, -ULEG_RAD / 2, 0.0f };
            float[] rightLegPos = { LEG_POS, TORSO_LEN + ULEG_RAD + ULEG_RAD / 2, 0.0f };
            float[] leftLegBackPos = { 1.0f - LEG_POS, -ULEG_RAD / 2, torsoPos.Z - ((LEG_SCALE) + (LEG_SCALE) + FOOTZ_SZ) }; //[1] was 0
            float[] rightLegBackPos = { 1.0f - LEG_POS, TORSO_LEN + ULEG_RAD + ULEG_RAD / 2, torsoPos.Z - ((LEG_SCALE) + (LEG_SCALE) + FOOTZ_SZ) };

            var leftHip = new Vector3(leftLegPos[0], leftLegPos[1] + ULEG_RAD, torsoPos.Z);
            var rightHip = new Vector3(rightLegPos[0], rightLegPos[1] - ULEG_RAD, torsoPos.Z);
            var leftHipBack = new Vector3(leftLegBackPos[0], leftLegBackPos[1] + ULEG_RAD, torsoPos.Z);
            var rightHipBack = new Vector3(rightLegBackPos[0], rightLegBackPos[1] - ULEG_RAD, torsoPos.Z);

            _torso = AddBoxGeom(TORSO_LEN2, TORSO_LEN, TORSO_HEIGHT, TORSO_MASS, torsoPos);

            CreateLeg(leftLegPos, false, out _leftFoot, out _leftLowerLeg, out _leftUpperLeg, out _leftFootLegJoint, out _leftKneeLowerLegJoint);
            CreateLeg(rightLegPos, false, out _rightFoot, out _rightLowerLeg, out _rightUpperLeg, out _rightFootLegJoint, out _rightKneeLowerLegJoint);

            //was -1.3, 1.6
            _leftHipUpperLegJoint = AddUniversalJoint(_torso.Body, _leftUpperLeg.Body, leftHip, xAxis, yAxis, mi, fr, -0.8f, 0.8f, MAXTORQUE_HIPMINOR, MAXTORQUE_HIPMAJOR);
            _rightHipUpperLegJoint = AddUniversalJoint(_torso.Body, _rightUpperLeg.Body, rightHip, nxAxis, yAxis, mi, fr, -0.8f, 0.8f, MAXTORQUE_HIPMINOR, MAXTORQUE_HIPMAJOR);

            CreateLeg(leftLegBackPos, true, out _leftBackFoot, out _leftBackLowerLeg, out _leftBackUpperLeg, out _leftBackFootLegJoint, out _leftBackKneeLowerLegJoint);
            CreateLeg(rightLegBackPos, true, out _rightBackFoot, out _rightBackLowerLeg, out _rightBackUpperLeg, out _rightBackFootLegJoint, out _rightBackKneeLowerLegJoint);

            //was -0.2, 0.2
            _leftBackHipUpperLegJoint = AddUniversalJoint(_torso.Body, _leftBackUpperLeg.Body, leftHipBack, xAxis, yAxis, mi, fr, -0.8f, 0.8f, MAXTORQUE_HIPMINOR, MAXTORQUE_HIPMAJOR);
            _rightBackHipUpperLegJoint = AddUniversalJoint(_torso.Body, _rightBackUpperLeg.Body, rightHipBack, nxAxis, yAxis, mi, fr, -0.8f, 0.8f, MAXTORQUE_HIPMINOR, MAXTORQUE_HIPMAJOR);

            lo_limit[0] = _leftKneeLowerLegJoint.LimitMotor.LowStop;
            lo_limit[1] = _leftHipUpperLegJoint.LimitMotor1.LowStop;
            lo_limit[2] = _leftHipUpperLegJoint.LimitMotor2.LowStop;
            lo_limit[3] = _rightHipUpperLegJoint.LimitMotor2.LowStop;
            lo_limit[4] = _rightHipUpperLegJoint.LimitMotor1.LowStop;
            lo_limit[5] = _rightKneeLowerLegJoint.LimitMotor.LowStop;

            hi_limit[0] = _leftKneeLowerLegJoint.LimitMotor.HighStop;
            hi_limit[1] = _leftHipUpperLegJoint.LimitMotor1.HighStop;
            hi_limit[2] = _leftHipUpperLegJoint.LimitMotor2.HighStop;
            hi_limit[3] = _rightHipUpperLegJoint.LimitMotor2.HighStop;
            hi_limit[4] = _rightHipUpperLegJoint.LimitMotor1.HighStop;
            hi_limit[5] = _rightKneeLowerLegJoint.LimitMotor.HighStop;

            lo_limit[6] = _leftBackKneeLowerLegJoint.LimitMotor.LowStop;
            lo_limit[7] = _leftBackHipUpperLegJoint.LimitMotor1.LowStop;
            lo_limit[8] = _leftBackHipUpperLegJoint.LimitMotor2.LowStop;
            lo_limit[9] = _rightBackHipUpperLegJoint.LimitMotor2.LowStop;
            lo_limit[10] = _rightBackHipUpperLegJoint.LimitMotor1.LowStop;
            lo_limit[11] = _rightBackKneeLowerLegJoint.LimitMotor.LowStop;

            hi_limit[6] = _leftBackKneeLowerLegJoint.LimitMotor.HighStop; //back knee
            hi_limit[7] = _leftBackHipUpperLegJoint.LimitMotor1.HighStop;
            hi_limit[8] = _leftBackHipUpperLegJoint.LimitMotor2.HighStop;
            hi_limit[9] = _rightBackHipUpperLegJoint.LimitMotor2.HighStop;
            hi_limit[10] = _rightBackHipUpperLegJoint.LimitMotor1.HighStop;
            hi_limit[11] = _rightBackKneeLowerLegJoint.LimitMotor.HighStop; //other back knee

            orig_com = GetCenterOfMass();
            CurrentCom = GetCenterOfMass();
            orig_left[0] = _leftFoot.Position.X;
            orig_right[0] = _leftFoot.Position.Y;
            orig_left[1] = _rightFoot.Position.X;
            orig_right[1] = _rightFoot.Position.Y;
            orig_left[2] = 0.0f;
            orig_right[2] = 0.0f;

            for (int i = 0; i < Bodies.Count; i++)
                orig_quat[i] = Bodies[i].Position.X;
        }

        private void CreateLeg(float[] offset, bool flipped, out Sphere foot, out Geom lowerLeg, out Geom upperLeg, out Fixed footLegJoint, out Hinge kneeLowerLegJoint)
        {
            //var xAxis = new Vector3(1.0f, 0.0f, 0.0f);
            var yAxis = new Vector3(0.0f, -1.0f, 0.0f);
            //var zAxis = new Vector3(0.0f, 0.0f, 1.0f);

            float[] p = { offset[0], offset[1], offset[2] };

            var foot_pos = new Vector3(p[0], p[1], p[2] + (FOOTZ_SZ / 2.0f));
            foot = AddSphereGeom(FOOTZ_SZ / 2.0f, FOOT_MASS, foot_pos);

            //float sc = LEG_SCALE;//1/9 for different sized legs. default: 2.5;
            var lower_pos = new Vector3(p[0], p[1], p[2] + FOOTZ_SZ + (LEG_SCALE) / 2.0f);
            lowerLeg = AddBoxGeom(LLEG_RAD * 1.5f, LLEG_RAD * 1.5f, LEG_SCALE, LEG_MASS, lower_pos, Vector3.UnitZ, PI * 0.5f);
            //lowerLeg = AddCylinderGeom(LLEG_RAD, LEG_SCALE, LEG_MASS, lower_pos, DirectionAxis.Z, Vector3.UnitZ, PI * 0.5f); //was 3
            var upper_pos = new Vector3(p[0], p[1], p[2] + FOOTZ_SZ + (LEG_SCALE) + (LEG_SCALE) / 2.0f);
            upperLeg = AddBoxGeom(ULEG_RAD * 1.5f, ULEG_RAD * 1.5f, LEG_SCALE, LEG_MASS, upper_pos, Vector3.UnitZ, PI * 0.5f);
            //upperLeg = AddCylinderGeom(ULEG_RAD, LEG_SCALE, LEG_MASS, upper_pos, DirectionAxis.Z, Vector3.UnitZ, PI * 0.5f);

            var knee_joint_a = new Vector3(p[0], p[1], p[2] + FOOTZ_SZ + (LEG_SCALE));

            footLegJoint = AddFixedJoint(foot.Body, lowerLeg.Body);

            if (flipped)
                kneeLowerLegJoint = AddHingeJoint(lowerLeg.Body, upperLeg.Body, knee_joint_a, yAxis, 0.0f, 0.8f, MAXTORQUE_KNEE); //was -1.4, 0.8
            else
                kneeLowerLegJoint = AddHingeJoint(lowerLeg.Body, upperLeg.Body, knee_joint_a, yAxis, -0.8f, 0.0f, MAXTORQUE_KNEE); //was -1.4
        }


        public bool Update(float dt)
        {
            if (step == 0)
            {
                last_com[0] = CurrentCom.X;
                last_com[1] = CurrentCom.Y;
            }

            step++;
            var oldAngles = current_angles.ToArray(); // Copy

            //read current angles
            current_angles[0] = _leftKneeLowerLegJoint.Angle; //left knee
            current_angles[1] = _leftHipUpperLegJoint.Angle1; //left outhip	
            current_angles[2] = _leftHipUpperLegJoint.Angle2; //left mainhip

            current_angles[3] = _rightHipUpperLegJoint.Angle2; //right mainhip
            current_angles[4] = _rightHipUpperLegJoint.Angle1; //right outhip
            current_angles[5] = _rightKneeLowerLegJoint.Angle; //right knee
            //----BACK LEGS
            current_angles[6] = _leftBackKneeLowerLegJoint.Angle; //left knee
            current_angles[7] = _leftBackHipUpperLegJoint.Angle1; //left outhip	
            current_angles[8] = _leftBackHipUpperLegJoint.Angle2; //left mainhip

            current_angles[9] = _rightBackHipUpperLegJoint.Angle2; //right mainhip
            current_angles[10] = _rightBackHipUpperLegJoint.Angle1; //right outhip
            current_angles[11] = _rightBackKneeLowerLegJoint.Angle; //right knee

            for (var x = 0; x < current_angles.Length; x++)
                delta_angles[x] = (current_angles[x] - oldAngles[x]) / dt;

            // record behavior
            bool newleftdown = BodiesOnGround.Contains(_leftFoot.Body);
            bool newrightdown = BodiesOnGround.Contains(_rightFoot.Body);
            bool newleftdownback = BodiesOnGround.Contains(_leftBackFoot.Body);
            bool newrightdownback = BodiesOnGround.Contains(_rightBackFoot.Body);

            var quat = _torso.Quaternion;
            var q = new[] { quat.W, quat.X, quat.Y, quat.Z };

            float tanyaw = 2.0f * (q[0] * q[1] + q[3] * q[2]) / (q[3] * q[3] + q[0] * q[0] - q[1] * q[1] - q[2] * q[2]);
            float sinpitch = -2.0f * (q[0] * q[2] - q[3] * q[1]);
            float tanroll = 2.0f * (q[3] * q[0] + q[1] * q[2]) / (q[3] * q[3] - q[0] * q[0] - q[1] * q[1] + q[2] * q[2]);
            float yaw = (float)Math.Atan(tanyaw);
            float pitch = (float)Math.Asin(sinpitch);
            float roll = (float)Math.Atan(tanroll);

            var triggers = new float[4];

            if (newleftdown && footdown[0] == 0)
            {
                triggers[0] = 1;
                footdown[0] = 1;
            }

            if (newrightdown && footdown[1] == 0)
            {
                triggers[1] = 1;
                footdown[1] = 1;
            }

            if (newleftdownback && footdown[2] == 0)
            {
                triggers[2] = 1;
                footdown[2] = 1;
            }

            if (newrightdownback && footdown[3] == 0)
            {
                triggers[3] = 1;
                footdown[3] = 1;
            }


            // foot sensors
            if (newleftdown)
            {
                footdown[0] = 1;
                sensors[0] = 1;
            }
            else
            {
                footdown[0] = 0;
                sensors[0] = 0;
            }

            if (newrightdown)
            {
                footdown[1] = 1;
                sensors[1] = 1;
            }
            else
            {
                sensors[1] = 0;
                footdown[1] = 0;
            }

            if (newleftdownback)
            {
                sensors[2] = 1;
                footdown[2] = 1;
            }
            else
            {
                sensors[2] = 0;
                footdown[2] = 0;
            }

            if (newrightdownback)
            {
                footdown[3] = 1;
                sensors[3] = 1;
            }
            else
            {
                sensors[3] = 0;
                footdown[3] = 0;
            }

            /*
            //Hip sensors
            sensors[0] = current_angles[2]; //left hip
            sensors[1] = current_angles[3]; //right hip
            sensors[2] = current_angles[8]; //left hip back
            sensors[3] = current_angles[9]; //right hip back
            */

            // CRS
            if (MainProgram.doClune)
            {
                sensors = new double[20];

                sensors[0] = current_angles[2];
                sensors[1] = current_angles[1];
                sensors[2] = current_angles[0];
                sensors[3] = footdown[0];
                sensors[4] = pitch;

                sensors[5] = current_angles[3];
                sensors[6] = current_angles[4];
                sensors[7] = current_angles[5];
                sensors[8] = footdown[1];
                sensors[9] = roll;

                sensors[10] = current_angles[8];
                sensors[11] = current_angles[7];
                sensors[12] = current_angles[6];
                sensors[13] = footdown[2];
                sensors[14] = yaw;

                sensors[15] = current_angles[9];
                sensors[16] = current_angles[10];
                sensors[17] = current_angles[11];
                sensors[18] = footdown[3];
                sensors[19] = (float)Math.Sin(2 * Math.PI * timeCounter / cluneWavelength);
            }

            _controller.update(sensors, triggers);
            var outs = _controller.getOutputs();
            //Console.WriteLine(outs[9] + " - " + sensors[3] + "," + sensors[8] + "," + sensors[13] + "," + sensors[18]);
            //Console.WriteLine(sensors[19]);
            if (log) { } // no log implemented at this time

            var desired_angles = new float[current_angles.Length];

            for (int x = 0; x < current_angles.Length; x++)
            {
                desired_angles[x] = outs[x];

                if (desired_angles[x] < -1.0)
                    desired_angles[x] = -1.0f;

                if (desired_angles[x] > 1)
                    desired_angles[x] = 1.0f;

                if (desired_angles[x] != desired_angles[x])
                {
                    Console.WriteLine("NOT A NUMBER " + desired_angles[x] + "\n");
                    desired_angles[x] = 0;
                }

                if (_controller.Scale)
                {
                    if (desired_angles[x] > 1.0) desired_angles[x] = 1.0f;
                    if (desired_angles[x] < 0.0) desired_angles[x] = 0.0f;
                    desired_angles[x] = lo_limit[x] + (hi_limit[x] - lo_limit[x]) * desired_angles[x];

                }

            }

            for (int x = 0; x < current_angles.Length; x++)
            {
                float delta = desired_angles[x] - current_angles[x];
                float p_term = p_terms[x] * delta;
                float d_term = (-d_terms[x] * delta_angles[x]);
                desired_angvel[x] = p_term + d_term;
                if (log) { } // no log implemented
            }



            _leftKneeLowerLegJoint.LimitMotor.Velocity = desired_angvel[0]; //left knee
            _leftHipUpperLegJoint.LimitMotor1.Velocity = desired_angvel[1]; //left hipout
            _leftHipUpperLegJoint.LimitMotor2.Velocity = desired_angvel[2]; //left hipmain

            _rightHipUpperLegJoint.LimitMotor2.Velocity = desired_angvel[3]; //right hipmain
            _rightHipUpperLegJoint.LimitMotor1.Velocity = desired_angvel[4]; //right hipout
            _rightKneeLowerLegJoint.LimitMotor.Velocity = desired_angvel[5]; //right knee

            //BACK LEGS
            _leftBackKneeLowerLegJoint.LimitMotor.Velocity = desired_angvel[6]; //left knee
            _leftBackHipUpperLegJoint.LimitMotor1.Velocity = desired_angvel[7]; //left hipout
            _leftBackHipUpperLegJoint.LimitMotor2.Velocity = desired_angvel[8]; //left hipmain

            _rightBackHipUpperLegJoint.LimitMotor2.Velocity = desired_angvel[9]; //right hipmain
            _rightBackHipUpperLegJoint.LimitMotor1.Velocity = desired_angvel[10]; //right hipout
            _rightBackKneeLowerLegJoint.LimitMotor.Velocity = desired_angvel[11]; //right knee

            CurrentCom = GetCenterOfMass();

            if (!leftdown && newleftdown)
            {
                if (lft.Count == 0 || (step - lft[lft.Count - 1] > 100 && lastdown != 1))
                {
                    lft.Add(step);
                    lfx.Add(CurrentCom.X);
                    lfy.Add(CurrentCom.Y);

                    if (0 % 2 == 1) // GWM - removed novelty_function variable
                        lastdown = 1;
                    else
                        lastdown = 0;  //if this is set to 0, we don't care if feet sequence alternates
                }
            }
            if (!rightdown && newrightdown)
            {
                if (rft.Count == 0 || (step - rft[rft.Count - 1] > 100 && lastdown != -1))
                {
                    rft.Add(step);
                    rfx.Add(CurrentCom.X);
                    rfy.Add(CurrentCom.Y);
                    if (0 % 2 == 1) // GWM - removed novelty_function variable
                        lastdown = (-1);
                    else
                        lastdown = 0;  //if this is set to 0, we don't care if feet sequence alternates
                }
            }

            //don't let first recorded instance of both feet down set the lastdown criteria
            if (step == 1)
                lastdown = 0;

            leftdown = newleftdown;
            rightdown = newrightdown;
            rightdownback = newrightdownback;
            leftdownback = newleftdownback;
            //reset ground sensors for feetz
            BodiesOnGround.Clear();

            timeCounter++; // for clune architecture, counter used for sin wave
            return base.PostStep(dt) && Continue();
        }

        private bool Continue()
        {
            var torsoPos = _torso.Position;
            float orig_height = (TORSO_RAD / 2.0f + LEG_SCALE + LEG_SCALE + FOOTZ_SZ);
            return torsoPos.Z >= 0.5 * orig_height;
        }

        public float CalcFitness()
        {
            var newCom = GetCenterOfMass();
            var dist = newCom - orig_com;
            dist = dist * dist;
            var fitness = dist.X + dist.Y;
            return (float)Math.Sqrt(fitness);
        }
    }
}
#endif