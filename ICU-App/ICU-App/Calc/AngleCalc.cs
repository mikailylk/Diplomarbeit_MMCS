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
        public AngleCalc(){}

        /// <summary>
        /// Returns Vector3 X,Y,Z representing different angles in Euler
        /// </summary>
        /// <param name="q">Quaternion</param>
        /// <returns>Vector3 Euler angles</returns>
        public Vector3 ToEulerAngles(Quaternion q)
        {
            // Z-Y-X convention (Tayt-Brian)
            Vector3 angles = new();

            // roll / x
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch / y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }
            //angles.Y = (float)Math.Asin(sinp);

            // yaw / z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        /// <summary>
        /// Enumeration of possible rotation sequence types
        /// </summary>
        public enum RotSeq
        {
            ZYX, ZYZ, ZXY, ZXZ, YXZ, YXY, YZX, YZY, XYZ, XYX, XZY, XZX
        };

        /// <summary>
        /// Calculates the Euler angles using a two-axis rotation sequence
        /// </summary>
        /// <param name="r11"></param>
        /// <param name="r12"></param>
        /// <param name="r21"></param>
        /// <param name="r31"></param>
        /// <param name="r32"></param>
        /// <returns>Vector3 angles</returns>
        Vector3 twoaxisrot(float r11, float r12, float r21, float r31, float r32)
        {
            Vector3 ret = new Vector3
            {
                X = (float)(Math.Atan2(r11, r12) * (180 / Math.PI)),
                Y = (float)(Math.Acos(r21) * (180 / Math.PI)),
                Z = (float)(Math.Atan2(r31, r32) * (180 / Math.PI))
            };
            return ret;
        }

        /// <summary>
        /// Calculates the Euler angles using a three-axis rotation sequence
        /// </summary>
        /// <param name="r11"></param>
        /// <param name="r12"></param>
        /// <param name="r21"></param>
        /// <param name="r31"></param>
        /// <param name="r32"></param>
        /// <returns>Vector3 angles</returns>
        Vector3 threeaxisrot(float r11, float r12, float r21, float r31, float r32)
        {
            Vector3 ret = new Vector3
            {
                X = (float)(Math.Atan2(r31, r32) * (180 / Math.PI)),
                Y = (float)(Math.Asin(r21) * (180 / Math.PI)),
                Z = (float)(Math.Atan2(r11, r12) * (180 / Math.PI))
            };
            return ret;
        }

        /// <summary>
        /// Calculates the three Euler angles (in degrees) corresponding to a 
        /// given quaternion rotation and the given rotation order.
        /// </summary>
        /// <param name="q">The quaternion to convert to Euler angles.</param>
        /// <param name="rotSeq">The rotation sequence order.</param>
        /// <returns>The Euler angles as a Vector3, depending on the specified 
        /// rotation order.</returns>
        public Vector3 quaternion2Euler(Quaternion q, RotSeq rotSeq)
        {
            switch (rotSeq)
            {
                case RotSeq.ZYX:
                    return threeaxisrot(2 * (q.X * q.Y + q.W * q.Z),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z,
                        -2 * (q.X * q.Z - q.W * q.Y),
                        2 * (q.Y * q.Z + q.W * q.X),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);


                case RotSeq.ZYZ:
                    return twoaxisrot(2 * (q.Y * q.Z - q.W * q.X),
                        2 * (q.X * q.Z + q.W * q.Y),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z,
                        2 * (q.Y * q.Z + q.W * q.X),
                        -2 * (q.X * q.Z - q.W * q.Y));


                case RotSeq.ZXY:
                    return threeaxisrot(-2 * (q.X * q.Y - q.W * q.Z),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z,
                        2 * (q.Y * q.Z + q.W * q.X),
                        -2 * (q.X * q.Z - q.W * q.Y),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);


                case RotSeq.ZXZ:
                    return twoaxisrot(2 * (q.X * q.Z + q.W * q.Y),
                        -2 * (q.Y * q.Z - q.W * q.X),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z,
                        2 * (q.X * q.Z - q.W * q.Y),
                        2 * (q.Y * q.Z + q.W * q.X));


                case RotSeq.YXZ:
                    return threeaxisrot(2 * (q.X * q.Z + q.W * q.Y),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z,
                        -2 * (q.Y * q.Z - q.W * q.X),
                        2 * (q.X * q.Y + q.W * q.Z),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z);

                case RotSeq.YXY:
                    return twoaxisrot(2 * (q.X * q.Y - q.W * q.Z),
                        2 * (q.Y * q.Z + q.W * q.X),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z,
                        2 * (q.X * q.Y + q.W * q.Z),
                        -2 * (q.Y * q.Z - q.W * q.X));


                case RotSeq.YZX:
                    return threeaxisrot(-2 * (q.X * q.Z - q.W * q.Y),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z,
                        2 * (q.X * q.Y + q.W * q.Z),
                        -2 * (q.Y * q.Z - q.W * q.X),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z);


                case RotSeq.YZY:
                    return twoaxisrot(2 * (q.Y * q.Z + q.W * q.X),
                        -2 * (q.X * q.Y - q.W * q.Z),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z,
                        2 * (q.Y * q.Z - q.W * q.X),
                        2 * (q.X * q.Y + q.W * q.Z));


                case RotSeq.XYZ:
                    return threeaxisrot(-2 * (q.Y * q.Z - q.W * q.X),
                        q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z,
                        2 * (q.X * q.Z + q.W * q.Y),
                        -2 * (q.X * q.Y - q.W * q.Z),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);


                case RotSeq.XYX:
                    return twoaxisrot(2 * (q.X * q.Y + q.W * q.Z),
                        -2 * (q.X * q.Z - q.W * q.Y),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z,
                        2 * (q.X * q.Y - q.W * q.Z),
                        2 * (q.X * q.Z + q.W * q.Y));


                case RotSeq.XZY:
                    return threeaxisrot(2 * (q.Y * q.Z + q.W * q.X),
                        q.W * q.W - q.X * q.X + q.Y * q.Y - q.Z * q.Z,
                        -2 * (q.X * q.Y - q.W * q.Z),
                        2 * (q.X * q.Z + q.W * q.Y),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);


                case RotSeq.XZX:
                    return twoaxisrot(2 * (q.X * q.Z - q.W * q.Y),
                        2 * (q.X * q.Y + q.W * q.Z),
                        q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z,
                        2 * (q.X * q.Z + q.W * q.Y),
                        -2 * (q.X * q.Y - q.W * q.Z));

                default:
                    return Vector3.Zero;

            }

        }
    }
}
