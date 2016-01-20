using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class CCSpawn : CCActionInterval
	{
		CCActionFiniteTime _one;
		CCActionFiniteTime _two;

		public static CCActionFiniteTime Actions(CCActionFiniteTime action1, params CCActionFiniteTime[] args){
			CCActionFiniteTime prev = action1;
			for(int i=0; i<args.Length; i++){
				CCActionFiniteTime now  = args[i];
				prev = ActionWithOneTwo(prev, now);		
			}
			return prev;
		}

		public static CCActionFiniteTime ActionWithArray(CCActionFiniteTime[] actions){
			NSUtils.Assert (actions != null && actions.Length > 0, "CCActionSequence: actions should not be null.");
			CCActionFiniteTime prev = actions [0];
			for (int i=1; i<actions.Length; i++)
				prev = ActionWithOneTwo (prev, actions [i]);
			return prev;
		}
		
		public static CCActionFiniteTime ActionWithOneTwo(CCActionFiniteTime one, CCActionFiniteTime two){
			CCSpawn act = new CCSpawn(one, two);
			return act;
		}

		protected CCSpawn(CCActionFiniteTime one, CCActionFiniteTime two){
			initWithAction (one, two);
		}

		public void initWithAction(CCActionFiniteTime one, CCActionFiniteTime two){
			NSUtils.Assert( one!=null && two!=null, "Sequence: arguments must be non-nil");
			NSUtils.Assert( one!=_one && one!=_two, "Spawn: reinit using same parameters is not supported");
			NSUtils.Assert( two!=_two && two!=_one, "Spawn: reinit using same parameters is not supported");

			float d1 = one.duration;
			float d2 = two.duration;

			base.initWithDuration (Mathf.Max (d1, d2));
			_one = one;
			_two = two;

			if (FloatUtils.Big(d1 , d2))
				_two = CCSequence.Actions (two, new CCDelayTime (d1 - d2));
			else if (d1 < d2)
				_one = CCSequence.Actions (one, new CCDelayTime (d2 - d1));
		}

		protected override CCAction copyImpl ()
		{
			CCAction act = new CCSpawn (_one.copy(), _two.copy());
			return act;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			_one.startWithTarget (_target);
			_two.startWithTarget (_target);
		}

		public override void stop ()
		{
			_one.stop ();
			_two.stop ();
			base.stop ();
		}

		public override void update (float dt)
		{
			_one.update (dt);
			_two.update (dt);
		}

		protected override CCAction reverseImpl ()
		{
			CCAction act = new CCSpawn (_one.reverse (), _two.reverse ());
			return act;
		}
	}
}

