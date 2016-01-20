using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BBGamelib.flash.imp{
	public class Graphic : DisplayObjectImp
	{
		DefineGraphic _define;
		CCSprite _view;
    
        public Graphic(DefineGraphic define){
			_define = define;
			if(_define.className != null)
				_view = new CCSprite(string.Format("{0}_{1}.png", define.flash.prefix, _define.className));
			else
				_view = new CCSprite(string.Format("{0}_ID{1}.png", define.flash.prefix, _define.characterId));
			_view.cascadeColorEnabled = true;
			_view.cascadeOpacityEnabled = true;
			_view.anchorPoint = _define.anchorPoint;
			_view.gameObject.name = define.characterId.ToString();
			_view.opacityModifyRGB = false;
        }
        

		#region implements DisplayObject
		public override int characterId{ get{return _define.characterId;}}
		public override CCNodeRGBA view{get{return _view;}}
		public override Rect bounds{ get{
				Rect rect = view.boundingBox;
				rect.position = -_view.anchorPointInPixels;
				return rect;
			}
		}
		public override string className{ get{return _define.className;}}
		public override float fps {
			get {
				return _define.flash.frameRate;
			}
			set{
				CCDebug.Warning("BBGamelib:flash: Do nothing to setting a graphic's fps.");
			}
		}
		public override kTweenMode tweenMode{ get{return kTweenMode.SkipFrames;} 
			set{
				CCDebug.Warning("BBGamelib:flash: Do nothing to setting a graphic's tweenMode.");
			}
		}
		#endregion
	}
}

