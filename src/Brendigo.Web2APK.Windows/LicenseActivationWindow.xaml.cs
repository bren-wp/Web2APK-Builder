using Brendigo.Web2APK.Core;
using System.Windows;
namespace Brendigo.Web2APK.Windows;
public partial class LicenseActivationWindow : Window
{
    public string? ReplacingSerial {get;set;}
    public LicenseActivationWindow(){InitializeComponent();}
    private void Activate_Click(object sender,RoutedEventArgs e)
    {
        try {
            bool activated=ReplacingSerial is { Length: > 0 } oldSerial
                ? LicenseActivationStore.Replace(oldSerial,LicenseInput.Text.Trim(),"windows")
                : LicenseActivationStore.Activate(LicenseInput.Text.Trim(),"windows");
            if(!activated)
            {
                Message.Text="Licenca nije valjana, istekla je ili nije namijenjena Windows aplikaciji.";
                return;
            }
            DialogResult=true;
        }catch(Exception ex) {
            Message.Text="Aktivacija nije uspjela. Provjerite pravo upisa u korisničku mapu: "+ex.Message;
        }
    }
}
