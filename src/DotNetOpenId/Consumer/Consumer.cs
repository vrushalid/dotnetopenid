namespace DotNetOpenId.Consumer
{
	using System;
	using System.Collections.Specialized;
	using System.Web.SessionState;
	using DotNetOpenId;
	using DotNetOpenId.Store;
 using DotNetOpenId.Session;
	using System.Web;

	public class FailureException : ApplicationException
	{
		public Uri identity_url;

		public FailureException(Uri identity_url, string message)
			: base(message)
		{
			this.identity_url = identity_url;
		}
	}

	public class CancelException : ApplicationException
	{
		public Uri identity_url;

		public CancelException(Uri identity_url)
		{
			this.identity_url = identity_url;
		}
	}

	public class SetupNeededException : ApplicationException
	{
		private Uri consumer_id;
		public Uri ConsumerId
		{
			get { return consumer_id; }
		}

		private Uri user_setup_url;
		public Uri UserSetupUrl
		{
			get { return user_setup_url; }
		}

		public SetupNeededException(Uri consumer_id, Uri user_setup_url)
		{
			this.consumer_id = consumer_id;
			this.user_setup_url = user_setup_url;
		}
	}

	public class Consumer
	{
		ISessionState session;
		GenericConsumer consumer;

		private string session_key_prefix;

		public string SessionKeyPrefix
		{
			get { return session_key_prefix; }
			set { session_key_prefix = value; }
		}

		string last_token = "last_token";

		protected string TokenKey
		{
			get
			{
				return session_key_prefix + last_token;
			}
		}

		ServiceEndpointManager manager;

		/// <summary>
		/// Constructs an OpenId consumer that uses the HttpApplication dictionary as
		/// its association store.
		/// </summary>
		public Consumer(ISessionState session) : this(session, HttpApplicationAssociationStore) { }

		/// <summary>
		/// Constructs an OpenId consumer that uses a given IAssociationStore.
		/// </summary>
		public Consumer(ISessionState session, IAssociationStore store)
		{
			this.session = session;
			this.manager = new ServiceEndpointManager(session);
			this.consumer = new GenericConsumer(store, new SimpleFetcher());
		}

		public AuthRequest Begin(Uri openid_url)
		{
			ServiceEndpoint endpoint = this.manager.GetNextService(openid_url, this.SessionKeyPrefix);
			if (endpoint == null)
				throw new FailureException(null, "No openid endpoint found");
			return BeginWithoutDiscovery(endpoint);
		}

		public AuthRequest BeginWithoutDiscovery(ServiceEndpoint endpoint)
		{
			AuthRequest auth_req = this.consumer.Begin(endpoint);
			this.session[this.TokenKey] = auth_req.Token;
			return auth_req;
		}

		public ConsumerResponse Complete(NameValueCollection query)
		{
			string token = this.session[TokenKey] as string;
			if (token == null)
				throw new FailureException(null, "No session state found");

			ConsumerResponse response = this.consumer.Complete(query, token);
			this.manager.Cleanup(response.IdentityUrl, this.TokenKey);

			return response;
		}

		const string associationStoreKey = "DotNetOpenId.Consumer.Consumer.AssociationStore";
		static IAssociationStore HttpApplicationAssociationStore {
			get {
				HttpContext context = HttpContext.Current;
				if (context == null)
					throw new InvalidOperationException(Strings.IAssociationStoreRequiredWhenNoHttpContextAvailable);
				IAssociationStore store = (IAssociationStore)context.Application[associationStoreKey];
				if (store == null) {
					context.Application.Lock();
					try {
						if ((store = (IAssociationStore)context.Application[associationStoreKey]) == null) {
							context.Application[associationStoreKey] = store = new MemoryStore();
						}
					} finally {
						context.Application.UnLock();
					}
				}
				return store;
			}
		}
	}
}