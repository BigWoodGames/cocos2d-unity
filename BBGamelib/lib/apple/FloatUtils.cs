using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class FloatUtils
	{
		public static float Epsilon =  0.000000001f;
        public static bool ES(double a, double b){
			bool equal = EQ (a, b);
			if (equal)
				return true;
			else
				return a < b;
		}
        public static bool EB(double a, double b){
			bool equal = EQ (a, b);
			if (equal)
				return true;
			else
				return a > b;
		}
        public static bool EQ(double a, double b){
			bool equal = System.Math.Abs (b - a) < Epsilon;
			return equal;
		}

        public static bool NEQ(double a, double b){
			return !EQ(a, b);
		}
		
        public static bool Big(double a, double b){
			bool equal = EQ (a, b);
			if (equal)
				return false;
			else
				return a > b;
		}
        public static bool Small(double a, double b){
			bool equal = EQ (a, b);
			if (equal)
				return false;
			else
				return a < b;
		}
		
		public static float Round(float a, int n){
			int scale = 10 ^ n;
			float k = Mathf.Round(a * scale);
			k = k / scale;
			return k;
		}
	}
}

