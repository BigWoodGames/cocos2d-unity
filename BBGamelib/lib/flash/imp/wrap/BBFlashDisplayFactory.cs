using UnityEngine;
using System.Collections;
using BBGamelib.flash.imp;

namespace BBGamelib.flash.wrap{
	public class BBFlashDisplayFactory : DisplayFactory
	{
		public Graphic createGraphic(DefineGraphic define){
			return new BBFlashGraphic (define);
		}
		public Movie createMovie(DefineMovie define){
			return new BBFlashMovieImp (define);
		}
	}
}

