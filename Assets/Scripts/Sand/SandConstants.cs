using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SandConstants {

    public enum SandType
    {
        Sand,
        Water,
        Gas,
        Solid,
    }
    public enum SandFallDirection
    {
        Down,
        DownLeft,
        DownRight,
        Left,
        Right,
        Up,
        UpLeft,
        UpRight,
        None,
    }
    public static class SandFallDirectionExtensions
    {

        public static int GetFallDirectionX(this SandFallDirection dir)
        {
            return dir switch
            {
                SandFallDirection.Left or SandFallDirection.DownLeft or SandFallDirection.UpLeft => -1,
                SandFallDirection.Right or SandFallDirection.DownRight or SandFallDirection.UpRight => 1,
                _ => 0,
            };
        }

        public static int GetFallDirectionY(this SandFallDirection dir)
        {
            return dir switch
            {
                SandFallDirection.Up or SandFallDirection.UpLeft or SandFallDirection.UpRight => 1,
                SandFallDirection.Down or SandFallDirection.DownLeft or SandFallDirection.DownRight => -1,
                _ => 0,
            };
        }
    }
}

