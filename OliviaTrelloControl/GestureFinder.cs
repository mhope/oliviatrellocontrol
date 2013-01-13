using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliviaTrelloControl.Gestures
{
    public class GestureFinder
    {
        private bool hasBeenExtended = false;
        private  bool mousePressed = false;
        private int gestureCount = 0;

        public Gesture GetGesture(Skeleton skeleton)
        {
            SkeletonPoint rightElbow;
            SkeletonPoint rightHand;
            SkeletonPoint rightShoulder;
            GetArmFrom(skeleton, out rightElbow, out rightHand, out rightShoulder);

            System.Diagnostics.Debug.WriteLine("#Processing gesture" + ++gestureCount);
            System.Diagnostics.Debug.WriteLine(" - Shoulder: Y=" + Math.Round(rightShoulder.Y, 2) + ", Z=" + Math.Round(rightShoulder.Z, 2));
            System.Diagnostics.Debug.WriteLine(" - Elbow: Y=" + Math.Round(rightElbow.Y, 2) + ", Z=" + Math.Round(rightElbow.Z, 2));
            System.Diagnostics.Debug.WriteLine(" - Hand: Y=" + Math.Round(rightHand.Y, 2) + ", Z=" + Math.Round(rightHand.Z, 2));

            System.Diagnostics.Debug.Write("# S<->H: " + Math.Round(rightShoulder.Y - rightHand.Y, 2));
            System.Diagnostics.Debug.Write(", H<->E: " + Math.Round(rightHand.Y - rightElbow.Y, 2));
            System.Diagnostics.Debug.Write(", E<->H: " + Math.Round(rightElbow.Y - rightHand.Y, 2));
            System.Diagnostics.Debug.Write("\n");

            // not interacting if hand not raise
            if ((rightHand.Y == 0 && rightHand.Z == 0) || rightShoulder.Y - rightHand.Y > 0.5)
            {
                return Gesture.NotInteracting;
            }

            System.Diagnostics.Debug.Print("\t\tInteracting");
            if (IsArmExtended(skeleton))
            {
                System.Diagnostics.Debug.Print("\t\t\t\tExtended");
                hasBeenExtended = true;
                return Gesture.None;
            }
            else if (IsArmRetracted(skeleton))
            {
                if (hasBeenExtended)
                {
                    hasBeenExtended = false;
                    if (!mousePressed)
                    {
                        System.Diagnostics.Debug.Print("\t\t\t\tRetracted - picking up");
                        mousePressed = true;
                        return Gesture.PickUp;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Print("\t\t\t\tRetracted - putting down");
                        mousePressed = false;
                        return Gesture.PutDown;
                    }
                }
                else
                {
                    return FindMovementGesture(ref rightElbow, ref rightHand);
                }
            }
            else
            {
                return FindMovementGesture(ref rightElbow, ref rightHand);
            }
        }

        private static Gesture FindMovementGesture(ref SkeletonPoint rightElbow, ref SkeletonPoint rightHand)
        {
            if (rightHand.Y - rightElbow.Y > 0.1)
            {
                System.Diagnostics.Debug.Print("\t\t\tHand Up");
                return Gesture.HandUp;
            }
            if (rightElbow.Y - rightHand.Y > 0.1)
            {
                System.Diagnostics.Debug.Print("\t\t\tHand Down");
                return Gesture.HandDown;
            }
            return Gesture.None;
        }

        private static void GetArmFrom(Skeleton skeleton, out SkeletonPoint rightElbow, out SkeletonPoint rightHand, out SkeletonPoint rightShoulder)
        {
            rightElbow = skeleton.Joints.Where(j => j.JointType == JointType.ElbowRight).First().Position;
            rightHand = skeleton.Joints.Where(j => j.JointType == JointType.HandRight).First().Position;
            rightShoulder = skeleton.Joints.Where(j => j.JointType == JointType.ShoulderRight).First().Position;
        }

        private static bool IsArmExtended(Skeleton skeleton)
        {
            //double distance =  DistanceFromStraightArm(skeleton);
            //System.Diagnostics.Debug.WriteLine("$e" + Math.Round(distance, 2));
            //return distance < 0.05;
            SkeletonPoint rightElbow;
            SkeletonPoint rightShoulder;
            SkeletonPoint rightHand;
            GetArmFrom(skeleton, out rightElbow, out rightHand, out rightShoulder);


            return rightHand.Z < 0.9;
        }


        private static bool IsArmRetracted(Skeleton skeleton)
        {
            //double distance = DistanceFromStraightArm(skeleton);
            //System.Diagnostics.Debug.WriteLine("$r" + Math.Round(distance, 2));
            //return distance > 0.1;
            SkeletonPoint rightElbow;
            SkeletonPoint rightShoulder;
            SkeletonPoint rightHand;
            GetArmFrom(skeleton, out rightElbow, out rightHand, out rightShoulder);


            return rightHand.Z > 1.0;
        }
        

        private static double DistanceFromStraightArm(Skeleton skeleton)
        {
            SkeletonPoint rightElbow;
            SkeletonPoint rightShoulder;
            SkeletonPoint rightHand;
            GetArmFrom(skeleton, out rightElbow, out rightHand, out rightShoulder);

            double distancehandShoulder = Skeleton3DGeometry.Distance3D(rightHand, rightShoulder);
            double distanceHandElbow = Skeleton3DGeometry.Distance3D(rightHand, rightElbow);
            double distanceElbowShoulder = Skeleton3DGeometry.Distance3D(rightElbow, rightShoulder);
                                
            return distanceHandElbow + distanceElbowShoulder - distancehandShoulder;

        }
    }
}
