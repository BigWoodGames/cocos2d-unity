using UnityEngine;
using System.Collections;
using BBGamelib.flash.imp;


namespace BBGamelib.flash.wrap{
	public class BBFlashImp : BBFlash
	{
		imp.Flash flash;
		
		public BBFlashImp(string path){
			flash = new imp.Flash (path, new BBFlashDisplayFactory()); 
		}
		
		public int frameRate{ get{return flash.frameRate;} }
		public int flashVersion{ get{return flash.flashVersion;}}


		public BBFlashMovie ctMovie(string className){
			TagDefineMovie define = flash.getDefine (className) as TagDefineMovie;
			if (define == null)
				return null;
			flash.imp.Display displayObject = flash.displayFactory.ctDisplay (define);
			return displayObject as BBFlashMovie;
		}

		public BBFlashGraphic ctGraphic(string className){
			TagDefineGraphic define = flash.getDefine (className) as TagDefineGraphic;
			if (define == null)
				return null;
			flash.imp.Display displayObject = flash.displayFactory.ctDisplay (define);
			return displayObject as BBFlashGraphic;
		}

		public bool hasChild(string className){
			TagDefineDisplay define = flash.getDefine (className) as TagDefineDisplay;
			return define != null;
		}
		
		public void Debug(){
			string s = flash.trace ();
			CCDebug.Log (s);
		}
	}
}
