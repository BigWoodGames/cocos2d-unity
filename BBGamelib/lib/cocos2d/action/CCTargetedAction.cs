using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** Overrides the target of an action so that it always runs on the target
	 * specified at action creation rather than the one specified by runAction.
	 */
	public class CCTargetedAction : CCActionInterval
	{
		
		System.Object _forcedTarget;
		CCActionFiniteTime _action;

		/** This is the target that the action will be forced to run with */
		public System.Object forcedTarget{
			set{_forcedTarget = value;}
			get{ return _forcedTarget;}
		}
		
		/** Create an action with the specified action and forced target */
		public CCTargetedAction(System.Object target, CCActionFiniteTime action){
			NSUtils.Assert (target != null, "CCTargetedAction: target should not be null");
			NSUtils.Assert (action != null, "CCTargetedAction: action should not be null");
			initWithTarget (target, action);	
		}
		/** Init an action with the specified action and forced target */
		void initWithTarget(System.Object targetIn, CCActionFiniteTime actionIn){
			base.initWithDuration (actionIn.duration);
			_forcedTarget = targetIn;
			_action = actionIn;
		}
		protected override CCAction copyImpl ()
		{
			CCAction copy = new CCTargetedAction (_forcedTarget, _action.copy ());
			return copy;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_action.startWithTarget (_forcedTarget);
		}

		public override void stop ()
		{
			_action.stop ();
		}
		public override void update (float dt)
		{
			_action.update (dt);
		}

		protected override CCAction reverseImpl ()
		{
			NSUtils.Assert(false, "CCTargetedAction: reverse not implemented.");
			return null;
		}
	}
}

