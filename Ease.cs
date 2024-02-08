using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APurpleApple
{
    public static class Ease
    {
        public static double InSin(double x)
        {
            return 1 - Math.Cos(x * Math.PI / 2);
        }

        public static double OutSin(double x)
        {
            return Math.Sin(x * Math.PI / 2);
        }

        public static double InElastic(double x)
        {
            return x == 0 ? 0 : x == 1 ? 1 : -Math.Pow(2, 10 * x - 10) * Math.Sin((x * 10 - 10.75) * (2 * Math.PI / 3));
        }
    }
}
