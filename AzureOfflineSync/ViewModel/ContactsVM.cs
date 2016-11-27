using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace AzureOfflineSync
{
	public class ContactsVM:ObservableBaseObject
	{
		public ObservableCollection<Contact> Contacts { get; set; }
		private AzureClient _client;
		public Command RefreshCommand { get; set; }
		public Command PurgeCommand { get; set; }
		public Command GenerateContactsCommand { get; set; }
		private bool isBusy;
		public bool IsBusy
		{
			get { return isBusy; }
			set { isBusy = value; OnPropertyChanged(); }
		}


		public ContactsVM()
		{
			RefreshCommand = new Command(() => Load());
			GenerateContactsCommand = new Command(() => generateContacts());
			PurgeCommand = new Command(() => purgeContacts());
			Contacts = new ObservableCollection<Contact>();
			_client = new AzureClient();

		}

		void purgeContacts()
		{
			_client.PurgeData();
		}

		async void generateContacts()
		{
			string[] names = { "José Luis", "Miguel Ángel", "José Francisco", "Jesús Antonio", "Jorge", "Alberto",
								"Sofía", "Camila", "Valentina", "Isabella", "Ximena", "Ana"};
			string[] lastNames = { "Hernández", "García", "Martínez", "López", "González", "Méndez", "Castillo", "Corona","Cruz" };

			Random rdn = new Random(DateTime.Now.Millisecond);
			for (int i = 0; i < 10; i++)
			{
				_client.AddContact(new Contact() { Name = $"{names[rdn.Next(0, 12)]} {lastNames[rdn.Next(0, 8)]}"  });
			}
		}

		 public async void Load()
		{
			var result = await _client.GetContacts();

			Contacts.Clear();

			foreach (var item in result)
			{
				Contacts.Add(item);
			}
			IsBusy = false;
		}

	}
}
