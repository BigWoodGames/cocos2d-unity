using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** CCEase Polynomial abstract class
	@since v2.1
	 */
	public abstract class CCEasePolynomial : CCActionEase
	{
		public const int kDefaultPolynomial = 6;
	 	protected uint _polynomialOrder;
		protected float _intersetValue; //Used for InOut mid point time calculation
		protected bool _hasInflection; //odd numbered polynomial orders will display a point of inflection where the curve will invert
	
		/** Used to determine the steepness of the timing curve.
		 As the value increases, so does the steepness/rate of the curve.
		 Default value is 6, gives a similar curve to EaseExponential.
		 Values less than 6, produces a softer ease action.
		 Values greater than 6, produces a more pronounced action.
		 @warning Value must be greater than 1
		 */
		public uint polynomialOrder{
			get { return _polynomialOrder; } 
			set {
				NSUtils.Assert(value>1, @"Polynomial order must be greater than 1");
				_polynomialOrder = value;
				_hasInflection = (value % 2 > 0);
				_intersetValue = Mathf.Pow(0.5f, 1.0f / value) / 0.5f;
			} 
		}

		public CCEasePolynomial(CCActionInterval action){
			initWithAction (action);
		}

		public void intWithAction(CCActionInterval action){
			base.initWithAction (action);
			_polynomialOrder = kDefaultPolynomial;
			_hasInflection = false;
			_intersetValue = 1.78179743628068f;
		}
	}

	/** CCEase Polynomial In
	 @since v2.1
	 */
	public class CCEasePolynomialIn : CCEasePolynomial
	{
		public CCEasePolynomialIn(CCActionInterval action):base(action){}
		
		// Needed for BridgeSupport
		public override void update (float dt)
		{
			_inner.update (Mathf.Pow (dt, _polynomialOrder));
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEasePolynomialIn (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			CCEasePolynomialOut action = new CCEasePolynomialOut (_inner.reverse ());
			if (_polynomialOrder != kDefaultPolynomial) {
				action.polynomialOrder = _polynomialOrder;
			}
			return action;
		}
	}

	/** Ease Polynomial Out
	 @since v2.1
	 */
	public class CCEasePolynomialOut : CCEasePolynomial
	{
		public CCEasePolynomialOut(CCActionInterval action):base(action){}
		
		// Needed for BridgeSupport
		public override void update (float t)
		{
			if (_hasInflection) {
				t = Mathf.Pow(t-1.0f, _polynomialOrder) + 1.0f;
			} else {
				t = -Mathf.Pow(t-1.0f, _polynomialOrder) + 1.0f;
			}
			
			_inner.update(t);
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEasePolynomialOut (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			CCEasePolynomialIn action = new CCEasePolynomialIn (_inner.reverse ());
			if (_polynomialOrder != kDefaultPolynomial) {
				action.polynomialOrder = _polynomialOrder;
			}
			return action;
		}
	}

	/** Ease Polynomial InOut
	 @since v2.1
	 */
	public class CCEasePolynomialInOut : CCEasePolynomial
	{
		public CCEasePolynomialInOut(CCActionInterval action):base(action){}
		
		// Needed for BridgeSupport
		public override void update (float t)
		{
			if (FloatUtils.Small(t , 0.5f)) {
				t = Mathf.Pow(t*_intersetValue, _polynomialOrder);
			} else {
				if (_hasInflection) {
					t = Mathf.Pow((t - 1.0f)*_intersetValue, _polynomialOrder) + 1.0f;
				} else {
					t = -Mathf.Pow((t - 1.0f)*_intersetValue, _polynomialOrder) + 1.0f;
				}
			}
			
			_inner.update(t);
		}
		
		protected override CCAction copyImpl ()
		{
			return new CCEasePolynomialInOut (_inner.copy());
		}
		
		protected override CCAction reverseImpl ()
		{
			CCEasePolynomialInOut action = new CCEasePolynomialInOut (_inner.reverse ());
			return action;
		}
	}


}
