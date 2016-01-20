using UnityEngine;
using System.Collections;


namespace BBGamelib.flash.imp{
	public class DefineGraphic : DefineView
	{
		protected Vector2 _anchorPoint;
		public Vector2 anchorPoint{get{return _anchorPoint;} }
		protected override void parse(byte[] data, Cursor cursor){
			base.parse (data, cursor);
			_anchorPoint = Utils.ReadVector2 (data, cursor);
		}
		public override DisplayObject createDisplay ()
		{
			Graphic g = _flash.displayFactory.createGraphic (this);
			return g;
		}	
		public override string trace (int indent){
			return base.trace (indent) +  "AP: " + _anchorPoint + "\n";
		}
	}
}