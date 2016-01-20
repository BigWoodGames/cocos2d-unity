using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class CCShake : CCActionInterval
	{
		public const int CCSHAKE_EVERY_FRAME = 0;
		float shakeInterval;
		float nextShake;
		bool dampening;
		Vector2 startAmplitude;
		Vector2 amplitude;
		Vector2 last;
		
		public CCShake(float t, Vector2 pamplitude): this(t, pamplitude, true, CCSHAKE_EVERY_FRAME){
		}
		public CCShake(float t, Vector2 pamplitude, bool pdampening): this(t, pamplitude, pdampening, CCSHAKE_EVERY_FRAME){
		}
		public CCShake(float t, Vector2 pamplitude,int pshakeNum):this(t,pamplitude, true, pshakeNum){
		}
		public CCShake(float t, Vector2 pamplitude, bool pdampening, int shakeNum):base(t){
			startAmplitude = pamplitude;
			dampening = pdampening;

			// calculate shake intervals based on the number of shakes
			if(shakeNum == CCSHAKE_EVERY_FRAME)
				shakeInterval = 0;
			else
				shakeInterval = 1.0f/shakeNum;
		}

		protected override CCAction copyImpl ()
		{
			return new CCShake(this.duration, amplitude, dampening, FloatUtils.EQ(shakeInterval , 0) ? 0 : Mathf.RoundToInt(1/shakeInterval));
		}

		protected override CCAction reverseImpl ()
		{
			return new CCShake(this.duration, amplitude, dampening, FloatUtils.EQ(shakeInterval , 0) ? 0 : Mathf.RoundToInt(1/shakeInterval));
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);

			amplitude	= startAmplitude;
			last		= Vector2.zero;
			nextShake	= 0;
		}

		public override void stop ()
		{
			((CCNode)_target).position = ((CCNode)_target).position - last;
			
			base.stop();
		}

		public override void update (float t)
		{
			// waits until enough time has passed for the next shake
			if(FloatUtils.EQ(shakeInterval, 0))
			{} // shake every frame!
			else if(FloatUtils.Small(t , nextShake))
				return; // haven't reached the next shake point yet
			else
				nextShake += shakeInterval; // proceed with shake this time and increment for next shake goal
			
			// calculate the dampening effect, if being used
			if(dampening)
			{
				float dFactor = (1-t);
				amplitude.x = dFactor * startAmplitude.x;
				amplitude.y = dFactor * startAmplitude.y;
			}
			
			Vector2 newp = new Vector2((Random.Range(0, 100)/100.0f*amplitude.x*2) - amplitude.x,(Random.Range(0, 100)/100.0f*amplitude.y*2) - amplitude.y);
			
			// simultaneously un-move the last shake and move the next shake
			((CCNode)_target).position = ((CCNode)_target).position - last + newp;
			
			// store the current shake value so it can be un-done
			last = newp;
		}
	}
}

