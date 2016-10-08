using UnityEngine;
using System.Collections;

namespace BBGamelib.flash.imp{
	public abstract class TagDefine : ITag
	{
		public const byte DEF_TYPE_GRAPHIC = 1;
		public const byte DEF_TYPE_SPRITE = 2;

		public readonly Flash flash;
		public readonly int characterId;
		public readonly string className;

		public TagDefine(Flash flash, byte[] data, Cursor cursor){
			this.flash = flash;
			this.characterId = Utils.ReadInt32 (data, cursor);	
			this.className = Utils.ReadString (data, cursor);
		}
		
		public virtual string trace(int indent){
			return Utils.RepeatString(indent) + "[" + string.Format("{0}", characterId) + ":" + GetType().Name + "] ";
		}
	}

	public abstract class TagDefineDisplay : TagDefine{
		public readonly float preScale;
		public TagDefineDisplay(Flash flash, byte[] data, Cursor cursor) : base(flash, data, cursor)
		{
			this.preScale = Utils.ReadFloat (data, cursor);
		}
	}
}