using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System;
using System.Reflection;

namespace BBGamelib{
	public class ccUtils
	{
		public static Vector2 PointFromString(string s){
			s = s.Trim ();
			s = s.Substring (1, s.Length - 2);
			string[] ss = s.Split(',');
			if (ss.Length < 2) {
				return new Vector2 ();		
			} else {
				float x = float.Parse(ss[0]);
				float y = float.Parse(ss[1]);
				return new Vector2(x, y);
			}
		}

		public static string PointToString(Vector2 p){
			string s = string.Format ("{{{0},{1}}}", p.x, p.y);
			return s;
		}

		public static Rect RectFromString(string s){
			s = Regex.Replace (s, "\\{", "");
			s = Regex.Replace (s, "\\}", "");
			string[] ss = s.Split (',');
			if (ss.Length < 4) {
				return new Rect ();		
			} else {
				float x = float.Parse(ss[0]);
				float y = float.Parse(ss[1]);
				float w = float.Parse(ss[2]);
				float h = float.Parse(ss[3]);
				return new Rect(x, y, w, h);
			}
		}

		public static string RectToString(Rect rect){
			string s = string.Format ("{{{0},{1},{2},{3}}}", rect.x, rect.y, rect.width, rect.height);
			return s;
		}

		public static Vector2 PixelsToUnits(Vector2 pixels){
			return pixels / UIWindow.PIXEL_PER_UNIT;		
		}

		public static Vector2 UnitsToPixels(Vector2 units){
			return units * UIWindow.PIXEL_PER_UNIT;
		}

		public static Vector3 PixelsToUnits(Vector3 pixels){
			return pixels / UIWindow.PIXEL_PER_UNIT;		
		}
		
		public static Vector3 UnitsToPixels(Vector3 units){
			return units * UIWindow.PIXEL_PER_UNIT;
		}

		public static float CC_DEGREES_TO_RADIANS(float degree)
		{
			return (float)(Math.PI * degree / 180.0);
		}

		public static float CC_RADIANS_TO_DEGREE(float radians)
		{
			return (float)(radians * 180 / Math.PI);
		}

		public static void CC_INCREMENT_GL_DRAWS(uint n=1){
			CCDirector.sharedDirector.ccNumberOfDraws += n;
		}

		public static uint ccNumberOfDraws(){
			return CCDirector.sharedDirector.ccNumberOfDraws;
		}

		public static Rect RectUnion(Rect a, Rect b) { 
			float x1 = Math.Min(a.x, b.x);
			float x2 = Math.Max(a.x + a.width, b.x + b.width); 
			float y1 = Math.Min(a.y, b.y);
			float y2 = Math.Max(a.y + a.height, b.y + b.height);

			return new Rect(x1, y1, x2 - x1, y2 - y1); 
		}

		public static Rect RectIntersection(Rect a, Rect b) {
			float x1 = Math.Max(a.x, b.x);
			float x2 = Math.Min(a.x + a.width, b.x + b.width); 
			float y1 = Math.Max(a.y, b.y);
			float y2 = Math.Min(a.y + a.height, b.y + b.height); 
			
			if (FloatUtils.EB(x2 , x1)
			    && FloatUtils.EB(y2 , y1)) { 
				return new Rect(x1, y1, x2 - x1, y2 - y1);
			}
			return new Rect(0, 0, 0, 0); 
		}

		public static bool RectIntersectsRect(Rect a, Rect b) {
			return FloatUtils.Small(b.x , a.x + a.width) &&
				FloatUtils.Small(a.x , (b.x + b.width)) && 
					FloatUtils.Small(b.y , a.y + a.height) &&
					FloatUtils.Small(a.y , b.y + b.height); 
		} 

		public static GameObject GetChildObject(GameObject obj, string name){
			for(int i=0; i<obj.transform.childCount; i++){
				GameObject child = obj.transform.GetChild(i).gameObject;
				if(child.name == name){
					return child;
				}
			}
			return null;
		}
		public static GameObject GetChildObjectRecursively(GameObject obj, string name){
			for(int i=0; i<obj.transform.childCount; i++){
				GameObject child = obj.transform.GetChild(i).gameObject;
				if(child.name == name){
					return child;
				}
				child = GetChildObjectRecursively(child, name);
				if(child != null){
					return child;
				}
			}
			return null;
		}

		private static Texture2D _staticRectTexture;
		private static GUIStyle _staticRectStyle;
		// Note that this function is only meant to be called from OnGUI() functions.
		public static void GUIDrawRect( Rect position, Color color )
		{
			if( _staticRectTexture == null )
			{
				_staticRectTexture = new Texture2D( 1, 1 );
			}
			
			if( _staticRectStyle == null )
			{
				_staticRectStyle = new GUIStyle();
			}
			
			_staticRectTexture.SetPixel( 0, 0, color );
			_staticRectTexture.Apply();
			
			_staticRectStyle.normal.background = _staticRectTexture;
			
			GUI.Box( position, GUIContent.none, _staticRectStyle );
		}

		public static void SetRenderColor(Renderer renderer, Color tint, Color add){
			Material m = renderer.sharedMaterial;
			if (m != null) {
				MaterialPropertyBlock block = CCFactory.Instance.materialPropertyBlock;
				renderer.GetPropertyBlock (block);
				if (m.HasProperty ("_Color")) {
					block.SetColor ("_Color", tint);
				}
				if (m.HasProperty ("_AddColor")) {
					block.SetColor ("_AddColor", add);
				}
				renderer.SetPropertyBlock (block);
			}
		}

        public static void SetRenderValue(Renderer renderer, string key, int value){
            Material m = renderer.sharedMaterial;
            if (m != null) {
                MaterialPropertyBlock block = CCFactory.Instance.materialPropertyBlock;
                renderer.GetPropertyBlock (block);
                if (m.HasProperty (key)) {
                    block.SetFloat (key, value);
                }
                renderer.SetPropertyBlock (block);
            }
        }
	}
}