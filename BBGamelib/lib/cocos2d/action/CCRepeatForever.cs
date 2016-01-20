using UnityEngine;
using System.Collections;
using System;

namespace BBGamelib{
	/** Repeats an action for ever.
	 To repeat the an action for a limited number of times use the Repeat action.
	 @warning This action can't be Sequence-able because it is not an IntervalAction
	 */
	public class CCRepeatForever : CCAction
	{
		/** Inner action */
		CCActionInterval _innerAction;
		public CCActionInterval innerAction{
			set{_innerAction = value;}
			get{return _innerAction;}
		}
		
		/** initializes the action */
		public CCRepeatForever(CCActionInterval action){
			initWithAction (action);
		}

		public void initWithAction(CCActionInterval action){
			base.init ();
			this.innerAction = action;
		}
		
		protected override CCAction copyImpl ()
		{
			CCRepeatForever copy = new CCRepeatForever (_innerAction.copy ());
			return copy;
		}

		public override void startWithTarget (System.Object aTarget)
		{
			base.startWithTarget (aTarget);
			_innerAction.startWithTarget (_target);
		}

		public override void step (float dt)
		{
			_innerAction.step (dt);
			if (_innerAction.isDone ()) {
				float diff = _innerAction.elapsed - _innerAction.duration;
				_innerAction.startWithTarget(_target);

				// to prevent jerk. issue #390, 1247
				_innerAction.step(0.0f);
				_innerAction.step(diff);
			}
		}


		public override bool isDone ()
		{
			return false;
		}

		public CCRepeatForever reverse(){
			return new CCRepeatForever(_innerAction.reverse());
		}

	}
}

