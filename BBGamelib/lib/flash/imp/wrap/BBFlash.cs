using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace BBGamelib{
	public interface BBFlash
	{
		BBFlashMovie createMovie(string className);
		int frameRate{ get; }
		int flashVersion{ get;}
		bool hasMovie(string className);
		void Debug();
	}
}
