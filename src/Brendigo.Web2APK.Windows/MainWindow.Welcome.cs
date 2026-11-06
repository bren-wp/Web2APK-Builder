using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Brendigo.Web2APK.Windows;

public partial class MainWindow
{
    private void Welcome()
    {
        Heading("Dobrodošli u Brendigo Web2APK Builder",
            "Kreirajte Android aplikaciju iz web-stranice ili vlastitog HTML projekta.");
        var hero=new Grid { MinHeight=305 };
        hero.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(1,GridUnitType.Star) });
        _heroVisualColumn=new ColumnDefinition { Width=new GridLength(230) };
        hero.ColumnDefinitions.Add(_heroVisualColumn);
        var details=Stack();
        details.Margin=new Thickness(0,15,0,0);
        details.Children.Add(new Border {
            Background=BrushOf("#302864"),CornerRadius=new CornerRadius(13),
            HorizontalAlignment=HorizontalAlignment.Left,Padding=new Thickness(11,6,11,6),
            Child=Txt("✦  WINDOWS STUDIO",11,true,BrushOf("#C6B7FF"))
        });
        details.Children.Add(Txt("Pretvorite web u Android aplikaciju",29,true,White,
            new Thickness(0,19,0,12)));
        details.Children.Add(Txt("Od URL-a i lokalnih HTML datoteka do APK i AAB paketa, s kontrolom izgleda, prava pristupa i potpisivanja.",
            14,false,BrushOf("#C0CDEB"),new Thickness(0,0,0,17)));
        var buttons=Row();
        buttons.Children.Add(Btn("＋  Novi projekt",StartNew,true));
        buttons.Children.Add(Btn("▣  Moji projekti",()=>Navigate("projects")));
        details.Children.Add(buttons);
        Grid.SetColumn(details,0);hero.Children.Add(details);
        var icon=new Image {
            Source=new BitmapImage(new Uri("pack://application:,,,/Assets/Brendigo.ico")),
            Width=152,Height=152,Stretch=Stretch.Uniform,HorizontalAlignment=HorizontalAlignment.Center,
            VerticalAlignment=VerticalAlignment.Center
        };
        _heroPhone=icon;Grid.SetColumn(icon,1);hero.Children.Add(icon);
        PageHost.Children.Add(new Border {
            Child=hero,Background=new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString("#1E3D72"),
                (Color)ColorConverter.ConvertFromString("#40317B"),15),
            BorderThickness=new Thickness(1),BorderBrush=BrushOf("#4D4C8B"),
            CornerRadius=new CornerRadius(19),Padding=new Thickness(30,20,25,20),
            Margin=new Thickness(0,4,0,25),ClipToBounds=true
        });
        PageHost.Children.Add(Txt("SVE ŠTO VAM TREBA",11,true,BrushOf("#96A8D4"),
            new Thickness(2,0,0,12)));
        var features=Row();
        features.Children.Add(WelcomeFeature("🌐","Web i HTML","Pretvorite URL ili lokalni sadržaj u Android projekt."));
        features.Children.Add(WelcomeFeature("▧","APK i AAB","Izradite instalacijske i distribucijske pakete."));
        features.Children.Add(WelcomeFeature("◈","Potpisivanje","Koristite vlastiti ili novi lokalni sigurni ključ."));
        PageHost.Children.Add(features);
        var go=Row();
        go.Children.Add(Btn("Otvori nadzornu ploču  →",()=>Navigate("dashboard"),true));
        PageHost.Children.Add(go);
    }

    private Border WelcomeFeature(string icon,string title,string body)
    {
        var content=Stack();
        content.Children.Add(Txt(icon,29,true,BrushOf("#AB99FF"),new Thickness(0,0,0,8)));
        content.Children.Add(Txt(title,15,true,White,new Thickness(0,0,0,7)));
        content.Children.Add(Txt(body,12,false,Muted));
        var feature=Card(content,new Thickness(18));
        feature.Width=268;
        feature.MinHeight=159;
        return feature;
    }
}
