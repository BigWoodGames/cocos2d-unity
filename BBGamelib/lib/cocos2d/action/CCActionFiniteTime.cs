using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** Base class actions that do have a finite time duration.
	 Possible actions:
	   - An action with a duration of 0 seconds
	   - An action with a duration of 35.5 seconds
	 Infinite time actions are valid
	 */
	public abstract class CCActionFiniteTime : CCAction
	{
		//! duration in seconds
		protected float _duration = 0;
		public float duration{get{return _duration;}}


		public new virtual CCActionFiniteTime copy(){
			CCActionFiniteTime act = (CCActionFiniteTime)copyImpl();
			NSUtils.Assert (act!=null, "{0}:copyImpl not implemented", GetType().Name);
			return act;
		}
		
		protected abstract CCAction reverseImpl();
		public virtual CCActionFiniteTime reverse(){
			CCActionFiniteTime act = (CCActionFiniteTime)reverseImpl();
			NSUtils.Assert (act!=null, "{0}:reverseImpl not implemented", GetType().Name);
			return act;
		}
	}
}
