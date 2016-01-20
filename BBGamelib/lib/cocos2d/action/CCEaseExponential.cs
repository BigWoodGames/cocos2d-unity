using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** CCEase Exponential In*/
	public class CCEaseExponentialIn : CCActionEase
	{
		public CCEaseExponentialIn(CCActionInterval action):base(action){}
		public override void update (float t)
		{
			_inner.update(FloatUtils.EQ(t, 0) ? 0 : Mathf.Pow(2, 10 * (t/1 - 1)) /* - 1 * 0.001f */);
		}

		protected override CCAction copyImpl ()
		{
			return new CCEaseExponentialIn (_inner.copy());
		}

		protected override CCAction reverseImpl ()
		{
			return new CCEaseExponentialOut (_inner.reverse ());
		}
	}

	/** Ease Exponential Out*/
	public class CCEaseExponentialOut : CCActionEase
	{
		public CCEaseExponentialOut(CCActionInterval action):base(action){}
		public override void update (float t)
		{
			_inner.update(FloatUtils.EQ(t, 1) ? 1 : (-Mathf.Pow(2, -10 * t/1) + 1));
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEaseExponentialOut (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			return new CCEaseExponentialIn (_inner.reverse ());
		}
	}

	/** Ease Exponential InOut*/
	public class CCEaseExponentialInOut : CCActionEase
	{
		public CCEaseExponentialInOut(CCActionInterval action):base(action){}
		public override void update (float t)
		{
			// prevents rouding errors
			if( !FloatUtils.EQ(t, 1) && !FloatUtils.EQ(t, 0) ) {
				t *= 2;
				if (FloatUtils.Small( t , 1))
					t = 0.5f * Mathf.Pow(2, 10 * (t - 1));
				else
					t = 0.5f * (-Mathf.Pow(2, -10 * (t -1) ) + 2);
			}
			
			_inner.update(t);
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEaseExponentialInOut (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			return new CCEaseExponentialInOut (_inner.reverse ());
		}
	}
	

}