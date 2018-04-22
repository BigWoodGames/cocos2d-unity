using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace BBGamelib{
	public interface BBFlash
	{
		BBFlashMovie ctMovie(string className);
		BBFlashGraphic ctGraphic(string className);
		int frameRate{ get; }
		int flashVersion{ get;}
		bool hasChild(string className);
		void Debug();
	}
}
