using UnityEngine;
using System.Collections;

namespace BBGamelib{
    //
    // FadeIn
    //
    #region mark - CCFadeIn
    public class CCFadeIn : CCActionInterval
	{
        public CCFadeIn(float d): base(d){
		}

		public override void update (float t)
		{
			(_target as CCRGBAProtocol).opacity = (byte)(255 * t);
		}

		protected override CCAction copyImpl ()
		{
			return new CCFadeIn(_duration);
		}

		protected override CCAction reverseImpl ()
		{
			return new CCFadeOut(_duration);
		}
	}
    #endregion
	//
	// FadeOut
	//
	#region mark - CCFadeOut
	public class CCFadeOut : CCActionInterval
	{
		public CCFadeOut(float d){
			initWithDuration (d);
		}
		
		public override void update (float t)
		{
			(_target as CCRGBAProtocol).opacity = (byte)(255 * (1 - t));
		}

		protected override CCAction copyImpl ()
		{
			return new CCFadeOut(_duration);
		}

		protected override CCAction reverseImpl ()
		{
			return new CCFadeIn(_duration);
		}
	}
	#endregion
	
	//
	// FadeTo
	//
	#region mark - CCFadeTo
	public class CCFadeTo : CCActionInterval
	{
		byte _toOpacity;
		byte _fromOpacity;

		public CCFadeTo(float d, byte opacity){
			initWithDuration (d, opacity);		
		}
		
		public virtual void initWithDuration(float t, byte opacity){
			base.initWithDuration (t);
			_toOpacity = opacity;
		}

		protected override CCAction copyImpl ()
		{
			CCFadeTo act = new CCFadeTo(this.duration, _toOpacity);
			return act;
		}
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_fromOpacity = (_target as CCRGBAProtocol).opacity;
		}
		
		
		public override void update (float t)
		{
			(_target as CCRGBAProtocol).opacity = (byte)(_fromOpacity + (_toOpacity - _fromOpacity) * t);
		}

		protected override CCAction reverseImpl ()
		{
			NSUtils.Assert(false, "CCFadeTo: reverse not implemented.");
			return null;
		}
	}
	#endregion
}
