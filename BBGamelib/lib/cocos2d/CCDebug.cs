using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class CCDebug
	{
		public static byte COCOS2D_DEBUG = 2;
		public static bool COCOS2D_ASSERT = true;
		public static void Info(string format, params object[] args){
			if (COCOS2D_DEBUG > 1){
				if(args.Length > 0)
					Debug.LogFormat (format, args);
				else
					Debug.Log(format);
			}
		}
		public static void Log(string format, params object[] args){
			if (COCOS2D_DEBUG > 0){
				if(args.Length > 0)
					Debug.LogFormat (format, args);
				else
					Debug.Log(format);
			}
		}
		public static void Warning(string format, params object[] args){
			if (COCOS2D_DEBUG > 0){
				if(args.Length > 0)
					Debug.LogWarningFormat (format, args);
				else
					Debug.LogWarning(format);
			}
		}
		public static void Error(string format, params object[] args){
			if(args.Length > 0)
				Debug.LogErrorFormat (format, args);
			else
				Debug.LogError(format);
		}
	}
}

