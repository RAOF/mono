using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;

using Mono.Documentation;
using Mono.Options;

[assembly: AssemblyTitle("Monodocs-to-HTML")]
[assembly: AssemblyCopyright("Copyright (c) 2004 Joshua Tauberer <tauberer@for.net>, released under the GPL.")]
[assembly: AssemblyDescription("Convert Monodoc XML documentation to static HTML.")]

namespace Mono.Documentation {

class MDocToHtmlConverterOptions {
	public string dest;
	public string ext = "html";
	public string onlytype;
	public string template;
	public bool   dumptemplate;
	public bool   forceUpdate;
	public HashSet<string> versions = new HashSet<string> ();
}

class MDocToHtmlConverter : MDocCommand {

	static Dictionary<string, string[]> profiles = new Dictionary<string, string[]>() {
		//                      FxVersions-----                   VsVersions-----
		{ "monotouch",    new[]{"0.0.0.0", "2.0.5.0"              } },
		{ "net_1_0",      new[]{"1.0.3300.0",                     "7.0.3300.0"} },
		{ "net_1_1",      new[]{"1.0.5000.0",                     "7.0.5000.0"} },
		{ "net_2_0",      new[]{"2.0.0.0",                        "8.0.0.0"} },
		{ "net_3_0",      new[]{"2.0.0.0", "3.0.0.0",             "8.0.0.0"} },
		{ "net_3_5",      new[]{"2.0.0.0", "3.0.0.0", "3.5.0.0",  "8.0.0.0"} },
		{ "net_4_0",      new[]{"4.0.0.0"                         } },
		{ "silverlight",  new[]{"2.0.5.0",                        "9.0.0.0"} },
	};

	public override void Run (IEnumerable<string> args)
	{
		opts = new MDocToHtmlConverterOptions ();
		var p = new OptionSet () {
			{ "default-template",
				"Writes the default XSLT to stdout.",
				v => opts.dumptemplate = v != null },
			{ "ext=",
				"The file {EXTENSION} to use for created files.  "+
					"This defaults to \"html\".",
				v => opts.ext = v },
			{ "force-update",
				"Always generate new files.  If not specified, will only generate a " + 
					"new file if the source .xml file is newer than the current output " +
					"file.",
				v => opts.forceUpdate = v != null },
			{ "o|out=",
				"The {DIRECTORY} to place the generated files and directories.",
				v => opts.dest = v },
			{ "template=",
				"An XSLT {FILE} to use to generate the created " + 
					"files.If not specified, uses the template generated by " + 
					"--default-template.",
				v => opts.template = v },
			{ "with-profile=",
				"The .NET {PROFILE} to generate documentation for.  This is " + 
					"equivalent to using --with-version for all of the " +
					"versions that a profile uses.  Valid profiles are:\n  " +
					string.Join ("\n  ", profiles.Keys.OrderBy (v => v).ToArray ()),
				v => {
					if (!profiles.ContainsKey (v))
						throw new ArgumentException (string.Format ("Unsupported profile '{0}'.", v));
					foreach (var ver in profiles [v.ToLowerInvariant ()])
						opts.versions.Add (ver);
				} },
			{ "with-version=",
				"The assembly {VERSION} to generate documentation for.  This allows " + 
					"display of a subset of types/members that correspond to the given " +
					"assembly version.  May be specified multiple times.  " + 
					"If not specified, all versions are displayed.",
				v => opts.versions.Add (v) }
		};
		List<string> extra = Parse (p, args, "export-html", 
				"[OPTIONS]+ DIRECTORIES",
				"Export mdoc documentation within DIRECTORIES to HTML.");
		if (extra == null)
			return;
		if (opts.dumptemplate)
			DumpTemplate ();
		else
			ProcessDirectories (extra);
		opts.onlytype = "ignore"; // remove warning about unused member
	}

	static MDocToHtmlConverterOptions opts;

	void ProcessDirectories (List<string> sourceDirectories)
	{
		if (sourceDirectories.Count == 0 || opts.dest == null || opts.dest == "")
			throw new ApplicationException("The source and dest options must be specified.");
		
		Directory.CreateDirectory(opts.dest);

		// Load the stylesheets, overview.xml, and resolver
		
		XslCompiledTransform overviewxsl = LoadTransform("overview.xsl", sourceDirectories);
		XslCompiledTransform stylesheet = LoadTransform("stylesheet.xsl", sourceDirectories);
		XslCompiledTransform template;
		if (opts.template == null) {
			template = LoadTransform("defaulttemplate.xsl", sourceDirectories);
		} else {
			try {
				XmlDocument templatexsl = new XmlDocument();
				templatexsl.Load(opts.template);
				template = new XslCompiledTransform (DebugOutput);
				template.Load(templatexsl);
			} catch (Exception e) {
				throw new ApplicationException("There was an error loading " + opts.template, e);
			}
		}
		
		XmlDocument overview = GetOverview (sourceDirectories);

		ArrayList extensions = GetExtensionMethods (overview);
		
		// Create the master page
		XsltArgumentList overviewargs = new XsltArgumentList();
		overviewargs.AddParam("Index", "", overview.CreateNavigator ());

		var regenIndex = ShouldRegenIndexes (opts, overview, sourceDirectories);
		if (regenIndex) {
			overviewargs.AddParam("ext", "", opts.ext);
			overviewargs.AddParam("basepath", "", "./");
			Generate(overview, overviewxsl, overviewargs, opts.dest + "/index." + opts.ext, template, sourceDirectories);
			overviewargs.RemoveParam("basepath", "");
		}
		overviewargs.AddParam("basepath", "", "../");
		
		// Create the namespace & type pages
		
		XsltArgumentList typeargs = new XsltArgumentList();
		typeargs.AddParam("ext", "", opts.ext);
		typeargs.AddParam("basepath", "", "../");
		typeargs.AddParam("Index", "", overview.CreateNavigator ());
		
		foreach (XmlElement ns in overview.SelectNodes("Overview/Types/Namespace")) {
			string nsname = ns.GetAttribute("Name");

			if (opts.onlytype != null && !opts.onlytype.StartsWith(nsname + "."))
				continue;
				
			System.IO.DirectoryInfo d = new System.IO.DirectoryInfo(opts.dest + "/" + nsname);
			if (!d.Exists) d.Create();
			
			// Create the NS page
			string nsDest = opts.dest + "/" + nsname + "/index." + opts.ext;
			if (regenIndex) {
				overviewargs.AddParam("namespace", "", nsname);
				Generate(overview, overviewxsl, overviewargs, nsDest, template, sourceDirectories);
				overviewargs.RemoveParam("namespace", "");
			}
			
			foreach (XmlElement ty in ns.SelectNodes("Type")) {
				string typename, typefile, destfile;
				GetTypePaths (opts, ty, out typename, out typefile, out destfile);

				if (DestinationIsNewer (typefile, destfile))
					// target already exists, and is newer.  why regenerate?
					continue;

				XmlDocument typexml = new XmlDocument();
				typexml.Load(typefile);
				PreserveMembersInVersions (typexml);
				if (extensions != null) {
					DocLoader loader = CreateDocLoader (overview);
					XmlDocUtils.AddExtensionMethods (typexml, extensions, loader);
				}
				
				Console.WriteLine(nsname + "." + typename);
				
				Generate(typexml, stylesheet, typeargs, destfile, template, sourceDirectories);
			}
		}
	}

	private static ArrayList GetExtensionMethods (XmlDocument doc)
	{
		XmlNodeList extensions = doc.SelectNodes ("/Overview/ExtensionMethods/*");
		if (extensions.Count == 0)
			return null;
		ArrayList r = new ArrayList (extensions.Count);
		foreach (XmlNode n in extensions)
			r.Add (n);
		return r;
	}

	static bool ShouldRegenIndexes (MDocToHtmlConverterOptions opts, XmlDocument overview, List<string> sourceDirectories)
	{
		string overviewDest   = opts.dest + "/index." + opts.ext;
		if (sourceDirectories.Any (
					d => !DestinationIsNewer (Path.Combine (d, "index.xml"), overviewDest)))
			return true;

		foreach (XmlElement type in overview.SelectNodes("Overview/Types/Namespace/Type")) {
			string _, srcfile, destfile;
			GetTypePaths (opts, type, out _, out srcfile, out destfile);

			if (srcfile == null || destfile == null)
				continue;
			if (DestinationIsNewer (srcfile, destfile))
				return true;
		}

		return false;
	}

	static void GetTypePaths (MDocToHtmlConverterOptions opts, XmlElement type, out string typename, out string srcfile, out string destfile)
	{
		srcfile   = null;
		destfile  = null;

		string nsname       = type.ParentNode.Attributes ["Name"].Value;
		string typefilebase = type.GetAttribute("Name");
		string sourceDir    = type.GetAttribute("SourceDirectory");
		typename            = type.GetAttribute("DisplayName");
		if (typename.Length == 0)
			typename = typefilebase;
		
		if (opts.onlytype != null && !(nsname + "." + typename).StartsWith(opts.onlytype))
			return;

		srcfile = CombinePath (sourceDir, nsname, typefilebase + ".xml");
		if (srcfile == null)
			return;

		destfile = CombinePath (opts.dest, nsname, typefilebase + "." + opts.ext);
	}
	
	private static void DumpTemplate() {
		Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("defaulttemplate.xsl");
		Stream o = Console.OpenStandardOutput ();
		byte[] buf = new byte[1024];
		int r;
		while ((r = s.Read (buf, 0, buf.Length)) > 0) {
			o.Write (buf, 0, r);
		}
	}
	
	private static void Generate(XmlDocument source, XslCompiledTransform transform, XsltArgumentList args, string output, XslCompiledTransform template, List<string> sourceDirectories) {
		using (TextWriter textwriter = new StreamWriter(new FileStream(output, FileMode.Create))) {
			XmlTextWriter writer = new XmlTextWriter(textwriter);
			writer.Formatting = Formatting.Indented;
			writer.Indentation = 2;
			writer.IndentChar = ' ';
			
			try {
				var intermediate = new StringBuilder ();
				transform.Transform (
						new XmlNodeReader (source), 
						args, 
						XmlWriter.Create (intermediate, transform.OutputSettings),
						new ManifestResourceResolver(sourceDirectories.ToArray ()));
				template.Transform (
						XmlReader.Create (new StringReader (intermediate.ToString ())),
						new XsltArgumentList (),
						new XhtmlWriter (writer),
						null);
			} catch (Exception e) {
				throw new ApplicationException("An error occured while generating " + output, e);
			}
		}
	}
	
	private XslCompiledTransform LoadTransform(string name, List<string> sourceDirectories) {
		try {
			XmlDocument xsl = new XmlDocument();
			xsl.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream(name));
			
			if (name == "overview.xsl") {
				// bit of a hack.  overview needs the templates in stylesheet
				// for doc formatting, and rather than write a resolver, I'll
				// just do the import for it.
				
				XmlNode importnode = xsl.DocumentElement.SelectSingleNode("*[name()='xsl:include']");
				xsl.DocumentElement.RemoveChild(importnode);
				
				XmlDocument xsl2 = new XmlDocument();
				xsl2.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("stylesheet.xsl"));
				foreach (XmlNode node in xsl2.DocumentElement.ChildNodes)
					xsl.DocumentElement.AppendChild(xsl.ImportNode(node, true));
			}
			
			XslCompiledTransform t = new XslCompiledTransform (DebugOutput);
			t.Load (
					xsl, 
					XsltSettings.TrustedXslt,
					new ManifestResourceResolver (sourceDirectories.ToArray ()));
			
			return t;
		} catch (Exception e) {
			throw new ApplicationException("Error loading " + name + " from internal resource", e);
		}
	}

	private static DocLoader CreateDocLoader (XmlDocument overview)
	{
		Hashtable docs = new Hashtable ();
		DocLoader loader = delegate (string s) {
			XmlDocument d = null;
			if (!docs.ContainsKey (s)) {
				foreach (XmlNode n in overview.SelectNodes ("//Type")) {
					string ns = n.ParentNode.Attributes ["Name"].Value;
					string t  = n.Attributes ["Name"].Value;
					string sd = n.Attributes ["SourceDirectory"].Value;
					if (s == ns + "." + t.Replace ("+", ".")) {
						string f = CombinePath (sd, ns, t + ".xml");
						if (File.Exists (f)) {
							d = new XmlDocument ();
							d.Load (f);
						}
						docs.Add (s, d);
						break;
					}
				}
			}
			else
				d = (XmlDocument) docs [s];
			return d;
		};
		return loader;
	}

	static string CombinePath (params string[] paths)
	{
		if (paths == null)
			return null;
		if (paths.Length == 1)
			return paths [0];
		var path = Path.Combine (paths [0], paths [1]);
		for (int i = 2; i < paths.Length; ++i)
			path = Path.Combine (path, paths [i]);
		return path;
	}

	private XmlDocument GetOverview (IEnumerable<string> directories)
	{
		var index = new XmlDocument ();

		var overview  = index.CreateElement ("Overview");
		var assemblies= index.CreateElement ("Assemblies");
		var types     = index.CreateElement ("Types");
		var ems       = index.CreateElement ("ExtensionMethods");

		index.AppendChild (overview);
		overview.AppendChild (assemblies);
		overview.AppendChild (types);
		overview.AppendChild (ems);

		bool first = true;

		foreach (var dir in directories) {
			var indexFile = Path.Combine (dir, "index.xml");
			try {
				var doc = new XmlDocument ();
				doc.Load (indexFile);
				if (first) {
					var c = doc.SelectSingleNode ("/Overview/Copyright");
					var t = doc.SelectSingleNode ("/Overview/Title");
					var r = doc.SelectSingleNode ("/Overview/Remarks");
					if (c != null && t != null && r != null) {
						var e = index.CreateElement ("Copyright");
						e.InnerXml = c.InnerXml;
						overview.AppendChild (e);

						e = index.CreateElement ("Title");
						e.InnerXml = t.InnerXml;
						overview.AppendChild (e);

						e = index.CreateElement ("Remarks");
						e.InnerXml = r.InnerXml;
						overview.AppendChild (e);

						first = false;
					}
				}
				AddAssemblies (assemblies, doc);
				AddTypes (types, doc, dir);
				AddChildren (ems, doc, "/Overview/ExtensionMethods");
			}
			catch (Exception e) {
				Message (TraceLevel.Warning, "Could not load documentation index '{0}': {1}",
						indexFile, e.Message);
			}
		}

		return index;
	}

	static void AddChildren (XmlNode dest, XmlDocument source, string path)
	{
		var n = source.SelectSingleNode (path);
		if (n != null)
			foreach (XmlNode c in n.ChildNodes)
				dest.AppendChild (dest.OwnerDocument.ImportNode (c, true));
	}

	static void AddAssemblies (XmlNode dest, XmlDocument source)
	{
		foreach (XmlNode asm in source.SelectNodes ("/Overview/Assemblies/Assembly")) {
			var n = asm.Attributes ["Name"].Value;
			var v = asm.Attributes ["Version"].Value;
			if (dest.SelectSingleNode (string.Format ("Assembly[@Name='{0}'][@Value='{1}']", n, v)) == null) {
				dest.AppendChild (dest.OwnerDocument.ImportNode (asm, true));
			}
		}
	}

	static void AddTypes (XmlNode dest, XmlDocument source, string sourceDirectory)
	{
		var types = source.SelectSingleNode ("/Overview/Types");
		if (types == null)
			return;
		foreach (XmlNode ns in types.ChildNodes) {
			var n = ns.Attributes ["Name"].Value;
			var nsd = dest.SelectSingleNode (string.Format ("Namespace[@Name='{0}']", n));
			if (nsd == null) {
				nsd = dest.OwnerDocument.CreateElement ("Namespace");
				AddAttribute (nsd, "Name", n);
				dest.AppendChild (nsd);
			}
			foreach (XmlNode t in ns.ChildNodes) {
				if (!TypeInVersions (sourceDirectory, n, t))
					continue;
				var c = dest.OwnerDocument.ImportNode (t, true);
				AddAttribute (c, "SourceDirectory", sourceDirectory);
				nsd.AppendChild (c);
			}
			if (nsd.ChildNodes.Count == 0)
				dest.RemoveChild (nsd);
		}
	}

	static bool TypeInVersions (string sourceDirectory, string ns, XmlNode type)
	{
		if (opts.versions.Count == 0)
			return true;
		var file = Path.Combine (Path.Combine (sourceDirectory, ns), type.Attributes ["Name"].Value + ".xml");
		if (!File.Exists (file))
			return false;
		XPathDocument doc;
		using (var s = File.OpenText (file))
			doc = new XPathDocument (s);
		return MemberInVersions (doc.CreateNavigator ().SelectSingleNode ("/Type"));
	}

	static bool MemberInVersions (XPathNavigator nav)
	{
		return nav.Select ("AssemblyInfo/AssemblyVersion")
			.Cast<object> ()
			.Any (v => opts.versions.Contains (v.ToString ()));
	}

	static void AddAttribute (XmlNode self, string name, string value)
	{
		var a = self.OwnerDocument.CreateAttribute (name);
		a.Value = value;
		self.Attributes.Append (a);
	}

	private static bool DestinationIsNewer (string source, string dest)
	{
		return !opts.forceUpdate && File.Exists (dest) &&
			File.GetLastWriteTime (source) < File.GetLastWriteTime (dest);
	}

	private static void PreserveMembersInVersions (XmlDocument doc)
	{
		if (opts.versions.Count == 0)
			return;
		var remove = new List<XmlNode>();
		foreach (XmlNode m in doc.SelectNodes ("/Type/Members/Member")) {
			if (!MemberInVersions (m.CreateNavigator ()))
				remove.Add (m);
		}
		XmlNode members = doc.SelectSingleNode ("/Type/Members");
		foreach (var m in remove)
			members.RemoveChild (m);
	}
}

}
