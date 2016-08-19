using UnityEngine;
using System.Collections;

namespace BBGamelib{
	//
	// ScaleTo
	//
	#region mark - CC3ScaleTo
	/** Scales a CCNode object to a zoom factor by modifying its scale attribute.
	 @warning This action doesn't support "reverse"
	 */
	public class CC3ScaleTo : CCScaleTo
	{
		protected float _scaleZ;
		protected float _startScaleZ;
		protected float _endScaleZ;
		protected float _deltaZ;

		public CC3ScaleTo(float t, float scale):base(t,scale){
		}
		public override void initWithDuration (float t, float scale)
		{
			base.initWithDuration (t, scale);
			_endScaleZ = scale;
		}
		public CC3ScaleTo(float t, float scaleX, float scaleY, float scaleZ):base(t, scaleX, scaleY){
			initWithDuration (t, scaleX, scaleY);
			_endScaleZ = scaleZ;
		}

		protected override CCAction copyImpl ()
		{
			CCScaleTo act = new CC3ScaleTo(this.duration, _endScaleX, _endScaleY, _endScaleZ);
			return act;
		}

		public override void startWithTarget (object aTarget)
		{
			NSUtils.Assert (aTarget is CC3Node, "CC3Scale only supports with CC3Node, and target is {0}.", aTarget);
			base.startWithTarget (aTarget);
			_startScaleZ = (_target as CC3Node).scaleZ;
			_deltaZ = _endScaleZ - _startScaleZ;
		}

		public override void update (float t)
		{
			// added to support overriding setScale only
			if (FloatUtils.EQ(_startScaleX , _startScaleY) && FloatUtils.EQ(_startScaleX , _startScaleZ) && 
			    FloatUtils.EQ(_endScaleX , _endScaleY) && FloatUtils.EQ(_endScaleX , _endScaleZ))
			{
				(_target as CCNode).scale = _startScaleX + _deltaX * t;
			}
			else
			{
				(_target as CCNode).scaleX = _startScaleX + _deltaX * t;
				(_target as CCNode).scaleY = _startScaleY + _deltaY * t;
				(_target as CC3Node).scaleZ = _startScaleZ + _deltaZ * t;
			}
		}

	}
	#endregion
	//
	// ScaleBy
	//
	#region mark - CCScaleBy
	public class CC3ScaleBy : CC3ScaleTo
	{
		public CC3ScaleBy(float t, float scale):base(t, scale){
		}
		public CC3ScaleBy(float t, float scaleX, float scaleY, float scaleZ):base(t, scaleX, scaleY, scaleZ){
		}
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_deltaX = _startScaleX * _endScaleX - _startScaleX;
			_deltaY = _startScaleY * _endScaleY - _startScaleY;
			_deltaZ = _startScaleZ * _endScaleZ - _startScaleZ;
		}
		protected override CCAction copyImpl ()
		{
			CC3ScaleBy act = new CC3ScaleBy(this.duration, _endScaleX, _endScaleY, _endScaleZ);
			return act;
		}
		protected override CCAction reverseImpl ()
		{
			CC3ScaleBy act = new CC3ScaleBy (_duration, 1/_endScaleX, 1/_endScaleY, 1/_endScaleZ);
			return act;
		}
	}
	#endregion
}

