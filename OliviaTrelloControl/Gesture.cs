using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OliviaTrelloControl.Mouse;

namespace OliviaTrelloControl.Gestures
{
    public enum Gesture
    {
        HandUp,
        HandDown,
        None,
        NotInteracting,
        PickUp,
        PutDown
    }

    public static class GestureExtensions
    {
        private static int CARD_HEIGHT = 30;
        public static void MouseAction(this Gesture gesture)
        {
            switch (gesture)
            {
                case Gesture.PickUp:
                    MouseInterop.LeftDown();
                    System.Threading.Thread.Sleep(20);
                    MouseInterop.Move(0, 3);
                    System.Threading.Thread.Sleep(20);
                    MouseInterop.Move(0, 3);
                    System.Threading.Thread.Sleep(20);
                    MouseInterop.Move(0, 3);
                    System.Threading.Thread.Sleep(20);
                    MouseInterop.Move(0, 3);
                    break;
                case Gesture.PutDown:
                    MouseInterop.LeftUp();
                    break;
                case Gesture.HandUp:
                    MouseInterop.Move(0, 0 - CARD_HEIGHT);
                    break;
                case Gesture.HandDown:
                    MouseInterop.Move(0, CARD_HEIGHT);
                    break;
                default:
                    break;
            }
        }
    }
}
