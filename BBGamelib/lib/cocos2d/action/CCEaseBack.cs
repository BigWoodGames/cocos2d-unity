using UnityEngine;
using System.Collections;


namespace BBGamelib{
	//
	// EaseBackIn
	//
	public class CCEaseBackIn : CCActionEase
	{
		public CCEaseBackIn(CCActionInterval action):base(action){}
		public override void update (float t)	
		{
			float overshoot = 1.70158f;
			_inner.update( t * t * ((overshoot + 1) * t - overshoot) );
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEaseBackIn (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			return new CCEaseBackIn (_inner.reverse ());
		}
	}

	//
	// EaseBackIn
	//
	public class CCEaseBackOut : CCActionEase
	{
		public CCEaseBackOut(CCActionInterval action):base(action){}
		public override void update (float t)	
		{
			float overshoot = 1.70158f;

			t = t - 1;
			_inner.update(t * t * ((overshoot + 1) * t + overshoot) + 1);
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEaseBackIn (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			return new CCEaseBackIn (_inner.reverse ());
		}
	}

	//
	// EaseBackIn
	//
	public class CCEaseBackInOut : CCActionEase
	{
		public CCEaseBackInOut(CCActionInterval action):base(action){}
		public override void update (float t)	
		{
			float overshoot = 1.70158f * 1.525f;
			
			t = t * 2;
			if (FloatUtils.Small(t , 1))
				_inner.update( (t * t * ((overshoot + 1) * t - overshoot)) / 2);
			else {
				t = t - 2;
				_inner.update((t * t * ((overshoot + 1) * t + overshoot)) / 2 + 1);
			}
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEaseBackInOut (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			return new CCEaseBackInOut (_inner.reverse ());
		}
	}
}
