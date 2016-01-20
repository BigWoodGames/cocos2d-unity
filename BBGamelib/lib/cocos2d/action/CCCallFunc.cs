using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
	
namespace BBGamelib{
    //
    // CallFunc
    //
    #region mark CCCallFunc
    public class CCCallFunc : CCActionInstant
	{
		System.Object _targetCallback; 
		Action _selector;
        
        /** Target that will be called */
        public System.Object targetCallback{set{_targetCallback = value;} get{return _targetCallback;}}


		/// -----------------------------------------------------------------------
		/// @name Creating a CCActionCallFunc Object
		/// -----------------------------------------------------------------------
		
		/**
		 *  Creates the action with the callback.
		 *
		 *  @param t Target the selector is sent to.
		 *  @param s Selector to execute.
		 *
		 *  @return The call func action object.
		 */
		public CCCallFunc(System.Object target, Action s){
			initWithTarget (target, s);		
		}

		/// -----------------------------------------------------------------------
		/// @name Initializing a CCActionCallFunc Object
		/// -----------------------------------------------------------------------
		
		/**
		 *  Initializes the action with the callback.
		 *
		 *  @param t Target the selector is sent to
		 *  @param s Selector to execute
		 *
		 *  @return An initialized call func action object.
		 */
		public void initWithTarget(System.Object target, Action m){
			base.init ();
			_targetCallback = target;
			_selector = m;
		}


		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | Tag = {2} | selector = {3}>",
			                      GetType().Name,
			                      GetHashCode(),
			                      _tag,
			                      _selector
			                      );
		}
		protected override CCAction copyImpl ()
		{
			CCCallFunc copy = new CCCallFunc (_targetCallback,_selector);
			return copy;
		}

		public override void update (float dt)
		{
			excute ();
		}
        
        /** executes the callback */
		void excute(){
			_selector.Invoke ();
		}
	}
    #endregion

	#region mark CCCallBlock
	public class CCCallBlock : CCActionInstant
	{
		Action _block;
		/// -----------------------------------------------------------------------
		/// @name Creating a CCActionCallBlock Object
		/// -----------------------------------------------------------------------
		
		/**
		 *  Creates the action with the specified block, to be used as a callback.
		 *  The block will be "copied".
		 *
		 *  @param block Block to execute.
		 *
		 *  @return The call block action object.
		 */
		public CCCallBlock(Action s){
			initWithBlock (s);		
		}
		
		
		/// -----------------------------------------------------------------------
		/// @name Initializing a CCActionCallBlock Object
		/// -----------------------------------------------------------------------
		
		/**
		 *  Initializes the action with the specified block, to be used as a callback.
		 *  The block will be "copied".
		 *
		 *  @param block Block to execute.
		 *
		 *  @return An initialized call block action object.
		 */
		public void initWithBlock(Action m){
			base.init ();
			_block = m;
		}

		protected override CCAction copyImpl ()
		{
			CCCallBlock copy = new CCCallBlock (_block);
			return copy;
		}
		
		public override void update (float dt)
		{
			excute ();
		}
		
		/** executes the callback */
		void excute(){
			_block.Invoke ();
		}
	}

	#endregion

	#region CCCallBlockN
	/** Executes a callback using a block with a single CCNode parameter. */
	public class CCCallBlockN : CCActionInstant{
		Action<CCNode> _block;

		/** creates the action with the specified block, to be used as a callback.
		 The block will be "copied".
		 */
		public CCCallBlockN(Action<CCNode> block){
			initWithBlock (block);	
		}

		/** initialized the action with the specified block, to be used as a callback.
		 The block will be "copied".
		 */	
		public void initWithBlock(Action<CCNode> m){
			base.init ();
			_block = m;
		}

		
		protected override CCAction copyImpl ()
		{
			CCCallBlockN copy = new CCCallBlockN (_block);
			return copy;
		}
		
		
		public override void update (float dt)
		{
			excute ();
		}
		/** executes the callback */
		void excute(){
			_block.Invoke ((CCNode)_target);
		}
	}
	#endregion

	#region CCCallBlockO
	/** Executes a callback using a block with a single CCNode parameter. */
	public class CCCallBlockO<T> : CCActionInstant{
		Action<T> _block;
		T _object;
		
		/** creates the action with the specified block, to be used as a callback.
		 The block will be "copied".
		 */
		public CCCallBlockO(Action<T> block, T obj){
			initWithBlock (block, obj);	
		}
		
		/** initialized the action with the specified block, to be used as a callback.
		 The block will be "copied".
		 */	
		public void initWithBlock(Action<T> m, T obj){
			base.init ();
			_block = m;
			_object = obj;
		}
		
		
		protected override CCAction copyImpl ()
		{
			CCCallBlockO<T> copy = new CCCallBlockO<T> (_block, _object);
			return copy;
		}
		
		
		public override void update (float dt)
		{
			excute ();
		}
		/** executes the callback */
		void excute(){
			_block.Invoke (_object);
		}
	}
	#endregion
}