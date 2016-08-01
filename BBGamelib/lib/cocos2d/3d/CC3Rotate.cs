using UnityEngine;
using System.Collections;

namespace BBGamelib{
	//
	// RotateTo
	//
	#region mark - CCRotateTo
	public class CC3RotateTo : CCActionInterval
	{
		Vector3 _dstAngle;
		Vector3 _startAngle;
		Vector3 _diffAngle;
		
		public CC3RotateTo(float t, Vector3 angle){
			initWithDuration (t, angle);
		}
		
		
		public virtual void initWithDuration(float t, Vector3 angle){
			base.initWithDuration (t);
			_dstAngle = angle;
		}
		
		
		protected override CCAction copyImpl ()
		{
			CC3RotateTo act = new CC3RotateTo (this.duration, _dstAngle);
			return act;
		}
		
		public override void startWithTarget (object aTarget)
		{
			NSUtils.Assert (aTarget is CC3Node, "CC3Rotate only supports with CC3Node, and target is {0}.", aTarget);
			base.startWithTarget (aTarget);
			
			//Calculate angle
			CC3Node node = (CC3Node)_target;
			_startAngle = new Vector3(node.rotationX, node.rotationY, node.rotation);

			if (FloatUtils.Big(_startAngle.x , 0))
				_startAngle.x = _startAngle.x % 360.0f;
			else
				_startAngle.x = _startAngle.x % -360.0f;

			if (FloatUtils.Big(_startAngle.y , 0))
				_startAngle.y = _startAngle.y % 360.0f;
			else
				_startAngle.y = _startAngle.y % -360.0f;

			if (FloatUtils.Big(_startAngle.z, 0))
				_startAngle.z = _startAngle.z % 360.0f;
			else
				_startAngle.z = _startAngle.z % -360.0f;


			
			_diffAngle = _dstAngle - _startAngle;
			if (FloatUtils.Big(_diffAngle.x, 180))
				_diffAngle.x -= 360;
			if (FloatUtils.Small(_diffAngle.x , -180))
				_diffAngle.x += 360;

			if (FloatUtils.Big(_diffAngle.y, 180))
				_diffAngle.y -= 360;
			if (FloatUtils.Small(_diffAngle.y, -180))
				_diffAngle.y += 360;

			if (FloatUtils.Big(_diffAngle.z, 180))
				_diffAngle.z -= 360;
			if (FloatUtils.Small(_diffAngle.z, -180))
				_diffAngle.z += 360;
		}
		
		public override void update (float t)
		{
			Vector3 newAngle =  _startAngle + (_diffAngle * t);
			((CC3Node)_target).rotationX = newAngle.x;
			((CC3Node)_target).rotationY = newAngle.y;
			((CC3Node)_target).rotation = newAngle.z;
		}
		
		protected override CCAction reverseImpl ()
		{
			CC3RotateTo act = new CC3RotateTo (_duration, -_dstAngle);
			return act;
		}
	}
	#endregion
	//
	// RotateBy
	//
	#region mark - RotateBy
	/** Rotates a CCNode object clockwise a number of degrees by modifying its rotation attribute.*/
	public class CC3RotateBy : CCActionInterval
	{
		Vector3 _angle;
		Vector3 _startAngle;
		
		public CC3RotateBy(float t, Vector3 angle){
			initWithDuration (t, angle);		
		}
		
		public virtual void initWithDuration(float t, Vector3 angle){
			base.initWithDuration (t);
			_angle = angle;
		}
		
		protected override CCAction copyImpl ()
		{
			CC3RotateBy act = new CC3RotateBy (this.duration, _angle);
			return act;
		}
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			CC3Node node = (CC3Node)_target;
			_startAngle = new Vector3(node.rotationX, node.rotationY, node.rotation);
		}
		
		public override void update (float t)
		{
			// XXX: shall I add % 360
			Vector3 newAngle =  _startAngle + (_angle * t);
			((CC3Node)_target).rotationX = newAngle.x;
			((CC3Node)_target).rotationY = newAngle.y;
			((CC3Node)_target).rotation = newAngle.z;
		}
		
		protected override CCAction reverseImpl ()
		{
			CC3RotateBy act = new CC3RotateBy (_duration, -_angle);
			return act;
		}
	}
	#endregion
}