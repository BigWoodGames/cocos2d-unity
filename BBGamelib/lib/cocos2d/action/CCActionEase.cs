using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** Base class for Easing actions */
	public abstract class CCActionEase : CCActionInterval
	{
		/** The inner action */
		protected CCActionInterval _inner;
		public CCActionInterval inner{get{return _inner;}}
		
		/** creates the action */
		public CCActionEase(){}
		public CCActionEase(CCActionInterval action){
			initWithAction (action);
		}
		/** initializes the action */
		public void initWithAction(CCActionInterval action)
		{
			NSUtils.Assert( action!=null, "Ease: arguments must be non-nil");

			base.initWithDuration (action.duration);
			_inner = action;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_inner.startWithTarget (_target);
		}

		public override void stop ()
		{
			_inner.stop ();
			base.stop ();
		}

		public override void update (float dt)
		{
			_inner.update (dt);
		}
	}
}

