using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public delegate ParseStateFn ParseStateFn();

public class Parser {
	public List<char> Backbuf = new(collection: new char[10]);

	public int Backpos = 9;

	public int Column;

	public Queue<Token> Elem = new();

	public int Line;

	public int Pos;

	public StreamReader Source;

	public ParseStateFn State;

	public Parser(Stream source) {
		this.Source = new StreamReader(
			stream: source,
			encoding: Encoding.UTF8,
			detectEncodingFromByteOrderMarks: true
		);
		this.State = this.VisitStart;
	}

	public Parser(string source) {
		this.Source = new StreamReader(
			stream: new MemoryStream(
				buffer: Encoding.UTF8.GetBytes(s: source)
			),
			encoding: Encoding.UTF8,
			detectEncodingFromByteOrderMarks: true
		);
		this.State = this.VisitStart;
	}

	public ParseStateFn UnexpectedEOF => this.LexError(error: "unexpected eof");

	public ParseResult Parse() {
		var lastPrefabKey = "";
		var lastAtomPropKey = "";
		var result = new ParseResult();
		for ( ; ; )
		{
			var token = this.Next();
			if (token is TokenError tokenError) {
				result.Error = tokenError.Error;
				return result;
			}
			if (token is TokenEOF) {
				return result;
			}
			if (token is TokenPrefab tokenPrefab) {
				result.Map.Prefab[key: tokenPrefab.Key] = new Prefab {
					Key = tokenPrefab.Key,
				};
				lastPrefabKey = tokenPrefab.Key;
			}
			else if (token is TokenAtom tokenAtom) {
				var prefab = result.Map.Prefab[key: lastPrefabKey];
				prefab.Atoms.Add(item: new Atom {
					Path = tokenAtom.Path,
				});
			}
			else if (token is TokenAtomPropKey tokenAtomPropkey) {
				var prefab = result.Map.Prefab[key: lastPrefabKey];
				var atom = prefab.Atoms.Last();
				atom.Props[key: tokenAtomPropkey.Key] = new AtomProp {
					Key = tokenAtomPropkey.Key,
				};
				lastAtomPropKey = tokenAtomPropkey.Key;
			}
			else if (token is TokenAtomPropVal tokenAtomPropVal) {
				var prefab = result.Map.Prefab[key: lastPrefabKey];
				var atom = prefab.Atoms.Last();
				var prop = atom.Props[key: lastAtomPropKey];
				prop.Value = tokenAtomPropVal.Val;
			}
			else if (token is TokenChunk tokenChunk) {
				var chunk = new Chunk {
					X = tokenChunk.X,
					Y = tokenChunk.Y,
					Z = tokenChunk.Z,
				};
				result.Map.Chunks.Add(item: chunk);
			}
			else if (token is TokenChunkRow tokenChunkRow) {
				var keysize = result.Map.Prefab.First().Key.Length;
				var chunk = result.Map.Chunks.Last();
				var row = new ChunkRow();
				tokenChunkRow.Data.Chunk(size: keysize).ToList().ForEach(action: prefab =>
					row.Prefab.Add(item: new string(value: prefab))
				);
				chunk.Rows.Add(item: row);
			}
			else {
				result.Error = new Exception(
					message: $"unexpected token '{token.GetType()}'"
				);
				return result;
			}
		}
	}

	public Token Next() {
		for ( ; ; )
		{
			if (this.Elem.Count > 0) {
				return this.Elem.Dequeue();
			}

			this.State = this.State();
		}
	}

	private bool Peek(out char dst) {
		char r;
		dst = char.MinValue;
		if (this.Seek(dst: out r)) {
			return true;
		}

		this.Backup();
		dst = r;
		return false;
	}

	private void Backup() {
		this.Backpos -= 1;
		this.Pos -= this.Backbuf[index: this.Backpos].ToString().Length;
		if (this.Backbuf[index: this.Backpos] == '\n') {
			this.Line--;
			this.Column = 0;
		}
	}

	private bool Seek(out char dst) {
		var r = new char[1];
		int w;
		var bbend = this.Backbuf.Count - 1;
		if (this.Backpos == bbend) {
			// we are at the end of the buffer such
			// that there is no more room to seek forward.
			// time to read a new rune from source.
			w = this.Source.Read(buffer: r);
			if (w == 0) {
				dst = char.MinValue;
				return true;
			}
			// shift the back buffer left by one rune and
			// add our new rune to the buffer. No need to
			// increment the backbuff position as we are
			// already at the end.                      
			this.Backbuf.Remove(item: this.Backbuf.First());
			this.Backbuf.Add(item: r[0]);
			// update our rune position in source and return
			// the requested rune to the caller
			this.Pos += w;
			dst = r[0];
		}
		else {
			// we are behind the end of the buffer such
			// that we can simply seek forward to the
			// next available rune. increment back buf pos,
			// calculate new rune pos, and return requested
			// rune to caller
			this.Backpos += 1;
			this.Pos += this.Backbuf[index: this.Backpos].ToString().Length;
			dst = this.Backbuf[index: this.Backpos];
		}
		if (dst == '\n') {
			this.Line++;
			this.Column = 0;
		}
		else {
			this.Column++;
		}
		return false;
	}

	private ParseStateFn LexError(string error) {
		return () => {
			this.Elem.Enqueue(item: new TokenError {
				Error = new Exception(message: error),
			});
			return this.LexError(error: error);
		};
	}

	private ParseStateFn LexEOF() {
		return () => {
			this.Elem.Enqueue(item: new TokenEOF());
			return this.LexEOF;
		};
	}

	private ParseStateFn VisitStart() {
		if (this.ConsumeWhitespace() || this.Peek(dst: out var r)) {
			return this.LexEOF;
		}
		return r switch {
			'/' => this.VisitLineComment(resume: this.VisitStart),
			'"' => this.VisitPrefab,
			'(' => this.VisitChunk,
			_ => this.LexError(error: $"unexpected rune '{r}'"),
		};
	}

	private ParseStateFn VisitLineComment(ParseStateFn resume) {
		return () => {
			char r;
			if (this.Seek(dst: out r) || this.Peek(dst: out r)) {
				return this.UnexpectedEOF;
			}
			if (r != '/') {
				return this.LexError(
					error: $"expecting line comment. unexpected rune '{r}' after initial '/'"
				);
			}

			this.Seek(dst: out r);
			for ( ; ; )
			{
				if (this.Seek(dst: out r)) {
					return this.UnexpectedEOF;
				}
				switch (r) {
					case '\n':
						return resume;
					default:
						continue;
				}
			}
		};
	}

	private ParseStateFn VisitPrefab() {
		char r;
		if (this.Seek(dst: out r)) {
			return this.UnexpectedEOF;
		}
		var prefab = new TokenPrefab();
		if (this.ConsumeStringLiteral(result: out var key)) {
			return this.UnexpectedEOF;
		}
		prefab.Key = key;
		if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
			return this.UnexpectedEOF;
		}
		if (r != '=') {
			return this.LexError(
				error: $"expecting '=' after prefab key declaration '{key}'"
			);
		}
		if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
			return this.UnexpectedEOF;
		}
		if (r != '(') {
			return this.LexError(
				error: $"expecting prefab block start '(' after prefab key declaration '{key}'"
			);
		}
		if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
			return this.UnexpectedEOF;
		}
		if (r != '/') {
			return this.LexError(error: "expecting prefab atom path start '/'");
		}

		this.Backup();
		this.Elem.Enqueue(item: prefab);
		return this.VisitPrefabAtom;
	}

	private ParseStateFn VisitPrefabAtom() {
		char r;
		var atom = new TokenAtom();
		for ( ; ; )
		{
			if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
				return this.UnexpectedEOF;
			}
			if (r == ',') {
				if (atom.Path != "") {
					this.Elem.Enqueue(item: atom);
				}
				return this.VisitPrefabAtom;
			}
			if (r == ')') {
				this.Elem.Enqueue(item: atom);
				return this.VisitStart;
			}
			if (r == '{') {
				this.Elem.Enqueue(item: atom);
				return this.VisitAtomProps(resume: this.VisitPrefabAtom);
			}
			atom.Path += r;
		}
	}

	private ParseStateFn VisitAtomProps(ParseStateFn resume) {
		return () => {
			char r;
			var prop = new TokenAtomPropKey();
			for ( ; ; )
			{
				if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
					return this.UnexpectedEOF;
				}
				if (r == '}') {
					if (prop.Key != "") {
						this.Elem.Enqueue(item: prop);
					}
					return resume;
				}
				if (r == ';') {
					if (prop.Key != "") {
						this.Elem.Enqueue(item: prop);
					}
					return this.VisitAtomProps(resume: resume);
				}
				if (r == '=') {
					this.Elem.Enqueue(item: prop);
					return this.VisitAtomPropType(resume: this.VisitAtomProps(resume: resume));
				}
				prop.Key += r;
			}
		};
	}

	private ParseStateFn VisitAtomPropType(ParseStateFn resume) {
		return () => {
			char r;
			var propval = new TokenAtomPropVal();
			if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
				return this.UnexpectedEOF;
			}
			if (r == '"') {
				if (this.ConsumeStringLiteral(result: out var strval)) {
					return this.UnexpectedEOF;
				}
				propval.Val = strval;
				this.Elem.Enqueue(item: propval);
				return resume;
			}
			if (r == '-' || char.IsNumber(c: r)) {
				this.Backup();
				if (this.ConsumeNumberLiteral(result: out var result, error: out var error)) {
					return this.UnexpectedEOF;
				}
				if (error != null) {
					return this.LexError(error: error.Message);
				}
				propval.Val = result;
				this.Elem.Enqueue(item: propval);
				return resume;
			}
			if (r == 'l') {
				this.Backup();
				if (this.ConsumeListLiteral(result: out var result, error: out var error)) {
					return this.UnexpectedEOF;
				}
				if (error != null) {
					return this.LexError(error: error.Message);
				}
				propval.Val = result;
				this.Elem.Enqueue(item: propval);
				return resume;
			}
			if (r == ';') {
				this.Elem.Enqueue(item: propval);
				return resume;
			}
			return resume;
		};
	}

	private ParseStateFn VisitChunk() {
		char r;
		if (this.Seek(dst: out r)) {
			return this.UnexpectedEOF;
		}
		var chunk = new TokenChunk();
		var idx = "";
		var dims = 1;
		for ( ; ; )
		{
			if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
				return this.UnexpectedEOF;
			}
			if (r == ',' || r == ')') {
				int.TryParse(s: idx, result: out var i);
				switch (dims) {
					case 1:
						chunk.X = i;
						break;
					case 2:
						chunk.Y = i;
						break;
					case 3:
						chunk.Z = i;
						break;
					default:
						return this.LexError(error: $"chunk idx out of range '{i}'");
				}
				idx = "";
				dims++;
			}
			if (r == ')') {
				this.Elem.Enqueue(item: chunk);
				break;
			}
			if (r == ',') {
				continue;
			}
			if (char.IsNumber(c: r)) {
				idx += r;
				continue;
			}
			return this.LexError(
				error: $"expected number when parsing dimension got '{r}'"
			);
		}
		if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
			return this.UnexpectedEOF;
		}
		if (r != '=') {
			return this.LexError(
				error: "expecting equals after chunk declaration"
			);
		}
		if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
			return this.UnexpectedEOF;
		}
		if (r != '{') {
			return this.LexError(
				error: $"expecting start of chunk block but recieved '{r}'"
			);
		}
		if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
			return this.UnexpectedEOF;
		}
		if (r != '"') {
			return this.LexError(
				error: $"expecting start of chunk body but recieved '{r}'"
			);
		}
		if (this.ConsumeWhitespace()) {
			return this.UnexpectedEOF;
		}
		return this.VisitChunkRow;
	}

	private ParseStateFn VisitChunkRow() {
		char r;
		var chunkrow = new TokenChunkRow();
		for ( ; ; )
		{
			if (this.Seek(dst: out r)) {
				return this.UnexpectedEOF;
			}
			if (char.IsWhiteSpace(c: r)) {
				if (this.ConsumeWhitespace()) {
					return this.UnexpectedEOF;
				}

				this.Elem.Enqueue(item: chunkrow);
				return this.VisitChunkRow;
			}
			if (r == '"') {
				break;
			}
			chunkrow.Data += r;
		}
		if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
			return this.UnexpectedEOF;
		}
		if (r != '}') {
			return this.LexError(
				error: $"expecting end of chunk block but recieved '{r}'"
			);
		}
		return this.VisitStart;
	}

	private bool ConsumeWhitespace() {
		char r;
		for ( ; ; )
		{
			if (this.Peek(dst: out r)) {
				return true;
			}
			if (char.IsWhiteSpace(c: r)) {
				this.Seek(dst: out r);
				continue;
			}
			break;
		}
		return false;
	}

	private bool ConsumeStringLiteral(out string result) {
		char r;
		var val = "";
		for ( ; ; )
		{
			if (this.Seek(dst: out r)) {
				result = val;
				return true;
			}
			if (r == '"') {
				break;
			}
			val += r.ToString();
		}
		result = val;
		return false;
	}

	private bool ConsumeNumberLiteral(out object result, out Exception error) {
		char r;
		var strval = "";
		var isfloat = false;
		for ( ; ; )
		{
			if (this.Seek(dst: out r)) {
				result = "";
				error = null;
				return true;
			}
			if (r != '-' && r != '.' && !char.IsNumber(c: r)) {
				this.Backup();
				if (isfloat) {
					var valid = double.TryParse(s: strval, result: out var f);
					if (!valid) {
						error = new Exception(message: $"failed to parse float from '{strval}'");
						result = "";
						return false;
					}
					result = f;
				}
				else {
					var valid = int.TryParse(s: strval, result: out var i);
					if (!valid) {
						error = new Exception(message: $"failed to parse int from '{strval}'");
						result = "";
						return false;
					}
					result = i;
				}
				error = null;
				return false;
			}
			if (r == '.') {
				isfloat = true;
			}
			strval += r.ToString();
		}
	}

	private bool ConsumeListLiteral(out Dictionary<object, object> result, out Exception error) {
		char r;
		result = new Dictionary<object, object>();
		if (this.ConsumeSeq(seq: "list", error: out error)) {
			return true;
		}
		if (error != null) {
			return false;
		}
		if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
			return true;
		}
		if (r != '(') {
			error = new Exception(
				message: $"expecting list opening paren '(' got '{r}'"
			);
			return false;
		}
		var items = new Dictionary<object, object>();
		(object Key, object Value) element = new();
		var writekey = (object v) => element.Key = v;
		var writeval = (object v) => element.Value = v;
		var write = writekey;
		var flushelem = false;
		var done = false;
		for ( ; ; )
		{
			if (flushelem) {
				var maxidx = -1;
				foreach (var entry in items) {
					if (entry.Key is int ei) {
						if (ei > maxidx) {
							maxidx = ei;
						}
					}
				}
				if (element.Key != null && element.Value == null) {
					items[key: maxidx + 1] = element.Key;
				}
				if (element.Key != null && element.Value != null) {
					items[key: element.Key] = element.Value;
				}
				write = writekey;
				flushelem = false;
				element = new ValueTuple<object, object>();
			}
			if (done) {
				break;
			}
			if (this.ConsumeWhitespace() || this.Seek(dst: out r)) {
				return true;
			}
			if (r == ',') {
				flushelem = true;
				continue;
			}
			if (r == ')') {
				flushelem = true;
				done = true;
				continue;
			}
			if (r == '=') {
				write = writeval;
				continue;
			}
			if (r == '"') {
				if (this.ConsumeStringLiteral(result: out var strval)) {
					return true;
				}
				write(arg: strval);
				continue;
			}
			if (r == '-' || char.IsNumber(c: r)) {
				this.Backup();
				if (this.ConsumeNumberLiteral(result: out var numval, error: out error)) {
					return true;
				}
				if (error != null) {
					return false;
				}
				write(arg: numval);
				continue;
			}
			if (r == '/') {
				this.Backup();
				if (this.ConsumeAtomPathLiteral(result: out var pathval, error: out error)) {
					return true;
				}
				if (error != null) {
					return false;
				}
				write(arg: pathval);
				continue;
			}
			if (char.IsLetterOrDigit(c: r)) {
				this.Backup();
				var strval = "";
				for ( ; ; )
				{
					if (this.Seek(dst: out r)) {
						return true;
					}
					if (r == '_' || char.IsLetter(c: r) || char.IsNumber(c: r)) {
						strval += r;
						continue;
					}
					break;
				}
				write(arg: strval);
				continue;
			}
			error = new Exception(
				message: $"unexpected rune '{r}' when parsing list item"
			);
		}
		result = items;
		return false;
	}

	private bool ConsumeSeq(string seq, out Exception error) {
		char r;
		var strval = "";
		error = null;
		foreach (var ch in seq) {
			if (this.Seek(dst: out r)) {
				return true;
			}
			strval += ch.ToString();
			if (strval == seq) {
				return false;
			}
			if (seq.Length == strval.Length && strval != seq) {
				error = new Exception(
					message: $"failed to consume sequence '{seq}' instead consumed '{strval}'"
				);
				return false;
			}
		}
		return false;
	}

	private bool ConsumeAtomPathLiteral(out string result, out Exception error) {
		char r;
		result = "";
		error = null;
		for ( ; ; )
		{
			if (this.Seek(dst: out r)) {
				return true;
			}
			if (r == '/' || r == '_' || char.IsLetterOrDigit(c: r)) {
				result += r;
				continue;
			}

			this.Backup();
			break;
		}
		return false;
	}

	public class ParseResult {
		public Map Map { get; set; } = new();
		public Exception Error { get; set; }
	}

	public interface Token {
	}

	public class TokenPrefab : Token {
		public string Key { get; set; } = string.Empty;
	}

	public class TokenAtom : Token {
		public string Path { get; set; } = string.Empty;
	}

	public class TokenAtomPropKey : Token {
		public string Key { get; set; } = string.Empty;
	}

	public class TokenAtomPropVal : Token {
		public object Val { get; set; }
	}

	public class TokenChunk : Token {
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
	}

	public class TokenChunkRow : Token {
		public string Data { get; set; } = string.Empty;
	}

	public class TokenEOF : Token {
	}

	public class TokenError : Token {
		public Exception Error { get; set; }
	}

	public class Map {
		public Dictionary<string, Prefab> Prefab { get; set; } = new();
		public List<Chunk> Chunks { get; set; } = new();
	}

	public class Prefab {
		public string Key { get; set; } = string.Empty;
		public List<Atom> Atoms { get; set; } = new();
	}

	public class Atom {
		public string Path { get; set; } = string.Empty;
		public Dictionary<string, AtomProp> Props { get; set; } = new();
	}

	public class AtomProp {
		public string Key { get; set; } = string.Empty;
		public object Value { get; set; }
	}

	public class Chunk {
		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public List<ChunkRow> Rows { get; set; } = new();
	}

	public class ChunkRow {
		public List<string> Prefab { get; set; } = new();
	}
}