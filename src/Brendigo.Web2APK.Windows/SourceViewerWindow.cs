using Brendigo.Web2APK.Core;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Brendigo.Web2APK.Windows;

/// <summary>Dark WPF source browser. All source remains local and access is rechecked on export.</summary>
public sealed class SourceViewerWindow : Window
{
    private readonly string _root;
    private readonly ProjectDefinition _project;
    private readonly ListBox _files;
    private readonly TextBox _source;

    public SourceViewerWindow(ProjectDefinition project)
    {
        LicenseActivationStore.Require("windows",project,"source");
        _project=project.Copy();
        _root=Path.Combine(Path.GetTempPath(),"Brendigo-SourceViewer",
            Guid.NewGuid().ToString("N"));
        try { AndroidProjectGenerator.Generate(_project,_root); }
        catch {
            try { if(Directory.Exists(_root))Directory.Delete(_root,true); }
            catch(IOException) {}
            throw;
        }
        Title="Izvorni kod · "+project.Name+" · Brendigo Web2APK";
        Icon=new System.Windows.Media.Imaging.BitmapImage(new Uri(
            "pack://application:,,,/Assets/Brendigo.ico"));
        Width=1130; Height=790; MinWidth=760;MinHeight=520;
        WindowStartupLocation=WindowStartupLocation.CenterOwner;
        WindowState=WindowState.Normal; ResizeMode=ResizeMode.CanResizeWithGrip;
        Background=Brush("#0B132A"); Foreground=Brush("#F0F4FF");

        var root=new Grid {Margin=new Thickness(20)};
        root.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
        root.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1,GridUnitType.Star)});
        root.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});

        var heading=new StackPanel{Margin=new Thickness(0,0,0,18)};
        heading.Children.Add(new TextBlock{
            Text="Izvorni Android projekt",FontSize=25,FontWeight=FontWeights.Bold});
        heading.Children.Add(new TextBlock{
            Text="Java, Gradle, AndroidManifest i resursi. Pregled i ZIP dostupni su plaćenim paketima.",
            Foreground=Brush("#A8B9DE"),FontSize=12,Margin=new Thickness(0,5,0,0),
            TextWrapping=TextWrapping.Wrap});
        Grid.SetRow(heading,0);root.Children.Add(heading);

        var grid=new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(275)});
        grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)});
        _files=new ListBox{Background=Brush("#15213C"),Foreground=Brush("#E5EAFB"),
            BorderBrush=Brush("#3C4C70"),BorderThickness=new Thickness(1),
            Padding=new Thickness(9),FontSize=12};
        var all=Directory.EnumerateFiles(_root,"*",SearchOption.AllDirectories)
            .Select(x=>Path.GetRelativePath(_root,x).Replace('\\','/'))
            .Where(x=>!x.StartsWith(".gradle/",StringComparison.OrdinalIgnoreCase))
            .OrderBy(x=>x,StringComparer.OrdinalIgnoreCase).ToArray();
        foreach(var name in all)_files.Items.Add(name);
        _files.SelectionChanged+=(_,_)=>LoadSelectedFile();
        Grid.SetColumn(_files,0);grid.Children.Add(_files);

        _source=new TextBox{Background=Brush("#0E1B35"),Foreground=Brush("#DCE7FF"),
            CaretBrush=Brush("#A897FA"),BorderBrush=Brush("#415582"),
            BorderThickness=new Thickness(1),FontFamily=new FontFamily("Consolas"),
            FontSize=12,IsReadOnly=true,TextWrapping=TextWrapping.NoWrap,
            VerticalScrollBarVisibility=ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,
            Padding=new Thickness(17),Margin=new Thickness(12,0,0,0)};
        Grid.SetColumn(_source,1);grid.Children.Add(_source);
        Grid.SetRow(grid,1);root.Children.Add(grid);

        var footer=new DockPanel{LastChildFill=false,Margin=new Thickness(0,18,0,0)};
        var note=new TextBlock{
            Text="Projekt je generiran lokalno · podrška: info@apk.com.hr",
            Foreground=Brush("#9CADCE"),FontSize=11,VerticalAlignment=VerticalAlignment.Center};
        DockPanel.SetDock(note,Dock.Left);footer.Children.Add(note);
        var download=MakeButton("Preuzmi izvorni kod (.zip)");
        download.Click+=(_,_)=>SaveZip();
        DockPanel.SetDock(download,Dock.Right);footer.Children.Add(download);
        var close=MakeButton("Zatvori");
        close.Click+=(_,_)=>Close();
        close.Margin=new Thickness(0,0,12,0);
        DockPanel.SetDock(close,Dock.Right);footer.Children.Add(close);
        Grid.SetRow(footer,2);root.Children.Add(footer);
        Content=root;
        if(_files.Items.Count>0)_files.SelectedIndex=0;
        Closed+=(_,_)=>{
            try{if(Directory.Exists(_root))Directory.Delete(_root,true);}
            catch(IOException) {}
            catch(UnauthorizedAccessException) {}
        };
    }

    private static Brush Brush(string color)=>
        new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
    private static Button MakeButton(string title)=>new(){
        Content=title,Padding=new Thickness(18,9,18,9),
        Foreground=Brush("#EFF1FF"),Background=Brush("#5141AD"),
        BorderBrush=Brush("#7667CD"),MinHeight=40,Cursor=System.Windows.Input.Cursors.Hand
    };

    private void LoadSelectedFile()
    {
        if(_files.SelectedItem is not string name)return;
        var path=Path.GetFullPath(Path.Combine(_root,name.Replace('/',Path.DirectorySeparatorChar)));
        var rooted=Path.GetFullPath(_root).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
        if(!path.StartsWith(rooted,StringComparison.OrdinalIgnoreCase)) {
            _source.Text="Neispravna putanja.";return;
        }
        try {
            if(new FileInfo(path).Length>1024*1024){
                _source.Text="Datoteka je prevelika za tekstualni pregled (>1 MB).";
                return;
            }
            if(!new[]{".java",".gradle",".xml",".properties",".json",".txt",".md",".html",
                     ".js",".css",".svg",".gitignore"}
                  .Contains(Path.GetExtension(path),StringComparer.OrdinalIgnoreCase)) {
                _source.Text="Binarna datoteka nije prikazana u tekstualnom pregledniku.";return;
            }
            _source.Text=File.ReadAllText(path);
            _source.ScrollToHome();
        }
        catch(IOException e){_source.Text="Ne mogu pročitati datoteku: "+e.Message;}
    }

    private void SaveZip()
    {
        try {
            // A cancelled destination dialog must not cause any additional license action.
            if(!LicenseActivationStore.TryAuthorize("windows",_project,"source",out _,out var reason))
                throw new UnauthorizedAccessException(reason);
            var dialog=new SaveFileDialog {
                Title="Spremi izvorni Android projekt",Filter="ZIP arhiva (*.zip)|*.zip",
                DefaultExt=".zip",FileName=_project.PackageName+"-android-source.zip"
            };
            if(dialog.ShowDialog(this)!=true)return;
            if(File.Exists(dialog.FileName)) {
                MessageBox.Show(this,"Datoteka već postoji. Odaberite novi naziv.",
                    "Izvoz",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            LicenseActivationStore.Require("windows",_project,"source");
            // Never write an export into the source tree being zipped; staging
            // beside the destination prevents incomplete user-visible archives.
            var destination=Path.GetFullPath(dialog.FileName);
            var sourcePrefix=Path.GetFullPath(_root)
                .TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
            if(destination.StartsWith(sourcePrefix,StringComparison.OrdinalIgnoreCase))
                throw new IOException("ZIP datoteku odaberite izvan mape izvornog koda.");
            var directory=Path.GetDirectoryName(destination) ??
                throw new IOException("Neispravna odredišna mapa.");
            var staging=Path.Combine(directory,"."+Path.GetFileName(destination)+
                "."+Guid.NewGuid().ToString("N")+".tmp");
            try
            {
                ZipFile.CreateFromDirectory(_root,staging,CompressionLevel.Optimal,
                    includeBaseDirectory:false);
                File.Move(staging,destination); // Fails safely if destination appeared.
            }
            finally
            {
                if(File.Exists(staging))File.Delete(staging);
            }
            MessageBox.Show(this,"ZIP izvornog koda je spremljen.","Izvoz",
                MessageBoxButton.OK,MessageBoxImage.Information);
        }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException or ArgumentException)
        {
            MessageBox.Show(this,e.Message,"Izvoz nije uspio",
                MessageBoxButton.OK,MessageBoxImage.Error);
        }
    }
}
