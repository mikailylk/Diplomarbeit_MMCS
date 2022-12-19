using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ICU_App.Calc
{
    public class AngleCalc
    {
        #region c
        public AngleCalc()
        {

        }
        #endregion

        /// <summary>
        /// returns Vector3 X,Y,Z representing different angles in Euler
        /// </summary>
        /// <param name="q">Quaternion</param>
        /// <returns></returns>
        public Vector3 ToEulerAngles(Quaternion q)
        {
            Vector3 angles = new();

            //// roll / x
            //double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            //double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            //angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            //// pitch / y
            //double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            ////if (Math.Abs(sinp) >= 1)
            ////{
            ////    angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            ////}
            ////else
            ////{
            ////    angles.Y = (float)Math.Asin(sinp);
            ////}
            //angles.Y = (float)Math.Asin(sinp);

            //// yaw / z
            //double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            //double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            //angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            double yaw, pitch, roll;

            double test = q.X * q.Y + q.Z * q.W;
            if (test > 0.499)
            {
                yaw = 2 * Math.Atan2(q.X, q.W);
                pitch = Math.PI / 2;
                roll = 0;
            }
            else if (test < -0.499)
            {
                yaw = -2 * Math.Atan2(q.X, q.W);
                pitch = -Math.PI / 2;
                roll = 0;
            }
            else
            {
                double sqx = q.X * q.X;
                double sqy = q.Y * q.Y;
                double sqz = q.Z * q.Z;
                yaw = Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, 1 - 2 * sqy - 2 * sqz);
                pitch = Math.Asin(2 * test);
                roll = Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, 1 - 2 * sqx - 2 * sqz);
            }

            double yawDegrees = yaw * 180 / Math.PI;
            double pitchDegrees = pitch * 180 / Math.PI;
            double rollDegrees = roll * 180 / Math.PI;

            yawDegrees = yawDegrees % 360;
            if (yawDegrees < 0) yawDegrees += 360;

            pitchDegrees = pitchDegrees % 360;
            if (pitchDegrees < 0) pitchDegrees += 360;

            rollDegrees = rollDegrees % 360;
            if (rollDegrees < 0) rollDegrees += 360;


            angles.X = (float)rollDegrees;
            angles.Y = (float)pitchDegrees;
            angles.Z = (float)yawDegrees;

            return angles;
        }
    }
}
