using UnityEngine;
using System.Collections;
using BBGamelib.flash.imp;

namespace BBGamelib.flash{
	public class BBFlashDisplayFactory : DisplayFactory
	{
		public flash.imp.Display ctDisplay(TagDefineDisplay define){
			if (define is TagDefineGraphic)
				return new BBFlashGraphic (define as TagDefineGraphic);
			else if (define is TagDefineMovie)
			{
				BBFlashMovie movie = new BBFlashMovie (define as TagDefineMovie);
				return movie;
			}else 
				return null;
		}
	}
}

