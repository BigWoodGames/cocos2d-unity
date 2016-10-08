using UnityEngine;
using System.Collections;


namespace BBGamelib.flash.imp{
	public class TagDefineGraphic : TagDefineDisplay
	{
		protected Vector2 _anchorPoint;
		public Vector2 anchorPoint{get{return _anchorPoint;} }
		
		public TagDefineGraphic(Flash flash, byte[] data, Cursor cursor):base(flash, data, cursor){
			_anchorPoint = Utils.ReadVector2 (data, cursor);
		}

		public override string trace (int indent){
			return base.trace (indent) +  "AP: " + _anchorPoint + "\n";
		}
	}
}