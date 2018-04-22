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
            this.cascadeColorEnabled = false;
            this.cascadeOpacityEnabled = false;
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

        public override ColorTransform colorTransform
        {
            get
            {
                return base.colorTransform;
            }
            set
            {
                if ((value.add.r + value.add.g + value.add.b != 0)||
                    (value.tint.r + value.tint.g + value.tint.b != 765))
                {
                    this.cascadeColorEnabled = true;                    
                }
                base.colorTransform = value;
            }
        }

        public override OpacityTransform opacityTransform
        {
            get
            {
                return base.opacityTransform;
            }
            set
			{
				if(value.tint!=255 || value.add != 0)
                    this.cascadeOpacityEnabled = true;
                base.opacityTransform = value;
            }
        }

		public override void updateDisplayedColor (ColorTransform parentColor)
		{
			if ((parentColor.add.r + parentColor.add.g + parentColor.add.b != 0)||
				(parentColor.tint.r + parentColor.tint.g + parentColor.tint.b != 765))
			{
				this.cascadeColorEnabled = true;                    
			}
			base.updateDisplayedColor (parentColor);
		}

		public override void updateDisplayedOpacity (OpacityTransform parentOpacity)
		{
			if(parentOpacity.tint!=255 || parentOpacity.add != 0)
				this.cascadeOpacityEnabled = true;
			base.updateDisplayedOpacity (parentOpacity);
		}
	}
}

