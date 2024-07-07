using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace QESilver.IO
{
	class Tokenizer
	{
		private String _str;
		public Tokenizer(String str) { _str = str; }

		private String _splits = "";
		public void SplitBy(char ch) { _splits += ch; }
		public void SplitParens() { _splits += "()"; }
		public void SplitCode() { _splits += "(),;"; }
		public void SplitFGD() { _splits += "[](),=:"; }

		private int _line, _offset;
		private int _posCur, _lenCur;
		private int _posPrev, _lenPrev;
		public int CurrentLine { get { return _line; } }
		public int CurrentPosition { get { return _posCur; } }
		public bool AtEnd { get { return _offset == _str.Length; } }

		public ReadOnlySpan<char> Next()
		{
			int next, nl;
			char closeQuote;
			var strspan = _str.AsSpan();

			_currentInParens = false;
			_currentInQuotes = false;

			while (_offset < _str.Length)
			{
				if (Char.IsWhiteSpace(_str[_offset]))
				{
					if (_str[_offset] == '\n')
						++_line;
					++_offset;
					continue;
				}
				closeQuote = '\0';

				// skip comments
				if (_str[_offset] == '/' && _str[_offset + 1] == '/')
				{
					nl = _str.IndexOf('\n', _offset + 2);
					if (nl == -1)
					{
						_offset = _str.Length;  // eof in comment
						return strspan.Slice(_offset, 0);
					}
					_offset = nl;
					continue;
				}

				// grab quoted strings as single tokens
				if (_dblQuotedTokens && _str[_offset] == '\"')
				{
					_currentInQuotes = true;
					closeQuote = '\"';
				}
				if (_parenTokens && _str[_offset] == '(')
				{
					_currentInParens = true;
					closeQuote = ')';
				}

				if (closeQuote != '\0')
				{
					next = _str.IndexOf(closeQuote, _offset + 1);
					if (next == -1)
					{
						_currentInParens = false;
						_currentInQuotes = false;
						throw new Exception(String.Format(
							"Quoted token is incomplete on line {1} (EOF): {0}\n", 
							_str.Substring(_offset, _str.Length - _offset), _line));
					}
					if (!_newlinesInTokens)
					{
						nl = _str.IndexOf('\n', _offset + 1);
						if (nl != -1 && nl < next)
						{
							_currentInParens = false;
							_currentInQuotes = false;
							throw new Exception(String.Format(
								"Quoted token is incomplete on line {1} (EOL): {0}\n", 
								_str.Substring(_offset, nl - _offset), _line));
						}
					}
					return Advance(_offset + 1, next, next + 1);
				}

				// treat special characters as delimiters regardless of whitespace
				if (!IsWordChar(_str[_offset]))
				{
					return Advance(_offset, _offset + 1, _offset + 1);
				}

				next = _offset;
				while (next < _str.Length && IsWordChar(_str[next]))
					++next;
				return Advance(_offset, next, next);
			}

			return strspan.Slice(_offset, 0);
		}

		private bool IsWordChar(char c)
		{
			return (!Char.IsWhiteSpace(c) && !_splits.Contains(c));
		}

		private ReadOnlySpan<char> Advance(int start, int end, int newofs)
		{
			_posPrev = _posCur;
			_lenPrev = _lenCur;
			_posCur = start;
			_lenCur = end - start;
			_offset = newofs;
			return Current;
		}

		public ReadOnlySpan<char> Current { get { return _str.AsSpan().Slice(_posCur, _lenCur); } }
		public ReadOnlySpan<char> Prev { get { return _str.AsSpan().Slice(_posPrev, _lenPrev); } }

		public bool CurrentMatches(string s)
		{
			return MemoryExtensions.Equals(_str.AsSpan().Slice(_posCur, _lenCur), s, StringComparison.Ordinal);
		}
		public bool CurrentMatches(char c)
		{
			if (_lenCur != 1) return false;
			return _str[_posCur] == c;
		}

		private bool _parenTokens = false;
		private bool _dblQuotedTokens = true;
		private bool _currentInQuotes = false;
		private bool _currentInParens = false;
		private bool _newlinesInTokens = false;
		public bool DoubleQuotedTokens { get { return _dblQuotedTokens; } set { _dblQuotedTokens = value; } }
		public bool ParentheticalTokens { get { return _parenTokens; } set { _parenTokens = value; } }
		public bool NewlinesInTokens { get { return _newlinesInTokens; } set { _newlinesInTokens = value; } }
		public bool CurrentInQuotes { get { return _currentInQuotes; } }
		public bool CurrentInParens { get { return _currentInParens; } }
	}
}
