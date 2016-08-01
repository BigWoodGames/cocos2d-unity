using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib.scheduler{
	public delegate void TICK_IMP(float ccTime); 
	// A list double-linked list used for "updates with priority"
	public class tListEntry{
		public TICK_IMP impMethod;
		public System.Object target;
		public int priority;
		public bool paused;
		public bool markedForDeletion;
	}

	public class tHashUpdateEntry{
		public utList<tListEntry> list;
		public utNode<tListEntry> entry;
		public System.Object target;
	}

	public class tHashTimerEntry{
		public List<CCTimer> timers;
		public System.Object target;
		public int timerIndex;
		public CCTimer currentTimer;
		public bool currentTimerSalvaged;
		public bool paused;
	}
}