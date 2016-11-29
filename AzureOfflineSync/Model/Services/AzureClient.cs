using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace AzureOfflineSync
{
	public class AzureClient
	{
		private IMobileServiceClient _client;
		private IMobileServiceSyncTable<Contact> _table;
		const string offlineDbPath = @"localstore.db";


		public AzureClient()
		{
			_client = new MobileServiceClient("http://mod5sample.azurewebsites.net/");
			var store = new MobileServiceSQLiteStore(offlineDbPath);
			store.DefineTable<Contact>();

			//Initializes the SyncContext using the default IMobileServiceSyncHandler.
			_client.SyncContext.InitializeAsync(store);

			_table = _client.GetSyncTable<Contact>();
		}

		public async Task<IEnumerable<Contact>> GetContacts()
		{
			var empty = new Contact[0];
			try
			{
				if(Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
					await this.SyncAsync();

				return await _table.ToEnumerableAsync();
			}
			catch (Exception ex)
			{
				return empty;
			}
		}

		public async Task SyncAsync()
		{
			ReadOnlyCollection<MobileServiceTableOperationError> syncErrors = null;

			try
			{
				await _client.SyncContext.PushAsync();

				await _table.PullAsync(
					//The first parameter is a query name that is used internally by the client SDK to implement incremental sync.
					//Use a different query name for each unique query in your program
					"allContacts",
					_table.CreateQuery());
			}
			catch (MobileServicePushFailedException exc)
			{
				if (exc.PushResult != null)
				{
					syncErrors = exc.PushResult.Errors;
				}
			}

			// Simple error/conflict handling. A real application would handle the various errors like network conditions,
			// server conflicts and others via the IMobileServiceSyncHandler.
			if (syncErrors != null)
			{
				foreach (var error in syncErrors)
				{
					if (error.OperationKind == MobileServiceTableOperationKind.Update && error.Result != null)
					{
						//Update failed, reverting to server's copy.
						await error.CancelAndUpdateItemAsync(error.Result);
					}
					else
					{
						// Discard local change.
						await error.CancelAndDiscardItemAsync();
					}

					Debug.WriteLine(@"Error executing sync operation. Item: {0} ({1}). Operation discarded.", error.TableName, error.Item["id"]);
				}
			}
		}

		public async void PurgeData() 
		{
			
			await _table.PurgeAsync("allContacts", _table.CreateQuery(), new System.Threading.CancellationToken());


		}

		public async void AddContact(Contact contact)
		{
			await _table.InsertAsync(contact);

		}
	}
}
