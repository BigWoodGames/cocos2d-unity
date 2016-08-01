using UnityEngine;
using System.Collections;
using BBGamelib;
using System;

namespace BBGamelib{
	//
	// JumpBy
	//
	#region mark - CC3JumpBy
	public class CC3JumpBy : CCActionInterval
	{
		protected Vector3 _startPosition;
		protected Vector3 _delta;
		protected float	_height;
		protected uint _jumps;
		protected Vector3 _previousPos;
		protected Vector3 _axis;

		public CC3JumpBy(float t, Vector3 pos, float height, uint jumps, Vector3 axis){
			initWithDuration (t, pos, height, jumps, axis);		
		}
		
		public void initWithDuration(float t, Vector3 pos, float height, uint jumps, Vector3 axis){
			base.initWithDuration (t);
			_delta = pos;
			_height = height;
			_jumps = jumps;
			_axis = axis;
		}
		
		protected override CCAction copyImpl ()
		{
			CC3JumpBy act = new CC3JumpBy(this.duration, _delta, _height, _jumps, _axis);
			return act;
		}
		
		public override void startWithTarget (object aTarget)
		{
			NSUtils.Assert (aTarget is CC3Node, "CC3Jump only supports with CC3Node, and target is {0}.", aTarget);
			base.startWithTarget (aTarget);
			_previousPos = _startPosition = ((CC3Node)_target).position3D;
		}
		
		public override void update (float t)
		{
			// Sin jump. Less realistic
			//	ccTime y = _height * fabsf( sinf(t * (CGFloat)M_PI * _jumps ) );
			//	y += _delta.y * dt;
			//	// parabolic jump (since v0.8.2)
			float frac =  t * _jumps % 1.0f;
			float y = _height * 4 * frac * (1 - frac);
			Vector3 delta = y * _axis;
			delta += new Vector3(_delta.x * Math.Abs(_axis.x), _delta.y * Math.Abs(_axis.y), _delta.z * Math.Abs(_axis.z)) * t;

			CC3Node node = (CC3Node)_target;
			bool stackable = ccConfig.CC_ENABLE_STACKABLE_ACTIONS;
			if(stackable){
				Vector3 currentPos = node.position3D;
				
				Vector3 diff = currentPos - _previousPos;
				_startPosition = diff +_startPosition;
				
				Vector3 newPos = _startPosition + delta;
				node.position3D = newPos;
				
				_previousPos = newPos;
			}else{
				node.position3D = _startPosition + delta;
			}
		}
		
		protected override CCAction reverseImpl ()
		{
			CC3JumpBy act = new CC3JumpBy(this.duration, _delta * -1, _height, _jumps, _axis);
			return act;
		}
	}
	#endregion

	
	//
	// JumpTo
	//
	#region mark - CCJumpTo
	public class CC3JumpTo : CC3JumpBy
	{
		public CC3JumpTo(float t, Vector3 pos, float height, uint jumps, Vector3 axis):base(t, pos, height, jumps, axis){
		}
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_delta = _delta - _startPosition;
		}
		
		protected override CCAction reverseImpl ()
		{
			CC3JumpTo act = new CC3JumpTo(this.duration, _delta * -1, _height, _jumps, _axis);
			return act;
		}
		
		protected override CCAction copyImpl ()
        {
            CC3JumpTo act = new CC3JumpTo(this.duration, _delta, _height, _jumps, _axis);
            return act;
        }
    }
    #endregion
}

