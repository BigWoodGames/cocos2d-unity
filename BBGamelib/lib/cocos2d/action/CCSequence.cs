using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class CCSequence : CCActionInterval
	{
		CCActionFiniteTime[] _actions = new CCActionFiniteTime[2];
		float _split;
		int _last;
		
		
		public static CCActionFiniteTime Actions(CCActionFiniteTime action1, params CCActionFiniteTime[] args){
			CCActionFiniteTime prev = action1;
			if(args!=null){
				for(int i=0; i<args.Length; i++){
					CCActionFiniteTime now  = args[i];
					if(now==null)
						break;
					prev = ActionWithOneTwo(prev, now);		
				}
			}
			return prev;
		}
		
		public static CCActionFiniteTime ActionWithArray(CCActionFiniteTime[] actions){
			NSUtils.Assert (actions != null && actions.Length > 0, "CCActionSequence: actions should not be null.");
			CCActionFiniteTime prev = actions [0];
			for (int i=1; i<actions.Length; i++){
				CCActionFiniteTime now  = actions[i];
				if(now==null)
					break;
				prev = ActionWithOneTwo (prev, now);
			}
			return prev;
		}
		
		public static CCActionFiniteTime ActionWithOneTwo(CCActionFiniteTime one, CCActionFiniteTime two){
			CCSequence act = new CCSequence(one, two);
			return act;
		}

		public CCSequence(CCActionFiniteTime one, CCActionFiniteTime two){
			initWithAction (one, two);		
		}
		
		protected void initWithAction(CCActionFiniteTime one, CCActionFiniteTime two){
			NSUtils.Assert( one!=null && two!=null, "Sequence: arguments must be non-nil");
			NSUtils.Assert( one!=_actions[0] && one!=_actions[1], "Sequence: re-init using the same parameters is not supported");
			NSUtils.Assert( two!=_actions[1] && two!=_actions[0], "Sequence: re-init using the same parameters is not supported");

			float d = one.duration + two.duration;
			base.initWithDuration(d);
			_actions [0] = one;
			_actions [1] = two;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_split = _actions [0].duration / Mathf.Max (_duration, FloatUtils.Epsilon);
			_last = -1;
		}
		public override void stop ()
		{
			if (_last != -1)
				_actions [_last].stop ();
			base.stop ();
		}
		public override void update (float t)
		{
			int found = 0;
			float new_t = 0;

			CCAction action0 = _actions [0];
			CCAction action1 = _actions [1];

			if(FloatUtils.Small(t , _split) ) {
				// action[0]
				found = 0;
				if( !FloatUtils.EQ(_split , 0) )
					new_t = t / _split;
				else
					new_t = 1;
				
			} else {
				// action[1]
				found = 1;
				if (FloatUtils.EQ( _split , 1) )
					new_t = 1;
				else
					new_t = (t-_split) / (1 - _split );
			}
			
			if ( found==1 ) {
				
				if( _last == -1 ) {
					// action[0] was skipped, execute it.
					action0.startWithTarget(_target);
					action0.update(1.0f);
					action0.stop();
				}
				else if( _last == 0 )
				{
					// switching to action 1. stop action 0.
					action0.update(1.0f);
					action0.stop();
				}
			}
			else if(found==0 && _last==1 )
			{
				// Reverse mode ?
				// XXX: Bug. this case doesn't contemplate when _last==-1, found=0 and in "reverse mode"
				// since it will require a hack to know if an action is on reverse mode or not.
				// "step" should be overriden, and the "reverseMode" value propagated to inner Sequences.
				action1.update(0);
				action1.stop();
			}
			
			// Last action found and it is done.
			if( found == _last && _actions[found].isDone() ) {
				return;
			}
			
			// New action. Start it.
			if( found != _last )
				_actions[found].startWithTarget(_target);
			
			_actions[found].update(new_t);
			_last = found;
		}
		
		protected override CCAction copyImpl ()
		{
			CCSequence act = new CCSequence(_actions [0].copy (), _actions [1].copy ());
			return act;
		}

		protected override CCAction reverseImpl ()
		{
			CCAction act = new CCSequence (_actions[1].reverse(), _actions[0].reverse());
			return act;
		}
	}
}