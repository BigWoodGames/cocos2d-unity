using UnityEngine;
using System.Collections;

namespace BBGamelib.flash.imp{
	public interface DisplayFactory
	{
		Display ctDisplay(TagDefineDisplay define);
	}

	public class DisplayFactoryDefault{
		public Display ctDisplay(TagDefineDisplay define){
			if (define is TagDefineGraphic)
				return new Graphic (define as TagDefineGraphic);
			else if (define is TagDefineMovie)
			{
				Movie movie = new Movie (define as TagDefineMovie);
				movie.movieCtrl.start();
				return movie;
			}else 
				return null;
		}
	}
}
