using UnityEngine;
using System.Collections;

namespace BBGamelib.flash.imp{
	public interface DisplayFactory
	{
		Graphic createGraphic(DefineGraphic define);
		Movie createMovie(DefineMovie define);
	}

	public class DisplayFactoryDefault{
		public Graphic createGraphic(DefineGraphic define){
			return new Graphic (define);
		}
		public Movie createMovie(DefineMovie define){
			return new MovieImp (define);
		}
	}
}
