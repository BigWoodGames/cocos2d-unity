using UnityEngine;
using System.Collections;

namespace BBGamelib{
	//
	// ScaleTo
	//
	#region mark - CCScaleTo
	/** Scales a CCNode object to a zoom factor by modifying its scale attribute.
	 @warning This action doesn't support "reverse"
	 */
	public class CCScaleTo : CCActionInterval
	{	
		protected float _scaleX;
		protected float _scaleY;
		protected float _startScaleX;
		protected float _startScaleY;
		protected float _endScaleX;
		protected float _endScaleY;
		protected float _deltaX;
		protected float _deltaY;

		public CCScaleTo(float t, float scale){
			initWithDuration (t, scale);
		}
		
		public virtual void initWithDuration(float t, float scale){
			base.initWithDuration (t);
			_endScaleX = scale;
			_endScaleY = scale;
		}
		
		public CCScaleTo(float t, float scaleX, float scaleY){
			initWithDuration (t, scaleX, scaleY);
		}
		
		public virtual void initWithDuration(float t, float scaleX, float scaleY){
			base.initWithDuration (t);
			_endScaleX = scaleX;
			_endScaleY = scaleY;
		}
		
		protected override CCAction copyImpl ()
		{
			CCScaleTo act = new CCScaleTo(this.duration, _endScaleX, _endScaleY);
			return act;
		}
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_startScaleX = (_target as CCNode).scaleX;
			_startScaleY = (_target as CCNode).scaleY;
			_deltaX = _endScaleX - _startScaleX;
			_deltaY = _endScaleY - _startScaleY;
		}
		
		public override void update (float t)
		{
			// added to support overriding setScale only
			if (FloatUtils.EQ(_startScaleX , _startScaleY) && FloatUtils.EQ(_endScaleX , _endScaleY))
			{
				(_target as CCNode).scale = _startScaleX + _deltaX * t;
			}
			else
			{
				(_target as CCNode).scaleX = _startScaleX + _deltaX * t;
				(_target as CCNode).scaleY = _startScaleY + _deltaY * t;
			}
		}

		protected override CCAction reverseImpl ()
		{
			NSUtils.Assert(false, "CCScaleTo: reverse not implemented.");
			return null;
		}
	}
	#endregion
	//
	// ScaleBy
	//
	#region mark - CCScaleBy
	public class CCScaleBy : CCScaleTo
	{
		public CCScaleBy(float t, float scale):base(t, scale){
		}
		public CCScaleBy(float t, float scaleX, float scaleY):base(t, scaleX, scaleY){
		}
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_deltaX = _startScaleX * _endScaleX - _startScaleX;
			_deltaY = _startScaleY * _endScaleY - _startScaleY;
		}
		protected override CCAction copyImpl ()
		{
			CCScaleBy act = new CCScaleBy(this.duration, _endScaleX, _endScaleY);
			return act;
		}
		protected override CCAction reverseImpl ()
		{
			CCScaleBy act = new CCScaleBy (_duration, 1/_endScaleX, 1/_endScaleY);
			return act;
		}
	}
	#endregion
}
