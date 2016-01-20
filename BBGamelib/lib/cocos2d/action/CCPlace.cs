using UnityEngine;
using System.Collections;

namespace BBGamelib{
	//
	// Places the node in a certain position
	//
	#region mark CCPlace
	public class CCPlace : CCActionInstant
	{
		Vector2 _position;
		
		/** creates a Place action with a position */
		public CCPlace(Vector2 pos){
			initWithPosition (pos);
		}
		
		/** Initializes a Place action with a position */
		public void initWithPosition(Vector2 pos){
			base.init ();
			_position = pos;		
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCPlace (_position);
		}
		
		public override void update (float dt)
		{
			(_target as CCNode).position = _position;
		}
	}
	#endregion
}
