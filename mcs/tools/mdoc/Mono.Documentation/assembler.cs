//
// The assembler: Help compiler.
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) 2003 Ximian, Inc.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Monodoc;
using Mono.Options;

namespace Mono.Documentation {
	
public class MDocAssembler : MDocCommand {
	public override void Run (IEnumerable<string> args)
	{
		string[] validFormats = {
			"ecma", 
			"ecmaspec", 
			"error", 
			"hb", 
			"man", 
			"simple", 
			"xhtml"
		};
		var formats = new Dictionary<string, List<string>> ();
		string prefix = "tree";
		string cur_format = "ecma";
		var options = new OptionSet () {
			{ "f|format=",
				"The documentation {FORMAT} used in DIRECTORIES.  " + 
					"Valid formats include:\n  " +
					string.Join ("\n  ", validFormats) + "\n" +
					"If not specified, the default format is `ecma'.",
				v => {
					if (Array.IndexOf (validFormats, v) < 0)
						Error ("Invalid documentation format: {0}.", v);
					cur_format = v;
				} },
			{ "o|out=",
				"Provides the output file prefix; the files {PREFIX}.zip and " + 
					"{PREFIX}.tree will be created.\n" +
					"If not specified, `tree' is the default PREFIX.",
				v => prefix = v },
			{ "<>", v => AddFormat (formats, cur_format, v) },
		};
		List<string> extra = Parse (options, args, "assemble", 
				"[OPTIONS]+ DIRECTORIES",
				"Assemble documentation within DIRECTORIES for use within the monodoc browser.");
		if (extra == null)
			return;

		List<Provider> list = new List<Provider> ();
		EcmaProvider ecma = null;
		bool sort = false;
		
		foreach (string format in formats.Keys) {
			switch (format) {
			case "ecma":
				if (ecma == null) {
					ecma = new EcmaProvider ();
					list.Add (ecma);
					sort = true;
				}
				foreach (string dir in formats [format])
					ecma.AddDirectory (dir);
				break;

			case "xhtml":
			case "hb":
				list.AddRange (formats [format].Select (d => (Provider) new XhtmlProvider (d)));
				break;

			case "man":
				list.Add (new ManProvider (formats [format].ToArray ()));
				break;

			case "simple":
				list.AddRange (formats [format].Select (d => (Provider) new SimpleProvider (d)));
				break;

			case "error":
				list.AddRange (formats [format].Select (d => (Provider) new ErrorProvider (d)));
				break;

			case "ecmaspec":
				list.AddRange (formats [format].Select (d => (Provider) new EcmaSpecProvider (d)));
				break;

			case "addins":
				list.AddRange (formats [format].Select (d => (Provider) new AddinsProvider (d)));
				break;
			}
		}

		HelpSource hs = new HelpSource (prefix, true);
		hs.TraceLevel = TraceLevel;

		foreach (Provider p in list) {
			p.PopulateTree (hs.Tree);
		}

		if (sort)
			hs.Tree.Sort ();
			      
		//
		// Flushes the EcmaProvider
		//
		foreach (Provider p in list)
			p.CloseTree (hs, hs.Tree);

		hs.Save ();
	}

	private void AddFormat (Dictionary<string, List<string>> d, string format, string file)
	{
		if (format == null)
			Error ("No format specified.");
		List<string> l;
		if (!d.TryGetValue (format, out l)) {
			l = new List<string> ();
			d.Add (format, l);
		}
		l.Add (file);
	}
}

}
