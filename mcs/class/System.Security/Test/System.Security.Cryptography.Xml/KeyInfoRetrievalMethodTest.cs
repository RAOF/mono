//
// KeyInfoRetrievalMethodTest.cs - NUnit Test Cases for KeyInfoRetrievalMethod
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography.Xml {

	[TestFixture]
	public class KeyInfoRetrievalMethodTest {

		[Test]
		public void TestNewEmptyKeyNode () 
		{
			KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();
#if NET_1_0
			Assert.AreEqual ("<RetrievalElement xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (uri1.GetXml ().OuterXml), "Empty");
#else
			Assert.AreEqual ("<RetrievalMethod xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (uri1.GetXml ().OuterXml), "Empty");
#endif
		}

		[Test]
		public void TestNewKeyNode () 
		{
			string uri = "http://www.go-mono.com/";
			KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();
			uri1.Uri = uri;
			XmlElement xel = uri1.GetXml ();

			KeyInfoRetrievalMethod uri2 = new KeyInfoRetrievalMethod (uri1.Uri);
			uri2.LoadXml (xel);

			Assert.AreEqual ((uri1.GetXml ().OuterXml), (uri2.GetXml ().OuterXml), "uri1==uri2");
			Assert.AreEqual (uri, uri1.Uri, "uri==Uri");
		}

		[Test]
		public void TestImportKeyNode () 
		{
#if NET_1_0
			string value = "<RetrievalElement URI=\"http://www.go-mono.com/\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />";
#else
			string value = "<RetrievalMethod URI=\"http://www.go-mono.com/\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />";
#endif
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (value);

			KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();
			uri1.LoadXml (doc.DocumentElement);

			// verify that proper XML is generated (equals to original)
			string s = (uri1.GetXml ().OuterXml);
			Assert.AreEqual (value, s, "Xml");

			// verify that property is parsed correctly
			Assert.AreEqual ("http://www.go-mono.com/", uri1.Uri, "Uri");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InvalidKeyNode1 () 
		{
			KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();
			uri1.LoadXml (null);
		}

		[Test]
		public void InvalidKeyNode2 () 
		{
			string bad = "<Test></Test>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (bad);

			KeyInfoRetrievalMethod uri1 = new KeyInfoRetrievalMethod ();
			// no exception is thrown
			uri1.LoadXml (doc.DocumentElement);
#if NET_2_0
			AssertCrypto.AssertXmlEquals ("invalid", "<RetrievalMethod xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (uri1.GetXml ().OuterXml));
#elif NET_1_1
			// note that URI="" is present (unlike a empty Uri)
			Assert.AreEqual ("<RetrievalMethod URI=\"\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (uri1.GetXml ().OuterXml), "invalid");
#else
			// Fx 1.0 misnamed the tag name
			Assert.AreEqual ("<RetrievalElement URI=\"\" xmlns=\"http://www.w3.org/2000/09/xmldsig#\" />", (uri1.GetXml ().OuterXml), "invalid");
#endif
		}
	}
}
