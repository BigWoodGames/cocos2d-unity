using UnityEngine;
using System.Collections;

namespace BBGamelib{
	
	//
	// EaseSineIn
	//
	public class CCEaseSineIn : CCActionEase
	{
		public CCEaseSineIn(CCActionInterval action):base(action){}
		
		// Needed for BridgeSupport
		public override void update (float t)
		{
			_inner.update(-1*Mathf.Cos(t * Mathf.PI/2) +1);
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEaseSineIn (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			CCEaseSineIn action = new CCEaseSineIn (_inner.reverse ());
			return action;
		}
	}

	//
	// EaseSineIn
	//
	public class CCEaseSineOut : CCActionEase
	{
		public CCEaseSineOut(CCActionInterval action):base(action){}
		
		// Needed for BridgeSupport
		public override void update (float t)
		{
			_inner.update(Mathf.Sin(t * Mathf.PI/2));
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEaseSineOut (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			CCEaseSineOut action = new CCEaseSineOut (_inner.reverse ());
			return action;
		}
	}

	//
	// EaseSineIn
	//
	public class CCEaseSineInOut : CCActionEase
	{
		public CCEaseSineInOut(CCActionInterval action):base(action){}
		
		// Needed for BridgeSupport
		public override void update (float t)
		{
			_inner.update(-0.5f*(Mathf.Cos( Mathf.PI*t) - 1));
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEaseSineInOut (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			CCEaseSineInOut action = new CCEaseSineInOut (_inner.reverse ());
			return action;
		}
	}
}

