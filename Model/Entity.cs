using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QESilver.Model
{
    class Entity
    {
		public List<Brush> Brushes;

		public Entity()
		{
			Brushes = new List<Brush>();
		}

		public void Add(Brush b)
		{
			if (b.Owner == this)
				return;
			b.Owner = this;
			Brushes.Add(b);
		}
    }
}
