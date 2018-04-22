using UnityEngine;
using System.Collections;

namespace BBGamelib{
	
	//
	// EaseElastic
	//
	public abstract class CCEaseBounce : CCActionEase
	{
		public CCEaseBounce(CCActionInterval action):base(action){}
		protected float bounceTime(float t)
		{
			if (FloatUtils.Small( t , 1 / 2.75f)) {
				return 7.5625f * t * t;
			}
			else if (FloatUtils.Small( t , 2 / 2.75f)) {
				t -= 1.5f / 2.75f;
				return 7.5625f * t * t + 0.75f;
			}
			else if (FloatUtils.Small( t , 2.5f / 2.75f)) {
				t -= 2.25f / 2.75f;
				return 7.5625f * t * t + 0.9375f;
			}
			
			t -= 2.625f / 2.75f;
			return 7.5625f * t * t + 0.984375f;
		}
	}

	//
	// CCEaseBounceIn
	//
	public class CCEaseBounceIn : CCEaseBounce
	{
		public CCEaseBounceIn(CCActionInterval action):base(action){}
		public override void update (float t)
		{
			float newT = t;
			// prevents rounding errors
			if (!FloatUtils.EQ(t , 0) && !FloatUtils.EQ( t , 1))
				newT = 1 - bounceTime(1-t);
			
			_inner.update(newT);
		}
		
		protected override CCAction copyImpl ()
		{
			CCEaseBounceIn action = new CCEaseBounceIn (_inner.copy ());
			return action;
		}

		protected override CCAction reverseImpl ()
		{
			return new CCEaseBounceIn (_inner.reverse ());
		}
	}

	//
	// CCEaseBounceOut
	//
	public class CCEaseBounceOut : CCEaseBounce
	{
		public CCEaseBounceOut(CCActionInterval action):base(action){}
		public override void update (float t)
		{
			float newT = t;
			// prevents rounding errors
			if (!FloatUtils.EQ(t , 0) && !FloatUtils.EQ( t , 1))
				newT = bounceTime(t);
			
			_inner.update(newT);
		}
		
		protected override CCAction copyImpl ()
		{
			CCEaseBounceOut action = new CCEaseBounceOut (_inner.copy ());
			return action;
		}
		
		protected override CCAction reverseImpl ()
		{
			return new CCEaseBounceOut (_inner.reverse ());
		}
	}

	//
	// CCEaseBounceInOut
	//
	public class CCEaseBounceInOut : CCEaseBounce
	{
		public CCEaseBounceInOut(CCActionInterval action):base(action){}
		public override void update (float t)
		{
			float newT;
			// prevents possible rounding errors
			if (FloatUtils.EQ(t , 0) || FloatUtils.EQ( t , 1))
				newT = t;
			else if( FloatUtils.Small(t , 0.5f) ){
				t = t * 2;
				newT = (1 - bounceTime(1-t) ) * 0.5f;
			} else
				newT = bounceTime(t * 2 - 1) * 0.5f + 0.5f;
			
			_inner.update(newT);
		}
		
		protected override CCAction copyImpl ()
		{
			CCEaseBounceInOut action = new CCEaseBounceInOut (_inner.copy ());
			return action;
		}
		
		protected override CCAction reverseImpl ()
		{
			return new CCEaseBounceInOut (_inner.reverse ());
		}
	}

}