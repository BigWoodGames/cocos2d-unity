using UnityEngine;
using System.Collections;


namespace BBGamelib.flash.imp{
	public class DefineFactory
	{
		
		public static Define ParseDefine(Flash flash, byte[] data, Cursor cursor){
			//find nextIndex
			int dataLength = Utils.ReadInt32 (data, cursor);
			int nextIndex = cursor.index + dataLength;
			
			//parse
			byte type = Utils.ReadByte(data, cursor);
			Define def = null;
			if(type==Define.DEF_TYPE_GRAPHIC){
				def = new DefineGraphic().init(flash, data, cursor);
			}else if(type==Define.DEF_TYPE_SPRITE){
				def = new DefineMovie().init(flash, data, cursor);
			}else if(type==Define.DEF_TYPE_SOUND){
				//				def = new BBFlashGraphicDefinition().init(data, tmpcursor.index);
			}else if(type==Define.DEF_TYPE_TEXT){
				//				def = new BBFlashGraphicDefinition().init(data, tmpcursor.index);
			}
			
			
			//nextIndex
			cursor.index = nextIndex;
			
			return def;
		}
	}
}
