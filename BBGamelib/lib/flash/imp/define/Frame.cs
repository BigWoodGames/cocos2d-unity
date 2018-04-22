using UnityEngine;
using System.Collections;

namespace BBGamelib.flash.imp{
	public class Frame{
		public readonly int frameIndex;
		public readonly string label;
		public readonly FrameObject[] objs;

		public Frame(int frameIndex, byte[] data, Cursor cursor){
			this.frameIndex = frameIndex;
			int dataLength = Utils.ReadInt32 (data, cursor);
			int nextIndex = cursor.index + dataLength;
			
			//parse
			label = Utils.ReadString (data, cursor);

			int objsCount = Utils.ReadInt32 (data, cursor);
			objs = new FrameObject[objsCount];
			for(int i=0; i<objsCount; i++){
				FrameObject obj = new FrameObject(data, cursor);
				objs[i] = obj;
			}
			cursor.index = nextIndex;
		}
		public string trace(int indent){
			string indent0 = Utils.RepeatString(indent);
			string indent2 = Utils.RepeatString(indent+2);
			string s = indent0 + "[Frame:" + frameIndex  + (label==null?"":("," + label)) + "]" + 
				indent2 + "placeObjects("+objs.Length + ")";
			if (objs.Length > 0) {
				s += ":\n";
				for(int i=0; i<objs.Length; i++){
					FrameObject obj = objs[i];
					s += obj.trace(indent + 2) + "\n";
				}
			}
			return s;
		}
	}

	public class FrameObject{
		public readonly int characterId;
		public readonly int placedAtIndex;
		public readonly int lastModfiedAtIndex;
		public readonly bool isKeyFrame;

		public FrameObject(byte[] data, Cursor cursor){
			this.characterId = Utils.ReadInt32(data, cursor);
			this.placedAtIndex = Utils.ReadInt32(data, cursor);
			this.lastModfiedAtIndex = Utils.ReadInt32(data, cursor);
			this.isKeyFrame = Utils.ReadByte(data, cursor)!=0;
		}

		public string trace(int indent){
			string s = Utils.RepeatString (indent) + 
				"placedAtIndex=" + placedAtIndex + ", " + 
				"lastModfiedAtIndex=" + lastModfiedAtIndex;
			return s;
		}
	}
}