using UnityEngine;
using System.Collections;
using System;

namespace BBGamelib{
    /** bezier configuration structure */
	public struct ccBezierConfig {
		// End position of the bezier.
		public Vector2 endPosition;
		// Bezier control point 1.
		public Vector2 controlPoint_1;
		// Bezier control point 2.
		public Vector2 controlPoint_2;
	} 
    
    //
    // BezierBy
    //An action that moves the target with a cubic Bezier curve by a certain distance.
    #region mark - CCBezierBy
    public class CCBezierBy : CCActionInterval
	{
		protected ccBezierConfig _config;
		protected Vector2 _startPosition;
		protected Vector2 _previousPosition;

        public CCBezierBy(float t, ccBezierConfig c){
			initWithDuration (t, c);		
		}

		public virtual void initWithDuration(float t, ccBezierConfig c){
			base.initWithDuration (t);
			_config = c;
		}

		protected override CCAction copyImpl ()
		{
			return new CCBezierBy (this.duration, _config);
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_previousPosition = _startPosition = (_target as CCNode).position;
		}

		public override void update (float t)
		{
			float xa = 0;
			float xb = _config.controlPoint_1.x;
			float xc = _config.controlPoint_2.x;
			float xd = _config.endPosition.x;
			
			float ya = 0;
			float yb = _config.controlPoint_1.y;
			float yc = _config.controlPoint_2.y;
			float yd = _config.endPosition.y;
			
			float x = bezierat(xa, xb, xc, xd, t);
			float y = bezierat(ya, yb, yc, yd, t);
			
			CCNode node = (CCNode)_target;

			bool stackable = ccConfig.CC_ENABLE_STACKABLE_ACTIONS;
			if(stackable){
				Vector2 currentPos = node.position;
				Vector2 diff = (currentPos - _previousPosition);
				_startPosition = ( _startPosition + diff);
				
				Vector2 newPos = ( _startPosition + new Vector2(x,y));
				node.position = newPos;
				
				_previousPosition = newPos;
			}else{
				node.position = _startPosition + new Vector2(x, y);
			}
		}

		protected override CCAction reverseImpl ()
		{
			ccBezierConfig r = new ccBezierConfig();
			
			r.endPosition	 = (_config.endPosition * -1);
			r.controlPoint_1 = (_config.controlPoint_2 + (_config.endPosition * -1));
			r.controlPoint_2 = (_config.controlPoint_1 + (_config.endPosition * -1));
			
			CCBezierBy act = new CCBezierBy(this.duration, r);
			return act;
		}
		
		// Bezier cubic formula:
		//	((1 - t) + t)3 = 1
		// Expands to…
		//   (1 - t)3 + 3t(1-t)2 + 3t2(1 - t) + t3 = 1
		static float bezierat( float a, float b, float c, float d, float t )
		{
			return (Mathf.Pow(1-t,3) * a +
			        3*t*(Mathf.Pow(1-t,2))*b +
			        3*Mathf.Pow(t,2)*(1-t)*c +
			        Mathf.Pow(t,3)*d );
		}
	}
    #endregion
	
	//
	// BezierTo
	//
	#region mark - CCBezierTo
	public class CCBezierTo : CCBezierBy
	{
		ccBezierConfig _toConfig;

		public CCBezierTo(float t, ccBezierConfig c):base(t, c){
		}
		
		public override void initWithDuration(float t, ccBezierConfig c){
			base.initWithDuration (t);
			_toConfig = c;
		}
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_config.controlPoint_1 = (_toConfig.controlPoint_1 - _startPosition);
			_config.controlPoint_2 = (_toConfig.controlPoint_2 - _startPosition);
			_config.endPosition = (_toConfig.endPosition - _startPosition);
		}

		protected override CCAction copyImpl ()
		{
			CCBezierTo act = new CCBezierTo(this.duration, _toConfig);
			return act;
		}

		protected override CCAction reverseImpl ()
		{
			ccBezierConfig r = new ccBezierConfig();
			
			r.endPosition	 = (_config.endPosition * -1);
			r.controlPoint_1 = (_config.controlPoint_2 + (_config.endPosition * -1));
			r.controlPoint_2 = (_config.controlPoint_1 + (_config.endPosition * -1));
			
			CCBezierTo act = new CCBezierTo(this.duration, r);
			return act;
		}
	}
	#endregion
}
