//
// System.Web.UI.Adapters.ControlAdapter
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

#if NET_2_0
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.Adapters
{
	public abstract class ControlAdapter
	{
		internal ControlAdapter (Control c)
		{
			control = c;
		}
		
		protected ControlAdapter ()
		{
		}

		protected HttpBrowserCapabilities Browser 
		{
			get {
				return Page.Request.Browser;
			}
		}

		internal Control control;
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected internal Control Control 
		{
			protected get {
				return control;
			}
			set {
				control = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		protected Page Page 
		{
			get {
				return Control.Page;
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		protected PageAdapter PageAdapter 
		{
			get {
				return Control.Page.PageAdapter;
			}
		}

		protected internal virtual void BeginRender (HtmlTextWriter w)
		{
			w.BeginRender();
		}

		protected internal virtual void CreateChildControls ()
		{
		}

		protected internal virtual void EndRender (HtmlTextWriter w)
		{
			w.EndRender ();
		}

		protected internal virtual void LoadAdapterControlState (object state)
		{
		}

		protected internal virtual void LoadAdapterViewState (object state)
		{
		}

		protected internal virtual void OnInit (EventArgs e)
		{
			Control.OnInit(e);
		}

		protected internal virtual void OnLoad (EventArgs e)
		{
			Control.OnLoad(e);
		}

		protected internal virtual void OnPreRender (EventArgs e)
		{
			Control.OnPreRender(e);
		}

		protected internal virtual void OnUnload (EventArgs e)
		{
			Control.OnUnload(e);
		}

		protected internal virtual void Render (HtmlTextWriter w)
		{
			Control.Render (w);
		}

		protected internal virtual void RenderChildren (HtmlTextWriter w)
		{
			Control.RenderChildren (w);
		}

		protected internal virtual object SaveAdapterControlState ()
		{
			return null;
		}

		protected internal virtual object SaveAdapterViewState ()
		{
			return null;
		}
	}
}
#endif
