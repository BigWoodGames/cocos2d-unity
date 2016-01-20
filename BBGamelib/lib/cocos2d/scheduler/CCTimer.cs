using UnityEngine;
using System.Collections;

namespace BBGamelib.scheduler{
	/** Light weight timer */
	public class CCTimer
	{
		protected float _interval;
		protected float _elapsed;
		protected bool _runForever;
		protected bool _useDelay;
		protected uint _nTimesExecuted;
		protected uint _repeat;//0 = once, 1 is 2 x executed
		protected float _delay;

		/** interval in seconds */
		public float interval{
			set{ _interval = value;}
			get{ return _interval;}
		}

		protected void setupTimer(float interval, uint repeat, float delay){
			_elapsed = -1;
			_interval = interval;
			_delay = delay;
			_useDelay = FloatUtils.Big(_delay, 0);
			_repeat = repeat;
			_runForever = _repeat == CCScheduler.kCCRepeatForever;
		}
		
		protected virtual void trigger(){
			// override me
		}
		
		protected virtual void cancel(){
			// override me
		}
		
		/** triggers the timer */
		public void update(float dt){
			if (FloatUtils.EQ(_elapsed, -1)){
				_elapsed = 0;
				_nTimesExecuted = 0;
			} else {
				if(_runForever && !_useDelay){
					//standard timer usage
					_elapsed += dt;
					if(FloatUtils.EB(_elapsed, _interval)){
						trigger();
						_elapsed = 0;
					}
				}else{
					//advanced usage
					_elapsed += dt;
					if(_useDelay){
						if(FloatUtils.EB(_elapsed, _delay)){
							trigger();
							_elapsed = _elapsed - _delay;
							_nTimesExecuted += 1;
							_useDelay = false;
						}
					}else{
						if(FloatUtils.EB(_elapsed, _interval)){
							trigger();
							_elapsed = 0;
							_nTimesExecuted += 1;
						}
					}
					if(!_runForever && FloatUtils.Big(_nTimesExecuted , _repeat)){
						cancel();
					}
				}
			}
		}
	}
}
