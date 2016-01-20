using UnityEngine;
using System.Collections;

namespace BBGamelib{
	//
	// JumpBy
	//
	#region mark - CCJumpBy
	public class CCJumpBy : CCActionInterval
	{
		protected Vector2 _startPosition;
		protected Vector2 _delta;
		protected float	_height;
		protected uint _jumps;
		protected Vector2 _previousPos;

        public CCJumpBy(float t, Vector2 pos, float height, uint jumps){
			initWithDuration (t, pos, height, jumps);		
		}

		public void initWithDuration(float t, Vector2 pos, float height, uint jumps){
			base.initWithDuration (t);
			_delta = pos;
			_height = height;
			_jumps = jumps;
		}

		protected override CCAction copyImpl ()
		{
			CCJumpBy act = new CCJumpBy(this.duration, _delta, _height, _jumps);
			return act;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_previousPos = _startPosition = ((CCNode)_target).position;
		}

		public override void update (float t)
		{
			// Sin jump. Less realistic
			//	ccTime y = _height * fabsf( sinf(t * (CGFloat)M_PI * _jumps ) );
			//	y += _delta.y * dt;
			
			//	// parabolic jump (since v0.8.2)
			float frac =  t * _jumps % 1.0f;
			float y = _height * 4 * frac * (1 - frac);
			y += _delta.y * t;
			
			float x = _delta.x * t;
			
			CCNode node = (CCNode)_target;
			bool stackable = ccConfig.CC_ENABLE_STACKABLE_ACTIONS;
			if(stackable){
				Vector2 currentPos = node.position;
				
				Vector2 diff = currentPos - _previousPos;
				_startPosition = diff +_startPosition;
				
				Vector2 newPos = _startPosition + new Vector2(x,y);
				node.position = newPos;
				
				_previousPos = newPos;
			}else{
				node.position = _startPosition + new Vector2(x, y);
			}
		}

		protected override CCAction reverseImpl ()
		{
			CCJumpBy act = new CCJumpBy(this.duration, _delta * -1, _height, _jumps);
			return act;
		}
	}
	#endregion
	
	//
	// JumpTo
	//
	#region mark - CCJumpTo
	public class CCJumpTo : CCJumpBy
	{
		public CCJumpTo(float t, Vector2 pos, float height, uint jumps):base(t, pos, height, jumps){
		}
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_delta = new Vector2 (_delta.x - _startPosition.x, _delta.y - _startPosition.y);
		}

		protected override CCAction reverseImpl ()
		{
			CCJumpTo act = new CCJumpTo(this.duration, _delta * -1, _height, _jumps);
			return act;
		}

		protected override CCAction copyImpl ()
		{
			CCJumpTo act = new CCJumpTo(this.duration, _delta, _height, _jumps);
			return act;
		}
	}
	#endregion
}