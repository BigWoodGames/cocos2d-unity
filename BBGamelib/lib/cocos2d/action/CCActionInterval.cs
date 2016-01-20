using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** An interval action is an action that takes place within a certain period of time.
		It has an start time, and a finish time. The finish time is the parameter
		duration plus the start time.

		These CCActionInterval actions have some interesting properties, like:
		 - They can run normally (default)
		 - They can run reversed with the reverse method
		 - They can run with the time altered with the Accelerate, AccelDeccel and Speed actions.

		For example, you can simulate a Ping Pong effect running the action normally and
		then running it again in Reverse mode.

		Example:

			CCAction * pingPongAction = [CCSequence actions: action, [action reverse], nil];
		*/
	public abstract class CCActionInterval : CCActionFiniteTime
	{
		protected float _elapsed;
		
		/** how many seconds had elapsed since the actions started to run. */
		public float elapsed{ get{return _elapsed;}}

		protected bool _firstTick;

		public CCActionInterval(){
		}

		public CCActionInterval(float d){
			initWithDuration (d);		
		}

		/** initializes the action */
		public virtual void initWithDuration(float d){
			base.init ();
			_duration = d;
			
			// prevent division by 0
			// This comparison could be in step:, but it might decrease the performance
			// by 3% in heavy based action games.
			if (FloatUtils.EQ(_duration, 0))
				_duration = float.Epsilon;
			_elapsed = 0;
			_firstTick = true;
		}
		
		/** returns YES if the action has finished */
		public override bool isDone ()
		{
			return FloatUtils.EB(_elapsed , _duration);
		}

		public override void step (float dt)
		{
			if( _firstTick ) {
				_firstTick = false;
				_elapsed = 0;
			} else
				_elapsed += dt;
			update (Mathf.Max (0,					// needed for rewind. elapsed could be negative
			            Mathf.Min (1, _elapsed /
			           		Mathf.Max (_duration, FloatUtils.Epsilon)	// division by 0
						)
						)
			        );
		}
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_elapsed = 0;
			_firstTick = true;
		}

		public new CCActionInterval reverse (){
			return (CCActionInterval)reverseImpl();
		}
		public new CCActionInterval copy (){
			return (CCActionInterval)copyImpl();
		}
	}
}
