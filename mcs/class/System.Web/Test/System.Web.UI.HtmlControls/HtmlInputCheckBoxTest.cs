//
// HtmlInputCheckBoxTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlInputCheckBox
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using MonoTests.stand_alone.WebHarness;
using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {
	public class TestHtmlInputCheckBox : HtmlInputCheckBox {
		public string Render ()
		{
			HtmlTextWriter writer = new HtmlTextWriter (new StringWriter ());
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}
	}

	[TestFixture]
	public class HtmlInputCheckBoxTest {

		[Test]
		public void DefaultProperties ()
		{
			HtmlInputCheckBox c = new HtmlInputCheckBox ();
		
			Assert.AreEqual (1, c.Attributes.Count, "Attributes.Count");

			Assert.IsFalse (c.Checked, "Checked");
			
			Assert.AreEqual (1, c.Attributes.Count, "Attributes.Count after");
		}

		[Test]
		public void NullProperties ()
		{
			HtmlInputCheckBox c = new HtmlInputCheckBox ();
			
			Assert.AreEqual (1, c.Attributes.Count, "Attributes.Count");
			Assert.AreEqual ("checkbox", c.Attributes["type"], "Attributes[\"type\"]");
			
			c.Checked = true;
			Assert.IsTrue (c.Checked, "Checked");
			
			Assert.AreEqual (2, c.Attributes.Count, "Attributes.Count after");
			Assert.AreEqual ("checked", c.Attributes["checked"], "Attributes[\"checked\"]");
		}

		[Test]
		public void CleanProperties ()
		{
			HtmlInputCheckBox c = new HtmlInputCheckBox ();

			c.Checked = true;
			Assert.AreEqual (2, c.Attributes.Count, "Attributes.Count");

			c.Checked = false;
			Assert.AreEqual (1, c.Attributes.Count, "Attributes.Count after");
		}

		[Test] 
		public void Render ()
		{
			TestHtmlInputCheckBox c = new TestHtmlInputCheckBox ();

			c.ID = "*1*";
			
			string s = c.Render ();

			Assert.IsTrue (s.IndexOf (" type=\"checkbox\"") > 0, "type");

			c.Checked = true;
			s = c.Render ();

#if NET_2_0
			HtmlDiff.AssertAreEqual ("<input name=\"*1*\" id=\"*1*\" type=\"checkbox\" checked=\"checked\" />", s, "Render fail");
#else
			HtmlDiff.AssertAreEqual ("<input name=\"*1*\" type=\"checkbox\" id=\"*1*\" checked=\"checked\" />", s, "Render fail");
#endif
		}
	}
}
