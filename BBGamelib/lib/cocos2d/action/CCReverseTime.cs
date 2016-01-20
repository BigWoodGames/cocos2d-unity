using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class CCReverseTime : CCActionInterval
	{
		CCActionFiniteTime  _other;

		public CCReverseTime(CCActionFiniteTime action){
			initWithAction(action);
		}

		public void initWithAction(CCActionFiniteTime action){
			NSUtils.Assert(action != null, "CCReverseTime: action should not be nil");
			NSUtils.Assert(action != _other, "CCReverseTime: re-init doesn't support using the same arguments");

			base.initWithDuration (action.duration);
			_other = action;
		}

		protected override CCAction copyImpl ()
		{
			CCReverseTime a = new CCReverseTime(_other.copy());
			return a;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_other.startWithTarget (_target);
		}

		public override void stop ()
		{
			_other.stop ();
			base.stop ();
		}

		public override void update (float t)
		{
			_other.update(1-t);
		}

		protected override CCAction reverseImpl ()
		{
			return _other.copy ();
		}
	}
}
