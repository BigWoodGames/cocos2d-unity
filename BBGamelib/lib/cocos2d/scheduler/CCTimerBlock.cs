using UnityEngine;
using System.Collections;

namespace BBGamelib.scheduler{
	public class CCTimerBlock : CCTimer
	{
		TICK_IMP _block;
		string _key;
		System.Object _target;

		/** unique identifier of the block */
		public string key{get{return _key;}}

		/** owner of the timer */
		public System.Object target{get{return _target;}}

		
		/** Initializes a timer with a target(owner), interval in seconds, repeat in number of times to repeat, delay in seconds and a block */
		public CCTimerBlock(System.Object owner, float seconds, string key, TICK_IMP block, uint repeat=CCScheduler.kCCRepeatForever, float delay=0){
			_block = block;
			_key = key;
			_target = owner;
			setupTimer (seconds, repeat, delay);
		}

		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | block)>", this.GetType().Name, this.GetHashCode());
		}

		protected override void trigger ()
		{
			_block.Invoke (_elapsed);
		}

		protected override void cancel ()
		{
			CCDirector.sharedDirector.scheduler.unscheduleBlockForKey (_key, _target);
		}
	}
}

