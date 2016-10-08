using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BBGamelib.flash.imp
{
	public abstract class Display : CCNodeRGBA
	{
		public abstract TagDefineDisplay define{ get; }
		public abstract Rect getBounds();

		protected string _instanceName;
		public string instanceName{ get { return _instanceName; } set { _instanceName = value; } }

		public Display(){
			this.cascadeColorEnabled = true;
			this.cascadeOpacityEnabled = true;
		}
	}
}
