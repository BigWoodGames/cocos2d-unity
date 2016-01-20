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


		public BBFlashMovie createMovie(string className){
			Define define = flash.getDefine (className);
			if (define == null)
				return null;
			DisplayObject displayObject = define.createDisplay ();
			return displayObject as BBFlashMovie;
		}

		public bool hasMovie(string className){
			Define define = flash.getDefine (className);
			return define != null;
		}
		
		public void Debug(){
			string s = flash.trace ();
			CCDebug.Log (s);
		}
	}
}
