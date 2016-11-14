using UnityEngine;
using System.Collections;
using System;


namespace BBGamelib.flash.imp{
	public class Cursor
	{
		public int index=0;
	}

	public class Utils
	{
		public static byte ReadByte(byte[] data, Cursor cursor)
		{
			byte aByte = data [cursor.index];
			cursor.index ++;
			return aByte;
		}
		public static int ReadInt32(byte[] data, Cursor cursor)
		{
			int aInt;
			if (BitConverter.IsLittleEndian) {
				int index = cursor.index;
				byte[] newData = new byte[]{data [index + 3], data [index + 2], data [index + 1], data [index + 0]};
				aInt = BitConverter.ToInt32 (newData, 0);			
			} else {
				aInt = BitConverter.ToInt32(data, cursor.index);
			}
			cursor.index += sizeof(Int32);
			return aInt;
		}
		public static float ReadFloat(byte[] data, Cursor cursor)
		{
			float aFloat;
			if (BitConverter.IsLittleEndian) {
				int index = cursor.index;
				byte[] newData = new byte[]{data [index + 3], data [index + 2], data [index + 1], data [index + 0]};
				aFloat = BitConverter.ToSingle (newData, 0);			
			} else {
				aFloat = BitConverter.ToSingle(data, cursor.index);
			}
			cursor.index += sizeof(float);
			return aFloat;
		}
		public static int ReadLength(byte[] data, Cursor cursor)
		{
			return Utils.ReadInt32 (data, cursor);
		}
		public static Vector2 ReadVector2(byte[] data, Cursor cursor)
		{
			Vector2 retVal = new Vector2();
			retVal.x = Utils.ReadFloat (data, cursor);
			retVal.y = Utils.ReadFloat (data, cursor);
			return retVal;
		}
		public static Rect ReadRect(byte[] data, Cursor cursor)
		{
			Rect retVal = new Rect();
			retVal.x 		= Utils.ReadFloat (data, cursor);
			retVal.y 		= Utils.ReadFloat (data, cursor);
			retVal.width 	= Utils.ReadFloat (data, cursor);
			retVal.height	= Utils.ReadFloat (data, cursor);
			return retVal;
		}
		public static Color32 ReadColor4B(byte[] data, Cursor cursor)
		{
			Color32 color = new Color32();
			color.r = Utils.ReadByte(data, cursor);
			color.g = Utils.ReadByte(data, cursor);
			color.b = Utils.ReadByte(data, cursor);
			color.a = Utils.ReadByte(data, cursor);
			return color;
		}
		public static Color ReadColor4f(byte[] data, Cursor cursor)
		{
			Color color = new Color();
			color.r = Utils.ReadFloat(data, cursor);
			color.g = Utils.ReadFloat(data, cursor);
			color.b = Utils.ReadFloat(data, cursor);
			color.a = Utils.ReadFloat(data, cursor);
			return color;
		}
		
		public static string ReadString(byte[] data, Cursor cursor)
		{
			int len = Utils.ReadInt32(data, cursor);
			if (len == 0) {
				return null;		
			}
			System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
			string s = enc.GetString(data, cursor.index, len);
			cursor.index += len;
			return s;
		}
		
		public static ColorTransform ReadColorTransform(byte[] data, Cursor cursor){
			ColorTransform ctr = new ColorTransform ();
			int mtype = Utils.ReadInt32(data, cursor);
			if (mtype == 1) {
				ctr.tint = new Color (1, 1, 1, 1);
			} else if (mtype == 2) {
				Color color = new Color ();
				color.r = Utils.ReadFloat (data, cursor);
				color.g = Utils.ReadFloat (data, cursor);
				color.b = Utils.ReadFloat (data, cursor);
				color.a = Utils.ReadFloat (data, cursor);
				ctr.tint = color;
			} else {
				ctr.tint = new Color(0, 0, 0, 0);
			}
			int atype = Utils.ReadInt32(data, cursor);
			if (atype == 1) {
				ctr.add = new Color (255, 255, 255, 255);
			} else if (atype == 2) {
				Color32 color = new Color ();
				color.r = Utils.ReadByte (data, cursor);
				color.g = Utils.ReadByte (data, cursor);
				color.b = Utils.ReadByte (data, cursor);
				color.a = Utils.ReadByte (data, cursor);
				ctr.add = color;
			} else {
				ctr.add = new Color(0, 0, 0, 0);
			}
			return ctr;
		}


		public static string RepeatString(int n, string str=" "){
			string s = "";
			for(int i=0; i<n; i++)
				s += str;
			return s;
		}
	}
}