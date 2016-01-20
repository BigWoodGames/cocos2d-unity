using UnityEngine;
using System.Collections;
using System;

namespace BBGamelib{
	/** Instant actions are immediate actions. They don't have a duration like
	 the CCIntervalAction actions.
	*/
	public abstract class CCActionInstant : CCActionFiniteTime
	{
		public override void init ()
		{
			base.init ();
			_duration = 0;
		}
		public override bool isDone ()
		{
			return true;
		}

		public override void step (float dt)
		{
			update (1);
		}

		public override void update (float dt)
		{
			//nothing
		}

		protected override CCAction reverseImpl ()
		{
			return copy ();
		}

		public new CCActionInstant reverse (){
			return (CCActionInstant)reverseImpl();
		}
		public new CCActionInstant copy (){
			return (CCActionInstant)copyImpl();
		}
	}
}

