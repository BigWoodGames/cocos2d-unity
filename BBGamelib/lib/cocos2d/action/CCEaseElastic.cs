using UnityEngine;
using System.Collections;

namespace BBGamelib{
	
	//
	// EaseElastic
	//
	public abstract class CCEaseElastic : CCActionEase
	{
		protected float _period;
		public CCEaseElastic(CCActionInterval action, float period=0.3f){
			initWithAction (action, period);
		}

		public void initWithAction(CCActionInterval action, float period=0.3f){
			base.initWithAction (action);
			_period = period;
		}

		
		protected override CCAction reverseImpl ()
		{
			NSUtils.Assert(false,@"Override me");
			return null;
		}
	}

	
	//
	// EaseElasticIn
	//
	public class CCEaseElasticIn : CCEaseElastic
	{
		public CCEaseElasticIn(CCActionInterval action):base(action){}
		public CCEaseElasticIn(CCActionInterval action, float period):base(action, period){}


		public override void update (float t)
		{
			float newT = 0;
			if (FloatUtils.EQ(t , 0) || FloatUtils.EQ( t , 1))
				newT = t;
			
			else {
				float s = _period / 4;
				t = t - 1;
				newT = -Mathf.Pow(2, 10 * t) * Mathf.Sin( (t-s) *(Mathf.PI * 2) / _period);
			}
			_inner.update(newT);
		}

		protected override CCAction reverseImpl ()
		{
			CCEaseElasticOut action = new CCEaseElasticOut (_inner.copy (), _period);
			return action;
		}

		protected override CCAction copyImpl ()
		{
			CCEaseElasticIn action = new CCEaseElasticIn (_inner.reverse (), _period);
			return action;
		}
	}
	
	//
	// CCEaseElasticOut
	//
	public class CCEaseElasticOut : CCEaseElastic
	{
		public CCEaseElasticOut(CCActionInterval action):base(action){}
		public CCEaseElasticOut(CCActionInterval action, float period):base(action, period){}
		
		
		public override void update (float t)
		{
			float newT = 0;
			if (FloatUtils.EQ(t , 0) || FloatUtils.EQ( t , 1))
			{
				newT = t;
				
			} else {
				float s = _period / 4;
				newT = Mathf.Pow(2, -10 * t) * Mathf.Sin( (t-s) *(Mathf.PI * 2) / _period) + 1;
			}
			_inner.update(newT);
		}
		protected override CCAction reverseImpl ()
		{
			CCEaseElasticIn action = new CCEaseElasticIn (_inner.reverse (), _period);
			return action;
		}
		
		protected override CCAction copyImpl ()
		{
			CCEaseElasticOut action = new CCEaseElasticOut (_inner.reverse (), _period);
			return action;
		}
	}
	//
	// CCEaseElasticInOut
	//
	public class CCEaseElasticInOut : CCEaseElastic
	{
		public CCEaseElasticInOut(CCActionInterval action):base(action){}
		public CCEaseElasticInOut(CCActionInterval action, float period):base(action, period){}
		
		
		public override void update (float t)
		{
			float newT = 0;
			
			if (FloatUtils.EQ(t , 0) || FloatUtils.EQ( t , 1))
				newT = t;
			else {
				t = t * 2;
				if(! FloatUtils.EQ(_period , 0) )
					_period = 0.3f * 1.5f;
				float s = _period / 4;
				
				t = t -1;
				if( FloatUtils.Small(t , 0) )
					newT = -0.5f * Mathf.Pow(2, 10 * t) * Mathf.Sin((t - s) * (Mathf.PI * 2) / _period);
				else
					newT = Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (Mathf.PI * 2) / _period) * 0.5f + 1;
			}
			_inner.update(newT);
		}
		protected override CCAction reverseImpl ()
		{
			CCEaseElasticInOut action = new CCEaseElasticInOut (_inner.reverse (), _period);
			return action;
		}
		
		protected override CCAction copyImpl ()
		{
			CCEaseElasticInOut action = new CCEaseElasticInOut (_inner.reverse (), _period);
			return action;
		}
	}



}
