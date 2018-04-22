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

        protected bool _removed;
        public virtual bool removed{ get { return _removed; } set { _removed = value; } }

        //user setting
        protected bool _hasUserVisible;
        protected bool _userVisible;
        protected bool _hasUserColorTransform;
        protected ColorTransform _userColorTransform;

        public bool hasUserVisible{ get { return _hasUserVisible; } set { _hasUserVisible = value; } }
        public bool userVisible{ get { return _userVisible; } set { _userVisible = value; } }
        public bool hasUserColorTransform{ get { return _hasUserColorTransform; } set { _hasUserColorTransform = value; } }
        public ColorTransform userColorTransform{ get { return _userColorTransform; } set { _userColorTransform = value; } }

		public Display(){
            _removed = false;
            _hasUserVisible = false;
            _userVisible = true;
            _hasUserColorTransform = false;
            _userColorTransform = ColorTransform.Default;
		}
	}
}
