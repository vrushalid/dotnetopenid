using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace DotNetOpenId.RelyingParty {
	class NamingContainerControlCollection : ControlCollection {
		Dictionary<string, Control> m_nameDictionary = new Dictionary<string, Control>();
		Dictionary<string, string> m_linkDictionary = new Dictionary<string, string>();

		internal NamingContainerControlCollection(Control owner) : base(owner) {
		}

		/// <summary>
		/// Determines whether a control is in the <see cref="NamingContainerControlCollection"></see>.
		/// </summary>
		/// <param name="name">The controls name.</param>
		/// <returns>
		///     <c>true</c> if a control is found; otherwise, <c>false</c>.
		/// </returns>
		public bool ContainsName(string name) {
			if (string.IsNullOrEmpty(name)) {
				return false;
			}
			return m_nameDictionary.ContainsKey(name);
		}

		public bool ContainsID(string id) {
			if (string.IsNullOrEmpty(id)) {
				return false;
			}
			return m_linkDictionary.ContainsKey(id);
		}

		/// <summary>
		/// Gets the control name given the control id.
		/// </summary>
		/// <param name="id">The control's name.</param>
		/// <returns></returns>
		public string GetName(string id) {
			if (string.IsNullOrEmpty(id)) {
				return id;
			}
			if (ContainsID(id)) {
				string name = m_linkDictionary[id];
				//if (baseid == shortid)
				//{
				//    return null;
				//}
				return name;
			}
			return id;
		}

		/// <summary>
		/// Registers the pair [name, control] and link id to name.
		/// </summary>
		/// <param name="id">The id value.</param>
		/// <param name="name">The name value.</param>
		/// <param name="control">The control.</param>
		public void RegisterControl(string id, string name, Control control) {
			m_nameDictionary.Add(name, control);
			m_linkDictionary.Add(id, name);
		}

		internal Control FindControl(string id, int pathOffset) {
			foreach (Control control in this) {
				if (control.ID == id) {
					return control;
				}
				Control result = control.FindControl(id);
				if (result != null) {
					return result;
				}
			}
			return null;

		}

		int suffixCounter;
		internal int GetUniqueControlSuffix() {
			return ++suffixCounter;
		}
	}
}
