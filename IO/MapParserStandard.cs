using QESilver.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using System.Security.Cryptography;

namespace QESilver.IO
{
	class MapParserStandard : MapParser
	{
		public MapParserStandard(string mapdata) : base(mapdata)
		{
		}

		public override void Read(ref List<Brush> blist, ref List<Entity> elist)
		{
			while (_tokr.Next().Length > 0)
			{
				if (!_tokr.CurrentMatches('{'))
					throw new Exception(String.Format(
						"Couldn't parse map file (expected {{ on line {0}, got {1})", 
						_tokr.CurrentLine.ToString(), _tokr.Current.ToString()));

				ParseEntity(ref blist, ref elist);
			}
		}

		protected override void ParseBrush(ref List<Brush> blist, ref Entity e)
		{
			Brush b = new();

			_tokr.ParentheticalTokens = true;
			while (!_tokr.Next().IsEmpty)
			{
				if (_tokr.CurrentMatches('}'))
					break;
				ParseFace(ref b);
			}
			_tokr.ParentheticalTokens = false;

			// add to the end of the entity chain
			e.Add(b);
			blist.Add(b);
		}

		protected override void ParseEntity(ref List<Brush> blist, ref List<Entity> elist)
		{
			Entity e = new Entity();

			while (_tokr.Next().Length > 0)
			{
				if (_tokr.CurrentMatches('{'))
				{
					// it's a brush
					ParseBrush(ref blist, ref e);
				}
				else if (_tokr.CurrentMatches('}'))
				{
					// entity is over
					//e.SetSpawnflagFilter();
					elist.Add(e);
					return;
				}
				else
				{
					// keyvalue
					ParseEPair(e);
				}
			}
			throw new Exception(String.Format(
				"Couldn't parse map file (EOF before closing }} on line {0})",
				_tokr.CurrentLine.ToString()));
		}

		protected override void ParseEPair(Entity e)
		{
			// TODO: all this verification should be done by the tokenizer by passing it expectations
			if (!_tokr.CurrentInQuotes)
			{
				throw new Exception(String.Format(
					"Couldn't parse map file (expected quoted key on line {0}, got {1})",
					_tokr.CurrentLine.ToString(), _tokr.Current.ToString()));
			}
			else if (_tokr.Current.IsEmpty)
			{
				Debug.Print(String.Format(	// warning
					"Empty quoted key on line {0}",
					_tokr.CurrentLine.ToString()));
				return;
			}
			
			var key = _tokr.Current;
			var val = _tokr.Next();

			if (val.IsEmpty)
			{
				if (_tokr.CurrentInQuotes)
				{
					Debug.Print(String.Format(  // warning
						"Empty quoted value on line {0}", 
						_tokr.CurrentLine));
					return;
				}
				throw new Exception(String.Format(
					"Couldn't parse map file (EOF before quoted value on line {0})", 
					_tokr.CurrentLine));
			}
			else if (!_tokr.CurrentInQuotes)
			{
				throw new Exception(String.Format(
					"Couldn't parse map file (expected quoted value on line {0}, got {1})", 
					_tokr.CurrentLine, _tokr.Current.ToString()));
			}

			// create directly for speed rather than use SetKeyvalue, which checks for duplicates
			//EPair* ep = new EPair(key, val);
			//ep->next = e.epairs;
			//e.epairs = ep;
		}

		protected override void ParseFace(ref Brush b)
		{
			Face f = new();
			var ftk = _tokr.Current;

			// read the three point plane definition
			for (int i = 0; i < 3; ++i)
			{
				if (!_tokr.CurrentInParens || !Utility.TryParse(_tokr.Current, ref f.Plane.Points[i]))
				{
					throw new Exception(String.Format(
						"Couldn't parse map (expected plane point '{0}' on line {1})",
						_tokr.Current.ToString(), _tokr.CurrentLine));
				}
				_tokr.Next();
			}
			f.Plane.Make();

			// current token should be a texture name
			//f->texdef.SetTemp(_tokr.Current);  // apply directly without setting because no wads are loaded yet

			ftk = _tokr.Next();

			if (_tokr.CurrentMatches('['))
				throw new Exception("Couldn't parse map (no [ expected in standard map format)");

			Vector2 scale, shift;

			// TODO: error check any of this
		//	shift.x = std::stof(ftk);
			ftk = _tokr.Next();
		//	shift.y = std::stof(ftk);
			ftk = _tokr.Next();
		//	f->texdef.SetRotation(std::stof(ftk));
			ftk = _tokr.Next();
		//	scale.x = std::stof(ftk);
			ftk = _tokr.Next();
			//	scale.y = std::stof(ftk);

			//	f->texdef.SetShift(shift);
			//	f->texdef.SetScale(scale);

			b.AddFace(f);
		}
	}
}
