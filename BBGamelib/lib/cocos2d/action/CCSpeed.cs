using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** Changes the speed of an action, making it take longer (speed<1)
	 or less (speed>1) time.
	 Useful to simulate 'slow motion' or 'fast forward' effect.
	 @warning This action can't be Sequence-able because it is not an CCIntervalAction
	 */
	public class CCSpeed : CCAction
	{
		CCActionInterval _innerAction;
		float _speed;

		/** initializes the action */
		public CCSpeed(CCActionInterval action, float speed){
			initWithAction (action, speed);
		}

		public void initWithAction(CCActionInterval action, float speed){
			base.init ();
			this.innerAction = action;
			_speed = speed;
		}
		
		/** alter the speed of the inner function in runtime */
		public float speed{
			get{return _speed;}
			set{ _speed = value;}
		}
		
		/** Inner action of CCSpeed */
		public CCActionInterval innerAction{
			get{return _innerAction;}
            set{ _innerAction = value;}
        }

		protected override CCAction copyImpl ()
		{
			CCSpeed copy = new CCSpeed (_innerAction.copy (), _speed);
			return copy;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_innerAction.startWithTarget (_target);
		}

		public override void stop ()
		{
			_innerAction.stop ();
			base.stop ();
		}

		public override void step (float dt)
		{
			_innerAction.step (dt * _speed);
		}

		public override bool isDone ()
		{
			return _innerAction.isDone ();
		}

		public CCAction reverse(){
			return new CCSpeed(_innerAction.reverse(), _speed);
		}
	}
}

