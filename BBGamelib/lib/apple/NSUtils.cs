using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace BBGamelib{
	public class NSUtils
	{
		public static void Assert(bool check, string format, params object[] args){
			if (!check) {
				throw new UnityException (string.Format (format, args));
			}
		}

		public static bool hasMethod(object objectToCheck, string methodName)
		{
			Type type = objectToCheck.GetType();
			MethodInfo[] ms = type.GetMethods();
			for (int i=0; i<ms.Length; i++) {
				MethodInfo m = ms[i];
				if(m.Name == methodName){
					return true;
				}
			}
			return false;
		} 
	}
}
