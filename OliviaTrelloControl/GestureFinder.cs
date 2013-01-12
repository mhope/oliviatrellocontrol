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

        public Gesture GetGesture(Skeleton skeleton)
        {
            SkeletonPoint rightElbow;
            SkeletonPoint rightHand;
            SkeletonPoint rightShoulder;
            GetArmFrom(skeleton, out rightElbow, out rightHand, out rightShoulder);

            // not interacting if hand not raise

            if (rightShoulder.Z - rightHand.Z < 0.2)
            {
                if (IsArmExtended(skeleton)) {
                    hasBeenExtended = true;
                    return Gesture.None;
                } else if (IsArmRetracted(skeleton)){ 
                    hasBeenExtended = false;
                    if (!mousePressed)
                    {
                        mousePressed = true;
                        return Gesture.PickUp;
                    }
                    else
                    {
                        mousePressed = false;
                        return Gesture.PutDown;
                    }
                }
            }

            if (rightHand.Y - rightElbow.Y > 0.2)
            {
                return Gesture.HandUp;
            }
            if (rightElbow.Y - rightHand.Y > 0.2)
            {
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
            return  DistanceFromStraightArm(skeleton)  < 0.01;
        }


        private static bool IsArmRetracted(Skeleton skeleton)
        {
            return DistanceFromStraightArm(skeleton) > 0.1;
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
