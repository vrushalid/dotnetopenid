using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;

namespace DotNetOpenId.Provider
{
    /// <summary>
    /// A message did not conform to the OpenID protocol.
    /// </summary>
    public class ProtocolException : Exception, IEncodable
    {
        NameValueCollection query;

        internal ProtocolException(NameValueCollection query, string text)
            : base(text)
        {
            this.query = query ?? new NameValueCollection();
        }

        internal bool HasReturnTo
        {
            get
            {
                return query[QueryStringArgs.openid.return_to] != null;
            }
        }

        #region IEncodable Members

        EncodingType IEncodable.EncodingType
        {
            get 
            {
                if (HasReturnTo)
                    return EncodingType.RedirectBrowserUrl;

                string mode = query.Get(QueryStringArgs.openid.mode);
                if (mode != null)
                    if (mode != QueryStringArgs.Modes.checkid_setup &&
                        mode != QueryStringArgs.Modes.checkid_immediate)
                        return EncodingType.ResponseBody;

                // Notes from the original port
                //# According to the OpenID spec as of this writing, we are
                //# probably supposed to switch on request type here (GET
                //# versus POST) to figure out if we're supposed to print
                //# machine-readable or human-readable content at this
                //# point.  GET/POST seems like a pretty lousy way of making
                //# the distinction though, as it's just as possible that
                //# the user agent could have mistakenly been directed to
                //# post to the server URL.

                //# Basically, if your request was so broken that you didn't
                //# manage to include an openid.mode, I'm not going to worry
                //# too much about returning you something you can't parse.
                return EncodingType.None;
            }
        }

        public IDictionary<string, string> EncodedFields
        {
            get
            {
                var q = new Dictionary<string, string>();
                q.Add(QueryStringArgs.openid.mode, QueryStringArgs.Modes.error);
                q.Add(QueryStringArgs.openid.error, this.Message);
                return q;
            }
        }
        public Uri BaseUri
        {
            get
            {
                string return_to = query.Get(QueryStringArgs.openid.return_to);
                if (return_to == null)
                    throw new InvalidOperationException("return_to URL has not been set.");
                return new Uri(return_to);
            }
        }

        #endregion

    }
}