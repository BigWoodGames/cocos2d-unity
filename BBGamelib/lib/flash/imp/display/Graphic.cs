using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BBGamelib.flash.imp{
	public class Graphic : Display
	{
		
		// ------------------------------------------------------------------------------
		//  ctor
		// ------------------------------------------------------------------------------
		TagDefineGraphic _define;
		CCSprite _view;
		public Graphic(TagDefineGraphic define){
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
			addChild (_view);
            if (string.IsNullOrEmpty(define.className))
            {
                this.nameInHierarchy = define.characterId.ToString();
            } else
            {
                this.nameInHierarchy = define.className;
            }
        }
        
		// ------------------------------------------------------------------------------
		//  implements
		// ------------------------------------------------------------------------------
		public override TagDefineDisplay define {
			get {
				return _define;
			}
		}

		public override Rect getBounds(){
			Rect rect = _view.boundingBox;
			rect.position = -_view.anchorPointInPixels;
			return rect;
		}



	}
}

