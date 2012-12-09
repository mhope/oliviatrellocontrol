using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OliviaTrelloControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        System.Timers.Timer timer;
        KinectSensor kinectSensor;
        private double TIMER_DELAY = 800;
        private int CARD_HEIGHT = 62;
        private bool isClicked = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTrello();
            InitializeKinect();
            InitializeTimer();
        }


        private void _poll_Frame(object sender, ElapsedEventArgs e)
        {
            Debug.WriteLine(DateTime.Now + ": timer elapsed");
            Skeleton skeleton  = FindSkeleton();
            if (skeleton != null)
            {
                ProcessSkeleton(skeleton);
            }
        }

        private Skeleton FindSkeleton()
        {
            var skeletonData = kinectSensor.SkeletonStream.OpenNextFrame(200);
            if (skeletonData != null)
            {
                Skeleton[] skeletons = new Skeleton[skeletonData.SkeletonArrayLength];
                skeletonData.CopySkeletonDataTo(skeletons);
                return skeletons.First();
            }
            return null;
        }

        private void ProcessSkeleton(Skeleton skeleton)
        {
            Gesture gesture = GetGesture(skeleton);
            ProcessGesture(gesture);
            Debug.WriteLine(DateTime.Now + ": Processing skeleton");
        }

        private void ProcessGesture(Gesture gesture)
        {
            Debug.WriteLine(DateTime.Now + ": gesture "+gesture);
            switch (gesture)
            {
                case (Gesture.HandDown):
                    SelectLowerElement();
                    break;
                case (Gesture.HandUp):
                    SelectHigherElement();
                    break;
                default: break;
            }
        }

        private void SelectHigherElement()
        {
            SendKeys.SendWait("{UP}");
            MouseCursorHigher();
        }

        private void SelectLowerElement()
        {
            SendKeys.SendWait("{DOWN}");
            MouseCursorLower();

        }

        private void MouseCursorLower()
        {
            MouseCursorMove(CARD_HEIGHT);
        }


        private void MouseCursorHigher()
        {
            MouseCursorMove(0-CARD_HEIGHT);
        }

        private void MouseCursorMove(int translateY)
        {
            System.Windows.Forms.Cursor.Position =
                new System.Drawing.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y + translateY);
        }

        private Gesture GetGesture(Skeleton skeleton)
        {
            SkeletonPoint rightElbow = skeleton.Joints.Where(j => j.JointType == JointType.ElbowRight).First().Position;
            SkeletonPoint rightHand = skeleton.Joints.Where(j => j.JointType == JointType.HandRight).First().Position;
            SkeletonPoint rightShoulder  = skeleton.Joints.Where(j => j.JointType == JointType.ShoulderRight).First().Position;

            // not interacting if hand not raised
            double handToShoulderZ = rightShoulder.Z - rightHand.Z;
            isClicked = isArmExtended(skeleton);
            Debug.WriteLine(DateTime.Now + ": isClicked = " + isClicked);

            if (handToShoulderZ < 0.2)
            {
                Debug.WriteLine(DateTime.Now + ": right shouler z = " + rightShoulder.Z + ", right hand z = " + rightHand.Z);
                return Gesture.NotInteracting;
            }

            if (rightHand.Y - rightElbow.Y > 0.1) {
                return Gesture.HandUp;
            }
            if (rightElbow.Y - rightHand.Y > 0.1)
            {
                return Gesture.HandDown;
            }
            return Gesture.None;

        }

        private void InitializeKinect()
        {
            foreach (KinectSensor kinect in KinectSensor.KinectSensors)
            {
                if (kinect.Status == KinectStatus.Connected)
                {
                    kinectSensor = kinect;
                    kinectSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    kinectSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters
                                                   {
                                                 Smoothing = 0.5f,
                                                 Correction = 0.5f,
                                                 Prediction = 0.5f,
                                                 JitterRadius = 0.05f,
                                                 MaxDeviationRadius = 0.04f
                                             });
                    kinectSensor.Start();
                    break;
                }
            }
        }


        private void InitializeTrello()
        {
            BringWindowToTop("chrome");
            PositionCursor();
        }

        private void PositionCursor()
        {
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(200, 250);
        }

        private void InitializeTimer()
        {
            timer = new System.Timers.Timer(TIMER_DELAY);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(_poll_Frame);
            timer.Start();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        
        private void BringWindowToTop(string processName)
        {

            Process[] processes = Process.GetProcessesByName(processName);
            foreach (Process p in processes)
            {
                SetForegroundWindow(p.MainWindowHandle);
            }
        }
        
        private void Clean(object sender, RoutedEventArgs e)
        {
            if (kinectSensor != null)
            {
                kinectSensor.Stop();
                kinectSensor = null;

            }

        }

        private static bool isArmExtended(Skeleton skeleton)
        {
            SkeletonPoint rightElbow = skeleton.Joints.Where(j => j.JointType == JointType.ElbowRight).First().Position;
            SkeletonPoint rightShoulder = skeleton.Joints.Where(j => j.JointType == JointType.ShoulderRight).First().Position;
            SkeletonPoint rightWrist = skeleton.Joints.Where(j => j.JointType == JointType.WristRight).First().Position;
            bool handRaised = rightShoulder.Z - rightWrist.Z < 0.5;
            double distanceWristShoulder = Distance3D(rightWrist, rightShoulder);
            double distanceWristElbow = Distance3D(rightWrist, rightElbow);
            double distanceElbowShoulder = Distance3D(rightElbow, rightShoulder);

            double sumLessDistance = distanceWristElbow + distanceElbowShoulder - distanceWristShoulder;
            bool armStraight = sumLessDistance < 0.01;
            if (armStraight)
            {
                //Debug.WriteLine("Arm is straight, distance  " + sumLessDistance + ". Hand " + (handRaised ? "is" : "is not") + " raised");
                //Debug.WriteLine("S <-> W: " + distanceWristShoulder); ;
                //Debug.WriteLine("S <-> E: " + distanceWristElbow); ;
                //Debug.WriteLine("E <-> W: " + distanceElbowShoulder);
                //Debug.WriteLine("right shoulder z :"+ rightShoulder.Z + ", right wrist z: " + rightWrist.Z + ", difference:" + (rightShoulder.Z - rightWrist.Z));
            }

            return handRaised && armStraight;
        }

        //From http://social.msdn.microsoft.com/Forums/en-US/kinectsdknuiapi/thread/899d8e03-6a8a-4cfc-a5e0-6a0268b4a29c
        private static double Distance3D(SkeletonPoint position1, SkeletonPoint position2)
        {
            //     __________________________________
            //d = &#8730; (x2-x1)^2 + (y2-y1)^2 + (z2-z1)^2
            //

            //Our end result
            double result = 0;
            //Take x2-x1, then square it
            double part1 = Math.Pow((position2.X - position1.X), 2);
            //Take y2-y1, then sqaure it
            double part2 = Math.Pow((position2.Y - position1.Y), 2);
            //Take z2-z1, then square it
            double part3 = Math.Pow((position2.Z - position1.Z), 2);
            //Add both of the parts together
            double underRadical = part1 + part2 + part3;
            //Get the square root of the parts
            result = Math.Sqrt(underRadical);
            //Return our result
            return result;
        }

    }
}
