using System.Text;
using System.Xml;

namespace Brendigo.Web2APK.Core;

public static class AndroidProjectGenerator
{
    private static void Put(string root, string relative, string data)
    {
        var path = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, data.Replace("\r\n", "\n"), new UTF8Encoding(false));
    }

    private static string XmlValue(string text) => System.Security.SecurityElement.Escape(text) ?? "";
    private static string JavaLiteral(string text) =>
        "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";

    public static string Generate(ProjectDefinition project, string outputDir)
    {
        project.Validate();
        var path = Path.GetFullPath(outputDir);
        if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
            throw new IOException("Odredišna mapa mora biti nova ili prazna.");
        if (project.SourceType == "local")
        {
            var root = Path.GetFullPath(project.LocalPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new IOException("Izlazna mapa ne smije biti unutar izvornog HTML projekta.");
        }
        Directory.CreateDirectory(path);

        Put(path, "settings.gradle",
            "pluginManagement { repositories { google(); mavenCentral(); gradlePluginPortal() } }\n" +
            "dependencyResolutionManagement { repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS); repositories { google(); mavenCentral() } }\n" +
            "rootProject.name = 'BrendigoGeneratedApp'\ninclude ':app'\n");
        Put(path, "build.gradle", "plugins { id 'com.android.application' version '9.5.0' apply false }\n");
        Put(path, "gradle.properties", "org.gradle.jvmargs=-Xmx2048m -Dfile.encoding=UTF-8\nandroid.useAndroidX=true\n");
        Put(path, ".gitignore", ".gradle/\nbuild/\napp/build/\nlocal.properties\n*.jks\n*.keystore\n");
        var buildGradle = """
plugins { id 'com.android.application' }
android {
    namespace '@@PACKAGE@@'
    compileSdk @@COMPILE@@
    defaultConfig {
        applicationId '@@PACKAGE@@'
        minSdk @@MIN@@
        targetSdk @@TARGET@@
        versionCode @@CODE@@
        versionName '@@VERSION@@'
    }
    signingConfigs {
        release {
            if (System.getenv('BRENDIGO_KEYSTORE')) {
                storeFile file(System.getenv('BRENDIGO_KEYSTORE'))
                storePassword System.getenv('BRENDIGO_STORE_PASSWORD')
                keyAlias System.getenv('BRENDIGO_KEY_ALIAS')
                keyPassword System.getenv('BRENDIGO_KEY_PASSWORD')
            }
        }
    }
    buildTypes {
        release {
            minifyEnabled false
            if (System.getenv('BRENDIGO_KEYSTORE')) signingConfig signingConfigs.release
        }
    }
    compileOptions {
        sourceCompatibility JavaVersion.VERSION_17
        targetCompatibility JavaVersion.VERSION_17
    }
}
dependencies { implementation 'androidx.webkit:webkit:1.14.0' }
""".Replace("@@PACKAGE@@", project.PackageName)
    .Replace("@@COMPILE@@", Math.Max(36, project.TargetSdk).ToString())
    .Replace("@@MIN@@", project.MinSdk.ToString())
    .Replace("@@TARGET@@", project.TargetSdk.ToString())
    .Replace("@@CODE@@", project.VersionCode.ToString())
    .Replace("@@VERSION@@", project.VersionName);
        Put(path, "app/build.gradle", buildGradle + "\n");

        var manifest = """
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <uses-permission android:name="android.permission.INTERNET" />
    <application android:label="@string/app_name" android:icon="@drawable/ic_launcher"
        android:allowBackup="false" android:usesCleartextTraffic="@@HTTP@@" android:theme="@style/AppTheme">
        <activity android:name=".MainActivity" android:exported="true"
            android:screenOrientation="@@ORIENTATION@@">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
        </activity>
    </application>
</manifest>
""".Replace("@@HTTP@@", project.AllowHttp.ToString().ToLowerInvariant())
    .Replace("@@ORIENTATION@@", project.Orientation);
        Put(path, "app/src/main/AndroidManifest.xml", manifest + "\n");
        Put(path, "app/src/main/res/values/strings.xml",
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><resources><string name=\"app_name\">" +
            XmlValue(project.Name) + "</string></resources>\n");
        Put(path, "app/src/main/res/values/styles.xml",
            "<resources><style name=\"AppTheme\" parent=\"" +
            (project.Theme == "dark" ? "android:style/Theme.Material.NoActionBar" : "android:style/Theme.Material.Light.NoActionBar") +
            "\"><item name=\"android:fontFamily\">sans</item><item name=\"android:colorAccent\">" +
            XmlValue(project.Accent) + "</item></style></resources>\n");
        Put(path, "app/src/main/res/drawable/ic_launcher.xml",
            "<vector xmlns:android=\"http://schemas.android.com/apk/res/android\" android:width=\"108dp\" android:height=\"108dp\" android:viewportWidth=\"108\" android:viewportHeight=\"108\">" +
            "<path android:fillColor=\"#7253FF\" android:pathData=\"M0,0h108v108h-108z\"/>" +
            "<path android:strokeColor=\"#FFFFFF\" android:strokeWidth=\"6\" android:fillColor=\"@android:color/transparent\" android:pathData=\"M30,26h26q24,0 10,20q17,14 -10,32h-26z\"/>" +
            "</vector>\n");

        var home = project.SourceType == "local" ?
            "https://appassets.androidplatform.net/assets/index.html" : project.Url;
        var java = """
package @@PACKAGE@@;
import android.app.Activity;
import android.os.Bundle;
import android.content.Intent;
import android.net.Uri;
import android.webkit.*;
import android.view.View;
import androidx.webkit.WebViewAssetLoader;

public final class MainActivity extends Activity {
    private WebView browser;
    private static final String HOME = @@HOME@@;
    private static final boolean LOCAL = @@LOCAL@@;
    private static final boolean EXTERNAL = @@EXTERNAL@@;
    private static final boolean HTTP = @@HTTP@@;

    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        if (@@FULLSCREEN@@) getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_FULLSCREEN);
        browser = new WebView(this);
        WebSettings settings = browser.getSettings();
        settings.setJavaScriptEnabled(@@JS@@);
        settings.setDomStorageEnabled(@@DOM@@);
        settings.setAllowFileAccess(false);
        settings.setAllowContentAccess(false);
        settings.setAllowFileAccessFromFileURLs(false);
        settings.setAllowUniversalAccessFromFileURLs(false);
        settings.setJavaScriptCanOpenWindowsAutomatically(false);
        WebView.setWebContentsDebuggingEnabled(false);
        final WebViewAssetLoader loader = new WebViewAssetLoader.Builder()
            .addPathHandler("/assets/", new WebViewAssetLoader.AssetsPathHandler(this)).build();
        browser.setWebViewClient(new WebViewClient() {
            @Override public WebResourceResponse shouldInterceptRequest(WebView view, WebResourceRequest request) {
                return LOCAL ? loader.shouldInterceptRequest(request.getUrl()) : null;
            }
            @Override public boolean shouldOverrideUrlLoading(WebView view, WebResourceRequest request) {
                Uri uri = request.getUrl();
                String scheme = uri.getScheme();
                if (!"https".equalsIgnoreCase(scheme) && !(HTTP && "http".equalsIgnoreCase(scheme))) return true;
                Uri origin = Uri.parse(HOME);
                boolean sameHost = origin.getHost() != null && origin.getHost().equalsIgnoreCase(uri.getHost());
                if (EXTERNAL && !sameHost) {
                    try { startActivity(new Intent(Intent.ACTION_VIEW, uri)); } catch (Exception ignored) { }
                    return true;
                }
                return false;
            }
        });
        setContentView(browser);
        browser.loadUrl(HOME);
    }
    @Override public void onBackPressed() {
        if (browser != null && browser.canGoBack()) browser.goBack(); else super.onBackPressed();
    }
    @Override protected void onDestroy() {
        if (browser != null) { browser.destroy(); browser = null; }
        super.onDestroy();
    }
}
""".Replace("@@PACKAGE@@", project.PackageName)
    .Replace("@@HOME@@", JavaLiteral(home))
    .Replace("@@LOCAL@@", (project.SourceType == "local").ToString().ToLowerInvariant())
    .Replace("@@EXTERNAL@@", project.OpenExternal.ToString().ToLowerInvariant())
    .Replace("@@HTTP@@", project.AllowHttp.ToString().ToLowerInvariant())
    .Replace("@@FULLSCREEN@@", (!project.StatusBar || project.Fullscreen).ToString().ToLowerInvariant())
    .Replace("@@JS@@", project.JavaScript.ToString().ToLowerInvariant())
    .Replace("@@DOM@@", project.DomStorage.ToString().ToLowerInvariant());
        Put(path, "app/src/main/java/" + project.PackageName.Replace('.', '/') + "/MainActivity.java", java + "\n");
        if (project.SourceType == "local")
            CopyLocalContent(project.LocalPath, Path.Combine(path, "app", "src", "main", "assets"));
        Put(path, "README.txt", "Generated by Brendigo Web2APK Builder native Windows Studio.\n" +
            "Requirements: JDK 17, Android SDK 36 or newer, Gradle 9.6+, Android Gradle Plugin 9.5.0.\n" +
            "Run: gradle --no-daemon :app:assembleRelease :app:bundleRelease\n" +
            "Signed release: set BRENDIGO_KEYSTORE, BRENDIGO_STORE_PASSWORD, BRENDIGO_KEY_ALIAS and BRENDIGO_KEY_PASSWORD.\n" +
            "Never store keystore passwords in source files. Verify app permissions and test on real devices before publication.\n");
        return path;
    }

    private static void CopyLocalContent(string from, string to)
    {
        var blocked = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".git", ".env", ".idea", ".gradle", ".vscode", "node_modules", "build", "dist" };
        int totalFiles = 0;
        long totalBytes = 0;
        void Copy(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            foreach (var entry in new DirectoryInfo(source).EnumerateFileSystemInfos())
            {
                if (entry.Name.StartsWith('.') || blocked.Contains(entry.Name)) continue;
                if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Simboličke poveznice nisu podržane.");
                if (entry is DirectoryInfo sub) Copy(sub.FullName, Path.Combine(destination, sub.Name));
                else if (entry is FileInfo file)
                {
                    if (++totalFiles > 1500 || (totalBytes += file.Length) > 80 * 1024 * 1024)
                        throw new IOException("Lokalni HTML projekt je prevelik (1500 datoteka / 80 MB).");
                    file.CopyTo(Path.Combine(destination, file.Name), overwrite: false);
                }
            }
        }
        Copy(from, to);
    }
}
