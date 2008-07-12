using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace DotNetOpenId.RelyingParty {
	abstract class NamingProvider {
		public abstract string SetControlID(string name, System.Web.UI.Control control);
		/// <summary>
		/// Creates a controls collection.
		/// </summary>
		/// <param name="control">The owner control.</param>
		/// <returns></returns>
		public ControlCollection CreateControlCollection(System.Web.UI.Control control) {
			return new NamingContainerControlCollection(control);
		}

		/// <summary>
		/// Gets the control name given the control's id.
		/// </summary>
		/// <param name="control">The control.</param>
		/// <param name="id">The controls id</param>
		/// <returns></returns>
		public string GetControlID(System.Web.UI.Control control, string id) {
			if (control == null) {
				throw new ArgumentNullException("control");
			}
			if (control.NamingContainer != null) {
				NamingContainerControlCollection namingContainerCollection = control.NamingContainer.Controls as NamingContainerControlCollection;
				if (namingContainerCollection != null) {
					return namingContainerCollection.GetName(id);
				}
			}
			return id;
		}

		/// <summary>
		/// Finds a control.
		/// </summary>
		/// <param name="control">The control.</param>
		/// <param name="id">The id.</param>
		/// <param name="pathOffset">The path offset.</param>
		/// <returns></returns>
		public Control FindControl(System.Web.UI.Control control, string id, int pathOffset) {
			if (control == null) {
				throw new ArgumentNullException("control");
			}

			NamingContainerControlCollection controlsCollection = control.Controls as NamingContainerControlCollection;
			if (controlsCollection == null) {
				return null;
			}

			return controlsCollection.FindControl(id, pathOffset);
		}
	}
}
