using System;

namespace src
{
    public static class FigureCalculator
    {
        private const int аccuracy = 6;

        public static double FindAreaOfCircle(double radius)
        {
            if (radius < 0) throw new ArgumentException("radius is wrong");

            return Math.Round(Math.PI * radius * radius, аccuracy);
        }

        public static double FindAreaOfTriangle(double a, double b, double c)
        {
            if (a <= 0 || b <= 0 || c <= 0) throw new ArgumentException("one or more of the sides are incorrect");
            if (a + b < c || a + c < b || b + c < a) throw new ArgumentException("one or more of the sides are incorrect");

            double p = (a + b + c) / 2;
            return Math.Round(Math.Sqrt((p * (p - a) * (p - b) * (p - c))), аccuracy);
        }

        public static bool isRectangular(double a, double b, double c)
        {
            return (a * a + b * b == c * c) || (a * a + c * c == b * b) || (c * c + b * b == a * a);
        }
    }
}
