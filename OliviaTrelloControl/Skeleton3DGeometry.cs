using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OliviaTrelloControl
{
    public static class Skeleton3DGeometry
    {
        //From http://social.msdn.microsoft.com/Forums/en-US/kinectsdknuiapi/thread/899d8e03-6a8a-4cfc-a5e0-6a0268b4a29c
        public static double Distance3D(SkeletonPoint position1, SkeletonPoint position2)
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
