using UnityEngine;
using System.Collections;

namespace BBGamelib{
	//
	// TintTo
	//
	#region mark - CCTintTo
	public class CCTintTo : CCActionInterval
	{
		Color32 _to;
		Color32 _from;

		public CCTintTo(float t, byte r, byte g, byte b){
			initWithDuration (t, r, g, b);
		}

		public virtual void initWithDuration(float t, byte r, byte g, byte b){
			base.initWithDuration (t);
			_to = new Color32(r, g, b, 255);
		}

		protected override CCAction copyImpl ()
		{
			CCTintTo act = new CCTintTo(this.duration, _to.r, _to.g, _to.b);
			return act;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			CCRGBAProtocol tn = (CCRGBAProtocol)_target;
			_from = tn.color;
		}

		public override void update (float t)
		{
			CCRGBAProtocol tn = (CCRGBAProtocol)_target;
			tn.color = new Color32 ((byte)Mathf.RoundToInt(_from.r + (_to.r - _from.r) * t),
			                        (byte)Mathf.RoundToInt(_from.g + (_to.g - _from.g) * t),
			                        (byte)Mathf.RoundToInt(_from.b + (_to.b - _from.b) * t), 1);
		}

		protected override CCAction reverseImpl ()
		{		
			NSUtils.Assert(false, "CCTintTo: reverse not implemented.");
			return null;
		}
	}
	#endregion
	//
	// TintBy
	//
	#region mark - CCTintBy
	public class CCTintBy : CCActionInterval
	{
		short _deltaR, _deltaG, _deltaB;
		short _fromR, _fromG, _fromB;

		public CCTintBy(float t, short r, short g, short b){
			initWithDuration (t, r, g, b);	
		}

		public virtual void initWithDuration(float t, short r, short g, short b){
			base.initWithDuration (t);
			_deltaR = r;
			_deltaG = g;
			_deltaB = b;
		}
		
		protected override CCAction copyImpl ()
		{
			CCTintBy act = new CCTintBy(this.duration, _deltaR, _deltaG, _deltaB);
			return act;
		}
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			CCRGBAProtocol tn = (CCRGBAProtocol) _target;
			Color32 color = tn.color;
			
			_fromR = color.r;
			_fromG = color.g;
			_fromB = color.b;
		}
		
		public override void update (float t)
		{
			CCRGBAProtocol tn = (CCRGBAProtocol) _target;
			tn.color = new Color32 ((byte)Mathf.RoundToInt( _fromR + _deltaR * t),
			                        (byte)Mathf.RoundToInt(_fromG + _deltaG * t), 
			                        (byte)Mathf.RoundToInt(_fromB + _deltaB * t), 1);
		}
		
		protected override CCAction reverseImpl ()
		{
			CCTintBy act = new CCTintBy(_duration, (short)-_deltaR, (short)-_deltaG, (short)-_deltaB);
			return act;
		}
	}
	#endregion
}