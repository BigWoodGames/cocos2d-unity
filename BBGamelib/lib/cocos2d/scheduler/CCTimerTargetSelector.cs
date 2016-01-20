using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System;

namespace BBGamelib.scheduler{
	public class CCTimerTargetSelector : CCTimer
	{
		System.Object _target;
		TICK_IMP _selector;
		/** selector */
		public TICK_IMP selector{get{return _selector;}}

		/** Initializes a timer with a target, a selector, an interval in seconds, repeat in number of times to repeat, delay in seconds */
		public CCTimerTargetSelector(System.Object t, TICK_IMP selector, float interval=0, uint repeat=CCScheduler.kCCRepeatForever, float delay=0){
			if(CCDebug.COCOS2D_DEBUG>=1)
				NSUtils.Assert (selector != null, "Method not found for selector - does it have the following form? void name(float dt)");
			_target = t;
			_selector = selector;
			setupTimer (interval, repeat, delay);
		}
		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | target:{2} selector:({3})>", this.GetType().Name, this.GetHashCode(),_target.GetType().Name,  _selector);
		}

		protected override void trigger ()
		{
			_selector.Invoke (_elapsed);
		}

		protected override void cancel ()
		{
			CCDirector.sharedDirector.scheduler.unscheduleSelector (_selector, _target);
		}
	}
}

