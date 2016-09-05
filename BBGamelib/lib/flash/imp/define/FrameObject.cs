using UnityEngine;
using System.Collections;

namespace BBGamelib.flash.imp{
	public class FrameObject{
		public readonly int frameNumber;
		public readonly string label;
		public readonly int soundEvents;//No use in current version
//		public readonly PlaceObject[] placeObjects;
//		public readonly int[] removedObjectDepths;

		byte[] _data;
		int _dataOffset;
//		int _placeObjectCount;
//		Cursor _cursor;

//		public int placeObjectCount{get{return _placeObjectCount;}}


		public FrameObject(int aFrameNumber, byte[] data, Cursor cursor){
			//find nextIndex
			frameNumber = aFrameNumber;
			int dataLength = Utils.ReadInt32 (data, cursor);
			int nextIndex = cursor.index + dataLength;
			
			//parse
			label = Utils.ReadString (data, cursor);
			soundEvents = Utils.ReadInt32 (data, cursor);

			_data = data;
//			_placeObjectCount = Utils.ReadInt32 (data, cursor);
			_dataOffset = cursor.index;
//			int objsCount = Utils.ReadInt32 (data, cursor);
//			placeObjects = new PlaceObject[objsCount];
//			for(int i=0; i<objsCount; i++){
//				placeObjects[i] = new PlaceObject(data, cursor);
//			}
			
			//nextIndex
			cursor.index = nextIndex;
		}


		PlaceObject[] _placeObjects;
		public PlaceObject[] placeObjects{
			get{
				if(_placeObjects == null){
					Cursor cursor = new Cursor();
					cursor.index = _dataOffset;
					int objsCount = Utils.ReadInt32 (_data, cursor);
					_placeObjects = new PlaceObject[objsCount];
					for(int i=0; i<objsCount; i++){
						_placeObjects[i] = new PlaceObject(_data, cursor);
					}
					_data = null;
				}
				return _placeObjects;
			}
		}
//		public void reset(){
//			_cursor = new Cursor ();
//			_cursor.index = _dataOffset;
//		}
//
//		public PlaceObject nextPlaceObject(){
//			PlaceObject obj = new PlaceObject(_data, _cursor);
//			return obj;
//		}

		public string trace(int indent){
//			string indent0 = Utils.RepeatString(indent);
//			string indent2 = Utils.RepeatString(indent+2);
//			string s = indent0 + "[Frame:" + frameNumber  + (label==null?"":("," + label)) + "]" + 
//				indent2 + "placeObjects("+placeObjects.Length + ")";
//			if (placeObjects.Length > 0) {
//				s += ":\n";
//				for(int i=0; i<placeObjects.Length; i++){
//					PlaceObject obj = placeObjects[i];
//					s += obj.trace(indent + 2) + "\n";
//				}
//			}
//			return s;
			return "";
		}
	}
	
	public struct PlaceObject
	{
		public readonly bool hasCharacter;
		public readonly int characterId;
		public readonly int depth;
		public readonly bool isVisible;
		public readonly bool hasMatrix;
		public readonly Vector2 position;
		public readonly float rotation;
		public readonly float scaleX;
		public readonly float scaleY;
		public readonly bool hasColorTransform;
		public readonly ColorTransform colorTransform;
		public readonly string instanceName;
		
		public PlaceObject(byte[] data, Cursor cursor){
			int dataLength = Utils.ReadInt32 (data, cursor);
			int nextIndex = cursor.index + dataLength;
			
			//parse
			hasMatrix = Utils.ReadByte (data, cursor) == 0 ? false : true;
			hasCharacter = Utils.ReadByte (data, cursor) == 0 ? false : true;
			hasColorTransform = Utils.ReadByte (data, cursor) == 0 ? false : true;
			isVisible = Utils.ReadByte (data, cursor) == 1 ? true : false;
			
			depth = Utils.ReadInt32(data, cursor);
			characterId = Utils.ReadInt32 (data, cursor);
			if (hasMatrix) {
				position = Utils.ReadVector2(data, cursor);
				rotation = Utils.ReadFloat(data, cursor);
				scaleX = Utils.ReadFloat(data, cursor);
				scaleY = Utils.ReadFloat(data, cursor);
			}else{
				position = Vector2.zero;
				rotation = 0;
				scaleX = 1;
				scaleY = 1;
			}


			if(hasColorTransform)
				colorTransform = Utils.ReadColorTransform(data, cursor);
			else
				colorTransform = new ColorTransform(Color.white, Color.black);
			instanceName = Utils.ReadString(data, cursor);
			
			cursor.index = nextIndex;
		}
		
		public string trace(int indent){
			string s = Utils.RepeatString (indent) + " ID=" + characterId + ", " +
								"hasCharacter=" + hasCharacter + ", "+
								"name=" + instanceName + ", " + 
								"depth=" + depth + ", " +
								"isVisible=" + isVisible + ", " + 
								"hasMatrix=" + hasMatrix + ", ";
			if (hasMatrix) {
				s += 	"position=(" + position + ")," + 
						"rotation=" + rotation +"," + 
						"scale=(" + scaleX + "," + scaleY + "),";
			}
			s += "hasColorTransform=" + hasColorTransform;
			if (hasColorTransform) {
				s += "," + "colorTransform=" + colorTransform;		
			}
			return s;
		}
	}
}