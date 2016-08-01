using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class FloatUtils
	{
		public static float Epsilon =  0.000000001f;
		public static bool ES(float a, float b){
			bool equal = EQ (a, b);
			if (equal)
				return true;
			else
				return a < b;
		}
		public static bool EB(float a, float b){
			bool equal = EQ (a, b);
			if (equal)
				return true;
			else
				return a > b;
		}
		public static bool EQ(float a, float b){
			bool equal = Mathf.Abs (b - a) < Epsilon;
			return equal;
		}

		public static bool NEQ(float a, float b){
			return !EQ(a, b);
		}
		
		public static bool Big(float a, float b){
			bool equal = EQ (a, b);
			if (equal)
				return false;
			else
				return a > b;
		}
		public static bool Small(float a, float b){
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

