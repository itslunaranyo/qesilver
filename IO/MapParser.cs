using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QESilver.Model;

namespace QESilver.IO
{
    abstract class MapParser
    {
		public MapParser(String mapdata)
		{
			_tokr = new Tokenizer(mapdata);
		}
		public abstract void Read(ref List<Brush> blist, ref List<Entity> elist);


		protected Tokenizer _tokr;
		protected abstract void ParseEntity(ref List<Brush> blist, ref List<Entity> elist);
		protected abstract void ParseEPair(Entity e);
		protected abstract void ParseBrush(ref List<Brush> blist, ref Entity e);
		protected abstract void ParseFace(ref Brush b);
	}
}
