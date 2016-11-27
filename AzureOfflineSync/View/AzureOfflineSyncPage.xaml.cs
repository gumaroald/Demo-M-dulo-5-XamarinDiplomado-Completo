using Xamarin.Forms;

namespace AzureOfflineSync
{
	public partial class AzureOfflineSyncPage : ContentPage
	{
		public AzureOfflineSyncPage()
		{
			InitializeComponent();
			BindingContext = new ContactsVM();
		}
	}
}
