using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** Base class for CCAction objects.*/
	public abstract class CCAction : NSCopying<CCAction>
	{
		//! Default tag

		public const int kCCActionTagInvalid = -1;


		protected System.Object _target = null;
		protected System.Object _originalTarget = null;
		protected int _tag = kCCActionTagInvalid;
		
		/** The "target". The action will modify the target properties.
         The target will be set with the 'startWithTarget' method.
         When the 'stop' method is called, target will be set to nil.
         The target is 'assigned', it is not 'retained'.
         */
		public System.Object target{get{return _target;}}
		
		/** The original target, since target can be nil.
         Is the target that were used to run the action. Unless you are doing something complex, like CCActionManager, you should NOT call this method.
         @since v0.8.2
        */
		public System.Object originalTarget{get{return _originalTarget;}}
		
		
		/** The action tag. An identifier of the action */
		public int tag{ set{ _tag = value;} get{return _tag;}}


		public virtual void init(){
			_originalTarget = _target = null;
			_tag = kCCActionTagInvalid;
		}

		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | Tag = {2}>", this.GetType().Name, this.GetHashCode(), _tag);
		}

		public virtual void startWithTarget(System.Object aTarget){
			_originalTarget = _target = aTarget;
		}
		public virtual void stop(){
			_target = null;
		}
		public virtual bool isDone(){
			return true;
		}
		public virtual void step(float dt){
			CCDebug.Log ("[Action step]. override me");
		}
		
		public virtual void update(float dt){
			CCDebug.Log ("[Action update]. override me");
		}
		
		protected abstract CCAction copyImpl();
		public virtual CCAction copy(){
			return copyImpl();
		}

	}
}

