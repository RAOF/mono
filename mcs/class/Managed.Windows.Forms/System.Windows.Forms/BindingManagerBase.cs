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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//	Jackson Harper	jackson@ximian.com
//


// NOT COMPLETE

using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms
{
	public abstract class BindingManagerBase
	{
		private BindingsCollection	bindings;
		internal bool transfering_data; /* true if we're pushing or pulling data */

		#region Public Constructors
		public BindingManagerBase()
		{
		}
		#endregion	// Public Constructors

		#region Protected Instance Fields
		protected EventHandler onCurrentChangedHandler;
		protected EventHandler onPositionChangedHandler;
#if NET_2_0
		internal EventHandler onCurrentItemChangedHandler;
#endif
		#endregion	// Protected Instance Fields

		#region Public Instance Properties
		public BindingsCollection Bindings {
			get {
				if (bindings == null) {
					bindings = new BindingsCollection ();
				}
				return bindings;
			}
		}

		public abstract int Count {
			get;
		}

		public abstract object Current {
			get;
		}

#if NET_2_0
		public bool IsBindingSuspended {
			get {
				return IsSuspended;
			}
		}
#endif

		public abstract int Position {
			get; set;
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public abstract void AddNew();

		public abstract void CancelCurrentEdit();

		public abstract void EndCurrentEdit();

#if NET_2_0
		public virtual PropertyDescriptorCollection GetItemProperties()
		{
			return GetItemPropertiesInternal ();
		}
		
		internal virtual PropertyDescriptorCollection GetItemPropertiesInternal ()
		{
			throw new NotImplementedException ();
		}
#else
		public abstract PropertyDescriptorCollection GetItemProperties();
#endif

		public abstract void RemoveAt(int index);

		public abstract void ResumeBinding();

		public abstract void SuspendBinding();
		#endregion	// Public Instance Methods

		internal virtual bool IsSuspended {
			get {
				return false;
			}
		}

		#region Protected Instance Methods
		[MonoTODO]
		protected internal virtual PropertyDescriptorCollection GetItemProperties (ArrayList dataSources, ArrayList listAccessors)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual PropertyDescriptorCollection GetItemProperties (Type listType, int offset, ArrayList dataSources, ArrayList listAccessors)
		{
			throw new NotImplementedException();
		}

		protected internal abstract string GetListName (ArrayList listAccessors);

		protected internal abstract void OnCurrentChanged (EventArgs e);

		protected void PullData()
		{
			try {
				if (!transfering_data) {
					transfering_data = true;
					UpdateIsBinding ();
				}
				foreach (Binding binding in Bindings) {
					binding.PullData ();
				}
			} finally {
				transfering_data = false;
			}
		}

		protected void PushData()
		{
			try {
				if (!transfering_data) {
					transfering_data = true;
					UpdateIsBinding ();
				}
				foreach (Binding binding in Bindings) {
					binding.PushData ();
				}
			} finally {
				transfering_data = false;
			}
		}


#if NET_2_0
		protected void OnBindingComplete (BindingCompleteEventArgs args)
		{
			if (BindingComplete != null)
				BindingComplete (this, args);
		}

		protected abstract void OnCurrentItemChanged (EventArgs e);

		protected void OnDataError (Exception e)
		{
			if (DataError != null)
				DataError (this, new BindingManagerDataErrorEventArgs (e));
		}
#endif

		protected abstract void UpdateIsBinding();
		#endregion	// Protected Instance Methods

		internal void AddBinding (Binding binding)
		{
			if (Bindings.Contains (binding))
				return;
			Bindings.Add (binding);
		}

		#region Events
		public event EventHandler CurrentChanged {
			add { onCurrentChangedHandler += value; }
			remove { onCurrentChangedHandler -= value; }
		}

		public event EventHandler PositionChanged {
			add { onPositionChangedHandler += value; }
			remove { onPositionChangedHandler -= value; }
		}

#if NET_2_0
		public event EventHandler CurrentItemChanged {
			add { onCurrentItemChangedHandler += value; }
			remove { onCurrentItemChangedHandler -= value; }
		}

		public event BindingCompleteEventHandler BindingComplete;
		public event BindingManagerDataErrorEventHandler DataError;
#endif
		#endregion	// Events
	}
}
