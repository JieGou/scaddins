﻿namespace SCaddins.HatchEditor
{
    public static class ConversionUtilities
    {
        public static double ToDeg(this double arg)
        {
            return System.Math.Round(arg * 180 / System.Math.PI, 8);
        }

        public static double ToMM(this double arg)
        {
            return System.Math.Round(arg * 304.8, 8);
        }
    }
}
