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

        public void apply (Movie movie, FrameObject frameObj){
            Display display = movie.depthDisplays [depth-1];
            if (hasCharacter)
            {
                if (display != null)
                {
                    if (hasMove || characterId != display.define.characterId)
                    {
//                        display.removeFromParent();
                        display.removed = true;
                        display.visible = false;
                        movie.caches.DL_APPEND(display);

                        movie.depthDisplays [depth - 1] = null;
                        display = null;
                    }else
                    {
                        //reuse cache
                        display.removed = false;
                    }
                } 
                if (display == null)
                {
                    for (var ent = movie.caches.head; ent != null; ent = ent.next)
                    {
                        if (ent.obj.define.characterId == characterId)
                        {
                            display = ent.obj;
                            display.removed = false;
                            display.visible = true;
                            movie.caches.DL_DELETE(ent);
                            movie.depthDisplays [depth - 1] = display;
                            if (display is Movie)
                            {
                                (display as Movie).movieCtrl.start();
                            }
                            break;
                        }
                    }
                }
                if (display == null)
                {
                    TagDefine newCharacterDefine = flash.getDefine(characterId);
                    if (newCharacterDefine is TagDefineDisplay)
                    {
                        display = flash.displayFactory.ctDisplay(newCharacterDefine as TagDefineDisplay);
                        movie.depthDisplays [depth - 1] = display;
                        movie.addChild(display);
                        if (display is Movie)
                        {
                            (display as Movie).movieCtrl.start();
                        }
                    }
                }
                NSUtils.Assert(display != null, "TagPlaceObject#apply failed to create display.");

                display.position = Vector2.zero;
                display.rotation = 0;
                display.scaleX = 1;//display.define.preScale;
                display.scaleY = 1;//display.define.preScale;
                display.zOrder = depth;
                if (display.hasUserVisible)
                {
                    display.visible = display.userVisible;
                } else
                {
                    display.visible = true;
                }
                if (display.hasUserColorTransform)
                {
                    display.colorTransform = display.userColorTransform;;
                    display.opacityTransform = new OpacityTransform(display.userColorTransform.tint.a, display.userColorTransform.add.a);

                } else
                {
                    display.colorTransform = ColorTransform.Default;
                    display.opacityTransform = new OpacityTransform(display.userColorTransform.tint.a, display.userColorTransform.add.a);
                }
            } 
            NSUtils.Assert(display != null, "TagPlaceObject#apply try to reference a null display.");


            if(display != null){
                if (hasMatrix) {
                    float preScale = display.define.preScale;
                    display.position = new Vector2(position.x, -position.y);
                    display.rotation = rotation;
                    display.scaleX = scaleX/preScale;
                    display.scaleY = scaleY/preScale;
                }
                if (hasVisible)
                {
                    if (display.hasUserVisible)
                    {
                        display.visible = display.userVisible;
                    } else
                    {
                        display.visible = visible;
                    }
				}
                if (hasColorTransform) {
                    if (display.hasUserColorTransform)
                    {
                        display.colorTransform = display.userColorTransform;;
                        display.opacityTransform = new OpacityTransform(display.userColorTransform.tint.a, display.userColorTransform.add.a);
                        
                    } else
                    {
                        display.colorTransform = colorTransform;
                        display.opacityTransform = new OpacityTransform(colorTransform.tint.a, colorTransform.add.a);
                    }
                } 
                display.instanceName = instanceName;
            }
        }
	}
}

