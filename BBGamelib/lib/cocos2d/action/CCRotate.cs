using UnityEngine;
using System.Collections;

namespace BBGamelib{
	//
	// RotateTo
	//
	#region mark - CCRotateTo
	public class CCRotateTo : CCActionInterval
	{
		float _dstAngle;
		float _startAngle;
		float _diffAngle;

		public CCRotateTo(float t, float angle){
			initWithDuration (t, angle);
		}


		public virtual void initWithDuration(float t, float angle){
			base.initWithDuration (t);
			_dstAngle = angle;
		}


		protected override CCAction copyImpl ()
		{
			CCRotateTo act = new CCRotateTo (this.duration, _dstAngle);
			return act;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			
			//Calculate angle
			_startAngle = ((CCNode)_target).rotation;
			if (FloatUtils.Big(_startAngle , 0))
				_startAngle = _startAngle % 360.0f;
			else
				_startAngle = _startAngle % -360.0f;
			
			_diffAngle = _dstAngle - _startAngle;
			if (FloatUtils.Big(_diffAngle , 180))
				_diffAngle -= 360;
			if (FloatUtils.Small(_diffAngle , -180))
				_diffAngle += 360;
		}

		public override void update (float t)
		{
			((CCNode)_target).rotation = _startAngle + (_diffAngle * t);
		}

		protected override CCAction reverseImpl ()
		{
			CCRotateTo act = new CCRotateTo (_duration, -_dstAngle);
			return act;
		}
	}
	#endregion
	//
	// RotateBy
	//
	#region mark - RotateBy
	/** Rotates a CCNode object clockwise a number of degrees by modifying its rotation attribute.*/
	public class CCRotateBy : CCActionInterval
	{
		float _angle;
		float _startAngle;
		
		public CCRotateBy(float t, float angle){
			initWithDuration (t, angle);		
		}
		
		public virtual void initWithDuration(float t, float angle){
			base.initWithDuration (t);
			_angle = angle;
		}
		
		protected override CCAction copyImpl ()
		{
			CCRotateBy act = new CCRotateBy (this.duration, _angle);
			return act;
		}
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_startAngle = ((CCNode)_target).rotation;
		}
		
		public override void update (float t)
		{
			// XXX: shall I add % 360
			((CCNode)_target).rotation = _startAngle + (_angle * t);
		}
		
		protected override CCAction reverseImpl ()
		{
			CCRotateBy act = new CCRotateBy (_duration, -_angle);
			return act;
		}
	}
	#endregion
}