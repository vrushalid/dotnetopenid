﻿//-----------------------------------------------------------------------
// <copyright file="FetchResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Extensions.AttributeExchange {
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// The Attribute Exchange Fetch message, response leg.
	/// </summary>
	[Serializable]
	public sealed class FetchResponse : ExtensionBase, IMessageWithEvents {
		/// <summary>
		/// The factory method that may be used in deserialization of this message.
		/// </summary>
		internal static readonly OpenIdExtensionFactory.CreateDelegate Factory = (typeUri, data, baseMessage) => {
			if (typeUri == Constants.TypeUri && baseMessage is IndirectSignedResponse) {
				string mode;
				if (data.TryGetValue("mode", out mode) && mode == Mode) {
					return new FetchResponse();
				}
			}

			return null;
		};

		/// <summary>
		/// The value of the 'mode' parameter.
		/// </summary>
		[MessagePart("mode", IsRequired = true)]
		private const string Mode = "fetch_response";

		/// <summary>
		/// The collection of provided attributes.  This field will never be null.
		/// </summary>
		private readonly KeyedCollection<string, AttributeValues> attributesProvided = new KeyedCollectionDelegate<string, AttributeValues>(av => av.TypeUri);

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchResponse"/> class.
		/// </summary>
		public FetchResponse()
			: base(new Version(1, 0), Constants.TypeUri, null) {
		}

		/// <summary>
		/// Gets a sequence of the attributes whose values are provided by the OpenID Provider.
		/// </summary>
		public KeyedCollection<string, AttributeValues> Attributes {
			get { return this.attributesProvided; }
		}

		/// <summary>
		/// Gets a value indicating whether the OpenID Provider intends to
		/// honor the request for updates.
		/// </summary>
		public bool UpdateUrlSupported {
			get { return this.UpdateUrl != null; }
		}

		/// <summary>
		/// Gets or sets the URL the OpenID Provider will post updates to.  
		/// Must be set if the Provider supports and will use this feature.
		/// </summary>
		[MessagePart("update_url", IsRequired = false)]
		public Uri UpdateUrl { get; set; }

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">
		/// The <paramref name="obj"/> parameter is null.
		/// </exception>
		public override bool Equals(object obj) {
			FetchResponse other = obj as FetchResponse;
			if (other == null) {
				return false;
			}

			if (this.Version != other.Version) {
				return false;
			}

			if (this.UpdateUrl != other.UpdateUrl) {
				return false;
			}

			if (!MessagingUtilities.AreEquivalentUnordered(this.Attributes.ToList(), other.Attributes.ToList())) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			unchecked {
				int hashCode = this.Version.GetHashCode();

				if (this.UpdateUrl != null) {
					hashCode += this.UpdateUrl.GetHashCode();
				}

				foreach (AttributeValues value in this.Attributes) {
					hashCode += value.GetHashCode();
				}

				return hashCode;
			}
		}

		#region IMessageWithEvents Members

		/// <summary>
		/// Called when the message is about to be transmitted,
		/// before it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnSending() {
			var extraData = ((IMessage)this).ExtraData;
			AXUtilities.SerializeAttributes(extraData, this.attributesProvided);
		}

		/// <summary>
		/// Called when the message has been received,
		/// after it passes through the channel binding elements.
		/// </summary>
		void IMessageWithEvents.OnReceiving() {
			var extraData = ((IMessage)this).ExtraData;
			foreach (var att in AXUtilities.DeserializeAttributes(extraData)) {
				this.Attributes.Add(att);
			}
		}

		#endregion

		/// <summary>
		/// Checks the message state for conformity to the protocol specification
		/// and throws an exception if the message is invalid.
		/// </summary>
		/// <remarks>
		/// 	<para>Some messages have required fields, or combinations of fields that must relate to each other
		/// in specialized ways.  After deserializing a message, this method checks the state of the
		/// message to see if it conforms to the protocol.</para>
		/// 	<para>Note that this property should <i>not</i> check signatures or perform any state checks
		/// outside this scope of this particular message.</para>
		/// </remarks>
		/// <exception cref="ProtocolException">Thrown if the message is invalid.</exception>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();

			if (this.UpdateUrl != null && !this.UpdateUrl.IsAbsoluteUri) {
				this.UpdateUrl = null;
				Logger.OpenId.ErrorFormat("The AX fetch response update_url parameter was not absolute ('{0}').  Ignoring value.", this.UpdateUrl);
			}
		}
	}
}
