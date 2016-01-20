using UnityEngine;
using System.Collections;

namespace BBGamelib{
	
	/** Delays the action a certain amount of seconds*/
	public class CCDelayTime : CCActionInterval
	{
		public CCDelayTime(float d){
			if(FloatUtils.EQ(d, 0))
				d = FloatUtils.Epsilon;
			initWithDuration (d);

		}
		// XXX: Added to prevent bug on BridgeSupport
		public override void update (float dt)
		{
			return;
		}
		protected override CCAction copyImpl ()
		{
			return new CCDelayTime(_duration);
		}

		protected override CCAction reverseImpl ()
		{
			return new CCDelayTime(_duration);
		}
	}
}
