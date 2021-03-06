﻿//-----------------------------------------------------------------------
// <copyright file="ErrorUtilities.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics;
	using System.Diagnostics.Contracts;
	using System.Globalization;
	using System.Web;

	/// <summary>
	/// A collection of error checking and reporting methods.
	/// </summary>
	[ContractVerification(true)]
	[Pure]
	internal static class ErrorUtilities {
		/// <summary>
		/// Wraps an exception in a new <see cref="ProtocolException"/>.
		/// </summary>
		/// <param name="inner">The inner exception to wrap.</param>
		/// <param name="errorMessage">The error message for the outer exception.</param>
		/// <param name="args">The string formatting arguments, if any.</param>
		/// <returns>The newly constructed (unthrown) exception.</returns>
		[Pure]
		internal static Exception Wrap(Exception inner, string errorMessage, params object[] args) {
			Contract.Requires(args != null);
			Contract.Assume(errorMessage != null);
			return new ProtocolException(string.Format(CultureInfo.CurrentCulture, errorMessage, args), inner);
		}

		/// <summary>
		/// Throws an internal error exception.
		/// </summary>
		/// <param name="errorMessage">The error message.</param>
		/// <exception cref="InternalErrorException">Always thrown.</exception>
		[Pure]
		internal static void ThrowInternal(string errorMessage) {
			VerifyInternal(false, errorMessage);
		}

		/// <summary>
		/// Checks a condition and throws an internal error exception if it evaluates to false.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="errorMessage">The message to include in the exception, if created.</param>
		/// <exception cref="InternalErrorException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyInternal(bool condition, string errorMessage) {
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<InternalErrorException>(!condition);
			if (!condition) {
				// Since internal errors are really bad, take this chance to
				// help the developer find the cause by breaking into the
				// debugger if one is attached.
				if (Debugger.IsAttached) {
					Debugger.Break();
				}

				throw new InternalErrorException(errorMessage);
			}
		}

		/// <summary>
		/// Checks a condition and throws an internal error exception if it evaluates to false.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="errorMessage">The message to include in the exception, if created.</param>
		/// <param name="args">The formatting arguments.</param>
		/// <exception cref="InternalErrorException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyInternal(bool condition, string errorMessage, params object[] args) {
			Contract.Requires(args != null);
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<InternalErrorException>(!condition);
			Contract.Assume(errorMessage != null);
			if (!condition) {
				errorMessage = string.Format(CultureInfo.CurrentCulture, errorMessage, args);
				throw new InternalErrorException(errorMessage);
			}
		}

		/// <summary>
		/// Checks a condition and throws an <see cref="InvalidOperationException"/> if it evaluates to false.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="errorMessage">The message to include in the exception, if created.</param>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyOperation(bool condition, string errorMessage) {
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<InvalidOperationException>(!condition);
			if (!condition) {
				throw new InvalidOperationException(errorMessage);
			}
		}

		/// <summary>
		/// Checks a condition and throws a <see cref="NotSupportedException"/> if it evaluates to false.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="errorMessage">The message to include in the exception, if created.</param>
		/// <exception cref="NotSupportedException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifySupported(bool condition, string errorMessage) {
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<NotSupportedException>(!condition);
			if (!condition) {
				throw new NotSupportedException(errorMessage);
			}
		}

		/// <summary>
		/// Checks a condition and throws a <see cref="NotSupportedException"/> if it evaluates to false.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="errorMessage">The message to include in the exception, if created.</param>
		/// <param name="args">The string formatting arguments for <paramref name="message"/>.</param>
		/// <exception cref="NotSupportedException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifySupported(bool condition, string errorMessage, params object[] args) {
			Contract.Requires(args != null);
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<NotSupportedException>(!condition);
			Contract.Assume(errorMessage != null);
			if (!condition) {
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, errorMessage, args));
			}
		}

		/// <summary>
		/// Checks a condition and throws an <see cref="InvalidOperationException"/> if it evaluates to false.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="errorMessage">The message to include in the exception, if created.</param>
		/// <param name="args">The formatting arguments.</param>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyOperation(bool condition, string errorMessage, params object[] args) {
			Contract.Requires(args != null);
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<InvalidOperationException>(!condition);
			Contract.Assume(errorMessage != null);
			if (!condition) {
				errorMessage = string.Format(CultureInfo.CurrentCulture, errorMessage, args);
				throw new InvalidOperationException(errorMessage);
			}
		}

		/// <summary>
		/// Checks a condition and throws an <see cref="InfoCard.InformationCardException"/> 
		/// if it evaluates to false.
		/// </summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="errorMessage">The message to include in the exception, if created.</param>
		/// <param name="args">The formatting arguments.</param>
		/// <exception cref="InfoCard.InformationCardException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyInfoCard(bool condition, string errorMessage, params object[] args) {
			Contract.Requires(args != null);
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<InfoCard.InformationCardException>(!condition);
			Contract.Assume(errorMessage != null);
			if (!condition) {
				errorMessage = string.Format(CultureInfo.CurrentCulture, errorMessage, args);
				throw new InfoCard.InformationCardException(errorMessage);
			}
		}

		/// <summary>
		/// Throws a <see cref="ProtocolException"/> if some <paramref name="condition"/> evaluates to false.
		/// </summary>
		/// <param name="condition">True to do nothing; false to throw the exception.</param>
		/// <param name="faultedMessage">The message being processed that would be responsible for the exception if thrown.</param>
		/// <param name="errorMessage">The error message for the exception.</param>
		/// <param name="args">The string formatting arguments, if any.</param>
		/// <exception cref="ProtocolException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyProtocol(bool condition, IProtocolMessage faultedMessage, string errorMessage, params object[] args) {
			Contract.Requires(args != null);
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<ProtocolException>(!condition);
			Contract.Assume(errorMessage != null);
			if (!condition) {
				throw new ProtocolException(string.Format(CultureInfo.CurrentCulture, errorMessage, args), faultedMessage);
			}
		}

		/// <summary>
		/// Throws a <see cref="ProtocolException"/> if some <paramref name="condition"/> evaluates to false.
		/// </summary>
		/// <param name="condition">True to do nothing; false to throw the exception.</param>
		/// <param name="message">The error message for the exception.</param>
		/// <param name="args">The string formatting arguments, if any.</param>
		/// <exception cref="ProtocolException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyProtocol(bool condition, string message, params object[] args) {
			Contract.Requires(args != null);
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<ProtocolException>(!condition);
			Contract.Assume(message != null);
			if (!condition) {
				throw new ProtocolException(string.Format(CultureInfo.CurrentCulture, message, args));
			}
		}

		/// <summary>
		/// Throws a <see cref="ProtocolException"/>.
		/// </summary>
		/// <param name="message">The message to set in the exception.</param>
		/// <param name="args">The formatting arguments of the message.</param>
		/// <returns>
		/// An InternalErrorException, which may be "thrown" by the caller in order
		/// to satisfy C# rules to show that code will never be reached, but no value
		/// actually is ever returned because this method guarantees to throw.
		/// </returns>
		/// <exception cref="ProtocolException">Always thrown.</exception>
		[Pure]
		internal static Exception ThrowProtocol(string message, params object[] args) {
			Contract.Requires(args != null);
			Contract.Assume(message != null);
			VerifyProtocol(false, message, args);

			// we never reach here, but this allows callers to "throw" this method.
			return new InternalErrorException();
		}

		/// <summary>
		/// Throws a <see cref="FormatException"/>.
		/// </summary>
		/// <param name="message">The message for the exception.</param>
		/// <param name="args">The string formatting arguments for <paramref name="message"/>.</param>
		/// <returns>Nothing.  It's just here so the caller can throw this method for C# compilation check.</returns>
		[Pure]
		internal static Exception ThrowFormat(string message, params object[] args) {
			Contract.Requires(args != null);
			Contract.Assume(message != null);
			throw new FormatException(string.Format(CultureInfo.CurrentCulture, message, args));
		}

		/// <summary>
		/// Throws a <see cref="FormatException"/> if some condition is false.
		/// </summary>
		/// <param name="condition">The expression to evaluate.  A value of <c>false</c> will cause the exception to be thrown.</param>
		/// <param name="message">The message for the exception.</param>
		/// <param name="args">The string formatting arguments for <paramref name="message"/>.</param>
		/// <exception cref="FormatException">Thrown when <paramref name="condition"/> is <c>false</c>.</exception>
		[Pure]
		internal static void VerifyFormat(bool condition, string message, params object[] args) {
			Contract.Requires(args != null);
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<FormatException>(!condition);
			Contract.Assume(message != null);
			if (!condition) {
				throw ThrowFormat(message, args);
			}
		}

		/// <summary>
		/// Verifies something about the argument supplied to a method.
		/// </summary>
		/// <param name="condition">The condition that must evaluate to true to avoid an exception.</param>
		/// <param name="message">The message to use in the exception if the condition is false.</param>
		/// <param name="args">The string formatting arguments, if any.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyArgument(bool condition, string message, params object[] args) {
			Contract.Requires(args != null);
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<ArgumentException>(!condition);
			Contract.Assume(message != null);
			if (!condition) {
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, message, args));
			}
		}

		/// <summary>
		/// Verifies something about the argument supplied to a method.
		/// </summary>
		/// <param name="condition">The condition that must evaluate to true to avoid an exception.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyArgumentInRange(bool condition, string parameterName) {
			Contract.Requires(condition);
			if (!condition) {
				throw new ArgumentOutOfRangeException(parameterName);
			}
		}

		/// <summary>
		/// Verifies something about the argument supplied to a method.
		/// </summary>
		/// <param name="condition">The condition that must evaluate to true to avoid an exception.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="message">The unformatted message.</param>
		/// <param name="args">The string formatting arguments.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyArgumentInRange(bool condition, string parameterName, string message, params object[] args) {
			Contract.Requires(condition);
			Contract.Requires(args != null);
			Contract.Assume(message != null);
			if (!condition) {
				throw new ArgumentOutOfRangeException(parameterName, string.Format(CultureInfo.CurrentCulture, message, args));
			}
		}

		/// <summary>
		/// Verifies something about the argument supplied to a method.
		/// </summary>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="message">The message to use in the exception if the condition is false.</param>
		/// <param name="args">The string formatting arguments, if any.</param>
		/// <returns>Never returns anything.  It always throws.</returns>
		/// <exception cref="ArgumentException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static Exception ThrowArgumentNamed(string parameterName, string message, params object[] args) {
			Contract.Requires(args != null);
			Contract.Assume(message != null);
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, message, args), parameterName);
		}

		/// <summary>
		/// Verifies something about the argument supplied to a method.
		/// </summary>
		/// <param name="condition">The condition that must evaluate to true to avoid an exception.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="message">The message to use in the exception if the condition is false.</param>
		/// <param name="args">The string formatting arguments, if any.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="condition"/> evaluates to <c>false</c>.</exception>
		[Pure]
		internal static void VerifyArgumentNamed(bool condition, string parameterName, string message, params object[] args) {
			Contract.Requires(args != null);
			Contract.Ensures(condition);
			Contract.EnsuresOnThrow<ArgumentException>(!condition);
			Contract.Assume(message != null);
			if (!condition) {
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, message, args), parameterName);
			}
		}

		/// <summary>
		/// Verifies that some given value is not null.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <param name="paramName">Name of the parameter, which will be used in the <see cref="ArgumentException"/>, if thrown.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
		[Pure]
		internal static void VerifyArgumentNotNull(object value, string paramName) {
			Contract.Requires(value != null);
			if (value == null) {
				throw new ArgumentNullException(paramName);
			}
		}

		/// <summary>
		/// Verifies that some string is not null and has non-zero length.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <param name="paramName">Name of the parameter, which will be used in the <see cref="ArgumentException"/>, if thrown.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="value"/> has zero length.</exception>
		[Pure]
		internal static void VerifyNonZeroLength(string value, string paramName) {
			Contract.Requires((value != null && value.Length > 0) || !string.IsNullOrEmpty(value));
			VerifyArgumentNotNull(value, paramName);
			if (value.Length == 0) {
				throw new ArgumentException(MessagingStrings.UnexpectedEmptyString, paramName);
			}
		}

		/// <summary>
		/// Verifies that <see cref="HttpContext.Current"/> != <c>null</c>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="HttpContext.Current"/> == <c>null</c></exception>
		[Pure]
		internal static void VerifyHttpContext() {
			Contract.Ensures(HttpContext.Current != null);
			ErrorUtilities.VerifyOperation(HttpContext.Current != null, MessagingStrings.HttpContextRequired);
		}
	}
}
