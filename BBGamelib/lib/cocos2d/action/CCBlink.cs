using UnityEngine;
using System.Collections;
using System;

namespace BBGamelib{
    //
    // Blink
    //
    public class CCBlink : CCActionInterval
	{
		uint _times;
		bool _originalState;

        public CCBlink(float t, uint blinks){
			initWithDuration (t, blinks);		
		}

		public virtual void initWithDuration(float t, uint blinks){
			base.initWithDuration (t);
			_times = blinks;
		}

		protected override CCAction copyImpl ()
		{
			
			CCBlink act = new CCBlink(this.duration, _times);
			return act;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_originalState = (target as CCNode).visible;
		}

		public override void update (float t)
		{
			if (! this.isDone ()) {
				float slice = 1.0f / _times;
				float m = t % slice;
				(_target as CCNode).visible = FloatUtils.Big(m, slice/2);
			}
		}

		public override void stop ()
		{
			(_target as CCNode).visible = _originalState;
			base.stop ();
		}

		protected override CCAction reverseImpl ()
		{
			CCBlink act = new CCBlink (_duration, _times);
			return act;
		}
	}
}

