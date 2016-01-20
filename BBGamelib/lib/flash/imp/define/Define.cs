using UnityEngine;
using System.Collections;

namespace BBGamelib.flash.imp{
	public abstract class Define
	{
		protected Flash _flash;
		public Flash flash{ get{return _flash;} }
		protected int _characterId;
		public int characterId{ get{return _characterId;}}
		protected string _className;
		public string className{get{return _className;} }
		
		public const byte DEF_TYPE_GRAPHIC = 1;
		public const byte DEF_TYPE_SPRITE = 2;
		public const byte DEF_TYPE_SOUND  = 3;
		public const byte DEF_TYPE_TEXT  = 4;


		public Define init(Flash flash, byte[] data, Cursor cursor){
			//parse
			_flash = flash;
			parse (data, cursor);
			return this;

		}

		protected virtual void parse(byte[] data, Cursor cursor){
			_characterId = Utils.ReadInt32 (data, cursor);	
		}

		public virtual string trace(int indent){
			return Utils.RepeatString(indent) + "[" + string.Format("{0}", _characterId) + ":" + GetType().Name + "] ";
		}

		public abstract DisplayObject createDisplay ();
	}

	
	public abstract class DefineView : Define
	{
		
		protected override void parse(byte[] data, Cursor cursor){
			base.parse (data, cursor);
			_className = Utils.ReadString (data, cursor);
		}
	}

}