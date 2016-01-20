using UnityEngine;
using System.Collections;

namespace BBGamelib{
	#region mark CCEaseRateAction
	/** Base class for Easing actions with rate parameters*/
	public abstract class CCEaseRateAction :  CCActionEase
	{
		protected float	_rate;
		public float rate{get{return _rate;} set{_rate=value;}}
		
		/** Creates the action with the inner action and the rate parameter */
		public CCEaseRateAction(CCActionInterval action, float rate){
			initWithAction (action, rate);
		}
		/** Initializes the action with the inner action and the rate parameter */
		public void initWithAction(CCActionInterval  action, float rate){
			base.initWithAction (action);
			this.rate = rate;
		}
	}
	#endregion


	/** CCEaseIn action with a rate*/
	public class CCEaseIn : CCEaseRateAction
	{
		public CCEaseIn(CCActionInterval action, float rate):base(action, rate){}

		// Needed for BridgeSupport
		public override void update (float dt)
		{
			_inner.update (Mathf.Pow (dt, _rate));
		}

		protected override CCAction copyImpl ()
		{
			return new CCEaseIn (_inner.copy(), rate);
		}

		protected override CCAction reverseImpl ()
		{
			return new CCEaseIn (_inner.reverse (), 1/rate);
		}
	}

	/** CCEaseOut action with a rate*/
	public class CCEaseOut : CCEaseRateAction
	{
		public CCEaseOut(CCActionInterval action, float rate):base(action, rate){}
		// Needed for BridgeSupport
		public override void update (float dt)
		{
			_inner.update (Mathf.Pow (dt, 1/_rate));
		}
		protected override CCAction copyImpl ()
		{
			return new CCEaseOut (_inner.copy(), rate);
		}
		protected override CCAction reverseImpl ()
		{
			return new CCEaseIn (_inner.reverse (), 1/rate);
		}
	}
	
	/** CCEaseInOut action with a rate*/
	public class CCEaseInOut : CCEaseRateAction
	{
		public CCEaseInOut(CCActionInterval action, float rate):base(action, rate){}
		public override void update (float t)
		{
			t *= 2;
			if (FloatUtils.Small( t , 1)) {
				_inner.update(0.5f * Mathf.Pow (t, _rate));
			}
			else {
				_inner.update(1.0f - 0.5f * Mathf.Pow(2-t, _rate));
			}
		}
		protected override CCAction copyImpl ()
		{
			return new CCEaseInOut (_inner.copy(), rate);
		}
		protected override CCAction reverseImpl ()
		{
			return new CCEaseIn (_inner.reverse (), rate);
		}
	}
}
