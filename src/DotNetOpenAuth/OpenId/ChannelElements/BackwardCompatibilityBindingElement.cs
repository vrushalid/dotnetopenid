﻿//-----------------------------------------------------------------------
// <copyright file="BackwardCompatibilityBindingElement.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId.Messages;

	/// <summary>
	/// Provides a mechanism for Relying Parties to work with OpenID 1.0 Providers
	/// without losing claimed_id and op_endpoint data, which OpenID 2.0 Providers
	/// are required to send back with positive assertions.
	/// </summary>
	internal class BackwardCompatibilityBindingElement : IChannelBindingElement {
		/// <summary>
		/// The name of the callback parameter that stores the Provider Endpoint URL
		/// to tack onto the return_to URI.
		/// </summary>
		private const string ProviderEndpointParameterName = "dnoi.op_endpoint";

		/// <summary>
		/// The name of the callback parameter that stores the Claimed Identifier
		/// to tack onto the return_to URI.
		/// </summary>
		private const string ClaimedIdentifierParameterName = "dnoi.claimed_id";

		#region IChannelBindingElement Members

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// This property is set by the channel when it is first constructed.
		/// </remarks>
		public Channel Channel { get; set; }

		/// <summary>
		/// Gets the protection offered (if any) by this binding element.
		/// </summary>
		/// <value><see cref="MessageProtections.None"/></value>
		public MessageProtections Protection {
			get { return MessageProtections.None; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public MessageProtections? ProcessOutgoingMessage(IProtocolMessage message) {
			SignedResponseRequest request = message as SignedResponseRequest;
			if (request != null && request.Version.Major < 2) {
				request.AddReturnToArguments(ProviderEndpointParameterName, request.Recipient.AbsoluteUri);

				CheckIdRequest authRequest = request as CheckIdRequest;
				if (authRequest != null) {
					request.AddReturnToArguments(ClaimedIdentifierParameterName, authRequest.ClaimedIdentifier);
				}

				return MessageProtections.None;
			}

			return null;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public MessageProtections? ProcessIncomingMessage(IProtocolMessage message) {
			IndirectSignedResponse response = message as IndirectSignedResponse;
			if (response != null && response.Version.Major < 2) {
				// GetReturnToArgument may return parameters that are not signed,
				// but we must allow for that since in OpenID 1.x, a stateless RP has 
				// no way to preserve the provider endpoint and claimed identifier otherwise.  
				// We'll verify the positive assertion later in the 
				// RelyingParty.PositiveAuthenticationResponse constructor anyway.
				// If this is a 1.0 OP signed response without these parameters then we didn't initiate
				// the request ,and since 1.0 OPs are not supposed to be able to send unsolicited 
				// assertions it's an invalid case that we throw an exception for.
				if (response.ProviderEndpoint == null) {
					string op_endpoint = response.GetReturnToArgument(ProviderEndpointParameterName);
					ErrorUtilities.VerifyProtocol(op_endpoint != null, MessagingStrings.RequiredParametersMissing, message.GetType().Name, ProviderEndpointParameterName);
					response.ProviderEndpoint = new Uri(op_endpoint);
				}

				PositiveAssertionResponse authResponse = response as PositiveAssertionResponse;
				if (authResponse != null) {
					if (authResponse.ClaimedIdentifier == null) {
						string claimedId = response.GetReturnToArgument(ClaimedIdentifierParameterName);
						ErrorUtilities.VerifyProtocol(claimedId != null, MessagingStrings.RequiredParametersMissing, message.GetType().Name, ClaimedIdentifierParameterName);
						authResponse.ClaimedIdentifier = claimedId;
					}
				}

				return MessageProtections.None;
			}

			return null;
		}

		#endregion
	}
}
