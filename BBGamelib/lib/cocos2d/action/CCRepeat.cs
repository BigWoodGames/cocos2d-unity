using UnityEngine;
using System.Collections;

namespace BBGamelib{
	
	/** Repeats an action a number of times.
	 * To repeat an action forever use the CCRepeatForever action.
	 */
	public class CCRepeat : CCActionInterval
	{
		uint _times;
		uint _total;
		float _nextDt;
		bool _isActionInstant;
		CCActionFiniteTime _innerAction;

		public CCRepeat(CCActionFiniteTime action, uint times){
			initWithAction (action, times);	
		}

		public void initWithAction(CCActionFiniteTime action, uint times){
			float d = action.duration * times;
			base.initWithDuration (d);
			_times = times;
			this.innerAction = action;
			_isActionInstant = action is CCActionInstant;
			if (_isActionInstant)
					_times -= 1;
			_total = 0;
		}

		/** Inner action */
		public CCActionFiniteTime innerAction{
			set{ _innerAction = value;}
			get{ return _innerAction;}
		}


		protected override CCAction copyImpl ()
		{
			CCRepeat act = new CCRepeat (_innerAction.copy (), _times);
			return act;
		}

		public override void startWithTarget (object aTarget)
		{
			_total = 0;
			_nextDt = _innerAction.duration / _duration;
			base.startWithTarget (aTarget);
			_innerAction.startWithTarget (aTarget);
		}

		public override void stop ()
		{
			_innerAction.stop ();
			base.stop ();
		}
		
		// issue #80. Instead of hooking step:, hook update: since it can be called by any
		// container action like CCRepeat, CCSequence, CCEase, etc..
		public override void update (float dt)
		{
			
			if (FloatUtils.EB(dt , _nextDt))
			{
				while (FloatUtils.Big(dt , _nextDt) && _total < _times)
				{
					
					_innerAction.update(1.0f);
					_total++;
					
					_innerAction.stop();
					_innerAction.startWithTarget(_target);
					_nextDt += _innerAction.duration/_duration;
				}
				
				// fix for issue #1288, incorrect end value of repeat
				if(FloatUtils.EB(dt , 1.0f) && _total < _times) 
				{
					_total++;
				}
				
				// don't set a instantaction back or update it, it has no use because it has no duration
				if (!_isActionInstant)
				{
					if (_total == _times)
					{
						_innerAction.update(1);
						_innerAction.stop();
					}
					else
					{
						// issue #390 prevent jerk, use right update
						_innerAction.update(dt - (_nextDt - _innerAction.duration/_duration));
					}
				}
			}
			else
			{
				_innerAction.update((dt * _times) % 1.0f);
			}
		}

		public override bool isDone ()
		{
			return _total == _times;
		}

		protected override CCAction reverseImpl ()
		{
			return new CCRepeat(_innerAction.reverse(), _times);
		}
	}
}
