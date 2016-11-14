using UnityEngine;
using System.Collections;

namespace BBGamelib.flash.imp
{
	public class TagPlaceObject : IDisplayListTag
	{
		public const byte TYPE = 4;
		
		public readonly Flash flash;	
		public readonly bool hasMatrix;
		public readonly bool hasMove;
		public readonly bool hasCharacter;
		public readonly bool hasColorTransform;
		public readonly bool hasVisible;
		public readonly int depth;


		public readonly int characterId;
		public readonly Vector2 position;
		public readonly float rotation;
		public readonly float scaleX;
		public readonly float scaleY;
		public readonly bool visible;
		public readonly ColorTransform colorTransform;
		public readonly string instanceName;

		public TagPlaceObject(Flash flash, byte[] data, Cursor cursor){
			this.flash = flash;
			int dataLength = Utils.ReadInt32 (data, cursor);
			int nextIndex = cursor.index + dataLength;
			
			//parse
			hasMatrix = Utils.ReadByte (data, cursor) != 0;
			hasMove = Utils.ReadByte (data, cursor) != 0;
			hasCharacter = Utils.ReadByte (data, cursor) != 0;
			hasColorTransform = Utils.ReadByte (data, cursor) != 0;
			bool hasName = Utils.ReadByte (data, cursor) != 0;
			hasVisible = Utils.ReadByte (data, cursor) != 0;
			depth = Utils.ReadInt32(data, cursor);

			if(hasCharacter)
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
			if(hasVisible)
				visible = Utils.ReadByte (data, cursor) != 0;

			if (hasColorTransform)
				colorTransform = Utils.ReadColorTransform (data, cursor);
			else
				colorTransform = ColorTransform.Default;

			if(hasName)
				instanceName = Utils.ReadString(data, cursor);
			
			cursor.index = nextIndex;
		}

		
		public void applyFrameObj (Movie movie, Frame frame, FrameObject frameObj){
			Display display = movie.depthDisplays [depth-1];
			if (hasCharacter) {
				if (hasMove && display != null) {
					display.removeFromParent ();
					movie.depthDisplays [depth - 1] = null;
					display = null;
				}
				if(display == null)
				{
					TagDefine newCharacterDefine = flash.getDefine (characterId);
					if (newCharacterDefine is TagDefineDisplay) {
						display = flash.displayFactory.ctDisplay (newCharacterDefine as TagDefineDisplay);
						movie.depthDisplays [depth - 1] = display;
						movie.addChild (display);
						if (display is Movie) {
							(display as Movie).movieCtrl.start ();
						}
					}
				}

				if(frame.frameIndex == movie.movieCtrl.startFrame){
					fixFrame(movie, frame, frameObj);
				}
				
				NSUtils.Assert (display != null, "Movie#applyTagPlaceObject try to reference a null display!");
			}else if (display == null) {
				TagDefine newCharacterDefine = flash.getDefine (frameObj.characterId);
				if (newCharacterDefine is TagDefineDisplay) {
					fixFrame(movie, frame, frameObj);
				}
				display = movie.depthDisplays [depth-1];
				
				NSUtils.Assert (display != null, "Movie#applyTagPlaceObject try to reference a null display!");
			}
			
			if(display != null){
				if (hasMatrix) {
					float preScale = display.define.preScale;
					display.position = new Vector2(position.x, -position.y);
					display.rotation = rotation;
					display.scaleX = scaleX/preScale;
					display.scaleY = scaleY/preScale;
				}else if(frame.frameIndex == 0){
					float preScale = display.define.preScale;
					display.position = Vector2.zero;
					display.rotation = 0;
					display.scaleX = preScale;
					display.scaleY = preScale;
				}

				if(hasVisible)
					display.visible = visible;
				else if(frame.frameIndex == 0){
					display.visible = true;
				}

				display.zOrder = depth;
				
				if (hasColorTransform) {
					display.colorTransform = colorTransform;
					display.opacityTransform = new OpacityTransform(colorTransform.tint.a, colorTransform.add.a);
				} 
				else if(frame.frameIndex == 0){
					display.colorTransform = ColorTransform.Default;
					display.opacityTransform = new OpacityTransform(display.colorTransform.tint.a, display.colorTransform.add.a);
				}
				display.instanceName = instanceName;
			}
		}

		void fixFrame(Movie movie, Frame frame, FrameObject frameObj){
			int lastKeyFrameIndex = -1;
			for(int i=frame.frameIndex; i>=0; i--){
				Frame preFrame = movie.movieDefine.frames[i];
				for(int j=0; j<preFrame.objs.Length; j++){
					FrameObject preFrameObj = preFrame.objs[j];
					if(preFrameObj.isKeyFrame && preFrameObj.placedAtIndex == frameObj.placedAtIndex){
						lastKeyFrameIndex = i;
						break;
					}
				}
				if(lastKeyFrameIndex>=0)
					break;
			}
			for(int i=lastKeyFrameIndex; i<frame.frameIndex; i++){
				Frame preFrame = movie.movieDefine.frames[i];
				for(int j=0; j<preFrame.objs.Length; j++){
					FrameObject preFrameObj = preFrame.objs[j];
					if(preFrameObj.placedAtIndex == frameObj.placedAtIndex){
						movie.applyFrameObj(preFrame, preFrameObj);
					}
				}
			}
		}
	}
}

