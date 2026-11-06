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
        Put(path, "build.gradle", "plugins { id 'com.android.application' version '9.5.0' apply false" +
            (project.FirebasePushEnabled ? "; id 'com.google.gms.google-services' version '4.5.0' apply false" : "") +
            " }\n");
        Put(path, "gradle.properties", "org.gradle.jvmargs=-Xmx2048m -Dfile.encoding=UTF-8\nandroid.useAndroidX=true\n");
        Put(path, ".gitignore", ".gradle/\nbuild/\napp/build/\nlocal.properties\n*.jks\n*.keystore\n");
        var buildGradle = """
plugins { id 'com.android.application' @@GOOGLE_PLUGIN@@ }
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
dependencies {
    implementation 'androidx.webkit:webkit:1.14.0'
    implementation 'androidx.core:core-splashscreen:1.0.1'
    @@ADS_DEPENDENCIES@@
    @@FIREBASE_DEPENDENCIES@@
    @@REFRESH_DEPENDENCIES@@
}
""".Replace("@@GOOGLE_PLUGIN@@", project.FirebasePushEnabled ? "; id 'com.google.gms.google-services'" : "")
    .Replace("@@ADS_DEPENDENCIES@@", project.AdMobEnabled ? "implementation 'com.google.android.gms:play-services-ads:25.5.0'\\n    implementation 'com.google.android.ump:user-messaging-platform:4.0.0'".Replace("\\n","\n") : "")
    .Replace("@@REFRESH_DEPENDENCIES@@", project.PullRefresh ? "implementation 'androidx.swiperefreshlayout:swiperefreshlayout:1.2.0'" : "")
    .Replace("@@FIREBASE_DEPENDENCIES@@", project.FirebasePushEnabled ? "implementation platform('com.google.firebase:firebase-bom:35.1.0')\\n    implementation 'com.google.firebase:firebase-messaging'".Replace("\\n","\n") : "")
    .Replace("@@PACKAGE@@", project.PackageName)
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
        var permissionXml =
            (project.Camera ? "    <uses-permission android:name=\"android.permission.CAMERA\" />\\n" : "") +
            (project.Microphone ? "    <uses-permission android:name=\"android.permission.RECORD_AUDIO\" />\\n" : "") +
            (project.Location ? "    <uses-permission android:name=\"android.permission.ACCESS_COARSE_LOCATION\" />\\n" : "") +
            (project.FirebasePushEnabled ? "    <uses-permission android:name=\"android.permission.POST_NOTIFICATIONS\" />\\n" : "");
        manifest = manifest.Replace("    <application", permissionXml.Replace("\\n", "\n") + "    <application");
        var iconResource = string.IsNullOrEmpty(project.IconPath) ? "@drawable/ic_launcher" : "@drawable/brand_icon";
        manifest = manifest.Replace("@drawable/ic_launcher",iconResource);
        // Adaptive launcher icons on API 26+, vector fallback on older Android versions.
        manifest = manifest.Replace("android:icon=\"" + iconResource + "\"", "android:icon=\"@mipmap/ic_launcher\" android:roundIcon=\"@mipmap/ic_launcher\"");
        if (project.Splash) manifest = manifest.Replace("android:theme=\"@style/AppTheme\"", "android:theme=\"@style/LaunchTheme\"");
        if (project.AdMobEnabled)
            manifest = manifest.Replace("    </application>",
                "        <meta-data android:name=\"com.google.android.gms.ads.APPLICATION_ID\" android:value=\"" +
                XmlValue(project.AdMobAppId) + "\" />\n    </application>");
        if (project.FirebasePushEnabled)
            manifest = manifest.Replace("    </application>",
                "        <service android:name=\".PushMessagingService\" android:exported=\"false\">" +
                "<intent-filter><action android:name=\"com.google.firebase.MESSAGING_EVENT\" /></intent-filter>" +
                "</service>\n    </application>");
        Put(path, "app/src/main/AndroidManifest.xml", manifest + "\n");
        var fallbackVector = "<vector xmlns:android=\"http://schemas.android.com/apk/res/android\" android:width=\"108dp\" android:height=\"108dp\" android:viewportWidth=\"108\" android:viewportHeight=\"108\">" +
            "<path android:fillColor=\"#7253FF\" android:pathData=\"M0,0h108v108h-108z\"/>" +
            "<path android:strokeColor=\"#FFFFFF\" android:strokeWidth=\"6\" android:fillColor=\"@android:color/transparent\" android:pathData=\"M30,26h26q24,0 10,20q17,14 -10,32h-26z\"/>" +
            "</vector>\n";
        Put(path, "app/src/main/res/mipmap-anydpi/ic_launcher.xml", fallbackVector);
        Put(path, "app/src/main/res/mipmap-anydpi-v26/ic_launcher.xml",
            "<adaptive-icon xmlns:android=\"http://schemas.android.com/apk/res/android\">" +
            "<background android:drawable=\"@color/icon_bg\" />" +
            "<foreground android:drawable=\"" + iconResource + "\" />" +
            "<monochrome android:drawable=\"@drawable/icon_monochrome\" />" +
            "</adaptive-icon>\n");
        Put(path, "app/src/main/res/values/icon_colors.xml",
            "<resources><color name=\"icon_bg\">" + XmlValue(project.Accent) + "</color></resources>\n");
        Put(path, "app/src/main/res/drawable/icon_monochrome.xml",
            "<vector xmlns:android=\"http://schemas.android.com/apk/res/android\" android:width=\"108dp\" android:height=\"108dp\" android:viewportWidth=\"108\" android:viewportHeight=\"108\">" +
            "<path android:fillColor=\"#FFFFFF\" android:pathData=\"M28,16h29c33,0 39,32 15,41 22,12 16,39 -15,39H28z\"/></vector>\n");
        Put(path, "app/src/main/res/values/strings.xml",
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><resources><string name=\"app_name\">" +
            XmlValue(project.Name) + "</string></resources>\n");
        Put(path, "app/src/main/res/values/styles.xml",
            "<resources><style name=\"AppTheme\" parent=\"" +
            (project.Theme == "dark" ? "android:style/Theme.Material.NoActionBar" : "android:style/Theme.Material.Light.NoActionBar") +
            "\"><item name=\"android:fontFamily\">sans</item><item name=\"android:colorAccent\">" +
            XmlValue(project.Accent) + "</item></style>" + (project.Splash ? "<style name=\"LaunchTheme\" parent=\"Theme.SplashScreen\"><item name=\"windowSplashScreenBackground\">" + (project.Theme == "dark" ? "#121D39" : "#FFFFFF") + "</item><item name=\"windowSplashScreenAnimatedIcon\">" + iconResource + "</item><item name=\"postSplashScreenTheme\">@style/AppTheme</item></style>" : "") + "</resources>\n");
        Put(path, "app/src/main/res/drawable/ic_launcher.xml",
            "<vector xmlns:android=\"http://schemas.android.com/apk/res/android\" android:width=\"108dp\" android:height=\"108dp\" android:viewportWidth=\"108\" android:viewportHeight=\"108\">" +
            "<path android:fillColor=\"#7253FF\" android:pathData=\"M0,0h108v108h-108z\"/>" +
            "<path android:strokeColor=\"#FFFFFF\" android:strokeWidth=\"6\" android:fillColor=\"@android:color/transparent\" android:pathData=\"M30,26h26q24,0 10,20q17,14 -10,32h-26z\"/>" +
            "</vector>\n");

        if (!string.IsNullOrEmpty(project.IconPath))
        {
            var iconDest = Path.Combine(path,"app","src","main","res","drawable-nodpi","brand_icon.png");
            Directory.CreateDirectory(Path.GetDirectoryName(iconDest)!);
            File.Copy(project.IconPath,iconDest,overwrite:true);
        }
        if (project.Splash)
        {
            Put(path,"app/src/main/res/drawable/launch_background.xml",
                "<layer-list xmlns:android=\"http://schemas.android.com/apk/res/android\">" +
                "<item><shape android:shape=\"rectangle\"><solid android:color=\"" + (project.Theme == "dark" ? "#121D39" : "#FFFFFF") + "\" /></shape></item>" +
                "<item android:drawable=\""+iconResource+"\" android:gravity=\"center\" />" +
                "</layer-list>\n");
        }
        if (project.FirebasePushEnabled)
        {
            File.Copy(project.FirebaseConfigPath,Path.Combine(path,"app","google-services.json"),overwrite:false);
            Put(path,"app/src/main/java/"+project.PackageName.Replace('.','/')+"/PushMessagingService.java",
                GeneratePushService(project.PackageName,project.Name));
        }
        var home = project.SourceType == "local" ?
            "https://appassets.androidplatform.net/assets/index.html" : project.Url;
        var java = """
package @@PACKAGE@@;
import android.Manifest;
import android.app.Activity;
import android.os.SystemClock;
import androidx.core.splashscreen.SplashScreen;
import android.app.DownloadManager;
import android.content.Context;
import android.content.pm.PackageManager;
import android.os.Environment;
import java.util.ArrayList;
import java.util.List;
import android.app.AlertDialog;
import android.os.Bundle;
import android.content.Intent;
import android.net.Uri;
import android.webkit.*;
import android.view.View;
import androidx.webkit.WebViewAssetLoader;

public final class MainActivity extends Activity {
    private WebView browser;
    private android.view.View mainContent;
    @@REFRESH_FIELD@@
    @@ADS_CLASS@@
    @@NON_AD_PRIVACY_MENU@@
    private static final String HOME = @@HOME@@;
    private static final String PRIVACY_URL = @@PRIVACY_URL@@;
    private static final boolean LOCAL = @@LOCAL@@;
    private static final boolean EXTERNAL = @@EXTERNAL@@;
    private void openPrivacyPolicy() {
        if (PRIVACY_URL.isEmpty()) return;
        try { startActivity(new Intent(Intent.ACTION_VIEW,Uri.parse(PRIVACY_URL))); }
        catch (Exception ignored) { }
    }
    private static final boolean HTTP = @@HTTP@@;
    private static final boolean SPLASH = @@SPLASH@@;
    private static final boolean PULL = @@PULL@@;
    private static final long SPLASH_MS = @@SPLASH_MS@@;
    private static final boolean CAMERA = @@CAMERA@@;
    private static final boolean MICROPHONE = @@MICROPHONE@@;
    private static final boolean GEO = @@GEO@@;
    private static final boolean UPLOADS = @@UPLOADS@@;
    private static final boolean DOWNLOADS = @@DOWNLOADS@@;
    private static final int FILE_CHOOSER_CODE = 6102;
    private static final int MEDIA_PERMISSION_CODE = 6103;
    private static final int LOCATION_PERMISSION_CODE = 6104;
    private ValueCallback<Uri[]> selectedFiles;
    private PermissionRequest pendingMedia;
    private String[] pendingResources;
    private GeolocationPermissions.Callback locationCallback;
    private String locationOrigin;
    private static final String[] EMPTY = new String[0];

    private android.view.View contentForWebView() {
        if(mainContent!=null)return mainContent;
        @@REFRESH_INIT@@
        mainContent=browser;
        return mainContent;
    }
    private void applySafeAreaInsets(android.view.View content) {
        if (@@FULLSCREEN@@) return;
        content.setOnApplyWindowInsetsListener((view,insets) -> {
            int left,top,right,bottom;
            if (android.os.Build.VERSION.SDK_INT >= 30) {
                android.graphics.Insets bars=insets.getInsets(android.view.WindowInsets.Type.systemBars());
                left=bars.left; top=bars.top; right=bars.right; bottom=bars.bottom;
            } else {
                left=insets.getSystemWindowInsetLeft(); top=insets.getSystemWindowInsetTop();
                right=insets.getSystemWindowInsetRight(); bottom=insets.getSystemWindowInsetBottom();
            }
            view.setPadding(left,top,right,bottom);
            return insets;
        });
        content.requestApplyInsets();
    }

    private boolean trusted(Uri uri) {
        if (uri == null || uri.getHost() == null) return false;
        Uri home = Uri.parse(HOME);
        String scheme = uri.getScheme();
        return uri.getHost().equalsIgnoreCase(home.getHost()) &&
            ("https".equalsIgnoreCase(scheme) || (HTTP && "http".equalsIgnoreCase(scheme)));
    }


    @Override public void onCreate(Bundle state) {
        long started = SystemClock.uptimeMillis();
        SplashScreen splash = SPLASH ? SplashScreen.installSplashScreen(this) : null;
        super.onCreate(state);
        if (splash != null) splash.setKeepOnScreenCondition(() -> SystemClock.uptimeMillis() - started < SPLASH_MS);
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
        settings.setGeolocationEnabled(GEO);
        browser.setWebChromeClient(new WebChromeClient() {
            @Override public boolean onShowFileChooser(WebView view, ValueCallback<Uri[]> callback,
                    FileChooserParams params) {
                if (!UPLOADS) return false;
                if (selectedFiles != null) selectedFiles.onReceiveValue(null);
                selectedFiles = callback;
                try {
                    startActivityForResult(params.createIntent(), FILE_CHOOSER_CODE);
                } catch (Exception failure) {
                    selectedFiles.onReceiveValue(null);
                    selectedFiles = null;
                }
                return true;
            }
            @Override public void onPermissionRequest(PermissionRequest request) {
                runOnUiThread(() -> {
                    if (!trusted(request.getOrigin())) { request.deny(); return; }
                    List<String> perms = new ArrayList<>();
                    for (String resource : request.getResources()) {
                        if (PermissionRequest.RESOURCE_VIDEO_CAPTURE.equals(resource) && CAMERA) {
                            if (checkSelfPermission(Manifest.permission.CAMERA) != PackageManager.PERMISSION_GRANTED)
                                perms.add(Manifest.permission.CAMERA);
                        } else if (PermissionRequest.RESOURCE_AUDIO_CAPTURE.equals(resource) && MICROPHONE) {
                            if (checkSelfPermission(Manifest.permission.RECORD_AUDIO) != PackageManager.PERMISSION_GRANTED)
                                perms.add(Manifest.permission.RECORD_AUDIO);
                        } else { request.deny(); return; }
                    }
                    if (perms.isEmpty()) request.grant(request.getResources());
                    else {
                        if (pendingMedia != null) pendingMedia.deny();
                        pendingMedia=request;
                        pendingResources=request.getResources();
                        requestPermissions(perms.toArray(EMPTY),MEDIA_PERMISSION_CODE);
                    }
                });
            }
            @Override public void onGeolocationPermissionsShowPrompt(String origin, GeolocationPermissions.Callback callback) {
                if (!GEO || !trusted(Uri.parse(origin))) { callback.invoke(origin,false,false); return; }
                if (checkSelfPermission(Manifest.permission.ACCESS_COARSE_LOCATION)==PackageManager.PERMISSION_GRANTED)
                    callback.invoke(origin,true,false);
                else {
                    if (locationCallback != null) locationCallback.invoke(locationOrigin,false,false);
                    locationCallback=callback;
                    locationOrigin=origin;
                    requestPermissions(new String[] { Manifest.permission.ACCESS_COARSE_LOCATION },LOCATION_PERMISSION_CODE);
                }
            }
        });
        browser.setWebViewClient(new WebViewClient() {
            @Override public void onPageFinished(WebView view,String url) {
                super.onPageFinished(view,url);
                @@REFRESH_STOP@@
            }
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
        if (DOWNLOADS) browser.setDownloadListener((address,agent,disposition,mime,length) -> {
            try {
                Uri uri=Uri.parse(address);
                String protocol=uri.getScheme();
                if (uri.getHost()==null || !("https".equalsIgnoreCase(protocol) ||
                        (HTTP && "http".equalsIgnoreCase(protocol)))) return;
                DownloadManager manager=(DownloadManager)getSystemService(Context.DOWNLOAD_SERVICE);
                if (manager==null) return;
                String safe=URLUtil.guessFileName(address,disposition,mime).replaceAll("[^a-zA-Z0-9._-]","_");
                if (safe.startsWith(".")) safe="download_"+safe.replace(".","_");
                if (safe.length()>100) safe=safe.substring(safe.length()-100);
                if (safe.isEmpty()) safe="download.bin";
                DownloadManager.Request req=new DownloadManager.Request(uri);
                req.setDestinationInExternalFilesDir(this,Environment.DIRECTORY_DOWNLOADS,safe);
                req.setNotificationVisibility(DownloadManager.Request.VISIBILITY_VISIBLE_NOTIFY_COMPLETED);
                manager.enqueue(req);
            } catch (Exception ignored) { }
        });
        @@CONTENT_VIEW@@
        browser.loadUrl(HOME);
        @@PUSH_PROMPT@@
    }
    @Override protected void onActivityResult(int requestCode,int resultCode,Intent data) {
        super.onActivityResult(requestCode,resultCode,data);
        if (requestCode == FILE_CHOOSER_CODE && selectedFiles != null) {
            selectedFiles.onReceiveValue(WebChromeClient.FileChooserParams.parseResult(resultCode,data));
            selectedFiles=null;
        }
    }
    @Override public void onRequestPermissionsResult(int requestCode,String[] permissions,int[] results) {
        super.onRequestPermissionsResult(requestCode,permissions,results);
        if (requestCode == MEDIA_PERMISSION_CODE && pendingMedia != null) {
            boolean granted = results.length > 0;
            for (int code : results) granted &= code==PackageManager.PERMISSION_GRANTED;
            if (granted) pendingMedia.grant(pendingResources);
            else pendingMedia.deny();
            pendingMedia=null;
            pendingResources=null;
        } else if (requestCode == LOCATION_PERMISSION_CODE && locationCallback != null) {
            boolean granted=results.length>0 && results[0]==PackageManager.PERMISSION_GRANTED;
            locationCallback.invoke(locationOrigin,granted,false);
            locationCallback=null;
            locationOrigin=null;
        }
    }
    @Override public void onBackPressed() {
        if (browser != null && browser.canGoBack()) browser.goBack(); else super.onBackPressed();
    }
    @Override protected void onDestroy() {
        @@ADS_DESTROY@@
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
    .Replace("@@PRIVACY_URL@@",JavaLiteral(project.PrivacyPolicyUrl))
    .Replace("@@SPLASH@@", project.Splash.ToString().ToLowerInvariant())
    .Replace("@@SPLASH_MS@@", project.SplashDurationMs.ToString())
    .Replace("@@ADS_CLASS@@",project.AdMobEnabled ? GenerateAdsClass(project.AdMobBannerId) : "")
    .Replace("@@NON_AD_PRIVACY_MENU@@",project.AdMobEnabled ? "" : GeneratePrivacyMenu())
    .Replace("@@CONTENT_VIEW@@",project.AdMobEnabled ? "setupAds();" :
        "android.view.View main=contentForWebView(); setContentView(main); applySafeAreaInsets(main);")
    .Replace("@@REFRESH_FIELD@@",project.PullRefresh ?
        "private androidx.swiperefreshlayout.widget.SwipeRefreshLayout refreshLayout;" : "")
    .Replace("@@REFRESH_INIT@@",project.PullRefresh ?
        "refreshLayout=new androidx.swiperefreshlayout.widget.SwipeRefreshLayout(this); "+
        "refreshLayout.addView(browser); "+
        "refreshLayout.setOnChildScrollUpCallback((parent,child)->browser.canScrollVertically(-1)); "+
        "refreshLayout.setOnRefreshListener(() -> browser.reload()); "+
        "mainContent=refreshLayout;return mainContent;" : "")
    .Replace("@@REFRESH_STOP@@",project.PullRefresh ?
        "if(refreshLayout!=null)refreshLayout.setRefreshing(false);" : "")
    .Replace("@@PULL@@",project.PullRefresh.ToString().ToLowerInvariant())
    .Replace("@@ADS_DESTROY@@",project.AdMobEnabled ? "if (adView != null) { adView.destroy(); adView=null; }" : "")
    .Replace("@@PUSH_PROMPT@@",project.FirebasePushEnabled ?
        "if (android.os.Build.VERSION.SDK_INT >= 33 && checkSelfPermission(android.Manifest.permission.POST_NOTIFICATIONS) != android.content.pm.PackageManager.PERMISSION_GRANTED) requestPermissions(new String[] { android.Manifest.permission.POST_NOTIFICATIONS },7100);" : "")
    .Replace("@@DOM@@", project.DomStorage.ToString().ToLowerInvariant())
    .Replace("@@CAMERA@@", project.Camera.ToString().ToLowerInvariant())
    .Replace("@@MICROPHONE@@", project.Microphone.ToString().ToLowerInvariant())
    .Replace("@@GEO@@", project.Location.ToString().ToLowerInvariant())
    .Replace("@@UPLOADS@@", project.FileUploads.ToString().ToLowerInvariant())
    .Replace("@@DOWNLOADS@@", project.Downloads.ToString().ToLowerInvariant());
        Put(path, "app/src/main/java/" + project.PackageName.Replace('.', '/') + "/MainActivity.java", java + "\n");
        if (project.SourceType == "local")
            CopyLocalContent(project.LocalPath, Path.Combine(path, "app", "src", "main", "assets"));
        Put(path, "GOOGLE_PLAY_CHECKLIST.txt", GooglePlayReadiness.AsText(project));
        Put(path, "README.txt", "Generated by Brendigo Web2APK Builder native Windows Studio.\n" +
            "Requirements: JDK 17, Android SDK 36 or newer, Gradle 9.6+, Android Gradle Plugin 9.5.0.\n" +
            "Run: gradle --no-daemon :app:assembleRelease :app:bundleRelease\n" +
            "Signed release: set BRENDIGO_KEYSTORE, BRENDIGO_STORE_PASSWORD, BRENDIGO_KEY_ALIAS and BRENDIGO_KEY_PASSWORD.\n" +
            "Never store keystore passwords in source files. Verify app permissions and test on real devices before publication.\n");
        return path;
    }

    private static string GeneratePrivacyMenu() => """
    @Override public boolean onCreateOptionsMenu(android.view.Menu menu) {
        if (!PRIVACY_URL.isEmpty()) menu.add(0,2001,0,"Politika privatnosti");
        return !PRIVACY_URL.isEmpty();
    }
    @Override public boolean onOptionsItemSelected(android.view.MenuItem item) {
        if (item.getItemId()==2001) { openPrivacyPolicy(); return true; }
        return super.onOptionsItemSelected(item);
    }
""";

    private static string GenerateAdsClass(string bannerId) => """
    private com.google.android.gms.ads.AdView adView;
    private void setupAds() {
        android.widget.LinearLayout holder = new android.widget.LinearLayout(this);
        holder.setOrientation(android.widget.LinearLayout.VERTICAL);
        holder.addView(contentForWebView(), new android.widget.LinearLayout.LayoutParams(-1,0,1f));
        android.widget.Button privacy = new android.widget.Button(this);
        privacy.setText("Postavke privatnosti oglasa");
        privacy.setOnClickListener(v -> com.google.android.ump.UserMessagingPlatform.showPrivacyOptionsForm(this,error -> {}));
        holder.addView(privacy, new android.widget.LinearLayout.LayoutParams(-1,-2));
        setContentView(holder);
        applySafeAreaInsets(holder);
        com.google.android.ump.ConsentInformation consent =
            com.google.android.ump.UserMessagingPlatform.getConsentInformation(this);
        com.google.android.ump.ConsentRequestParameters params =
            new com.google.android.ump.ConsentRequestParameters.Builder().build();
        consent.requestConsentInfoUpdate(this,params,
            () -> com.google.android.ump.UserMessagingPlatform.loadAndShowConsentFormIfRequired(this,
                error -> { if (consent.canRequestAds()) loadBanner(holder); }),
            error -> { if (consent.canRequestAds()) loadBanner(holder); });
    }
    private void loadBanner(android.widget.LinearLayout holder) {
        if (adView != null) return;
        com.google.android.gms.ads.MobileAds.initialize(this, status -> {});
        adView = new com.google.android.gms.ads.AdView(this);
        adView.setAdUnitId("@@BANNER_ID@@");
        adView.setAdSize(com.google.android.gms.ads.AdSize.BANNER);
        holder.addView(adView, new android.widget.LinearLayout.LayoutParams(-1,-2));
        adView.loadAd(new com.google.android.gms.ads.AdRequest.Builder().build());
    }
    @Override public boolean onCreateOptionsMenu(android.view.Menu menu) {
        com.google.android.ump.ConsentInformation ci =
            com.google.android.ump.UserMessagingPlatform.getConsentInformation(this);
        if (ci.getPrivacyOptionsRequirementStatus() ==
            com.google.android.ump.ConsentInformation.PrivacyOptionsRequirementStatus.REQUIRED)
            menu.add(0,1999,0,"Postavke privatnosti");
        if (!PRIVACY_URL.isEmpty()) menu.add(0,2001,1,"Politika privatnosti");
        return true;
    }
    @Override public boolean onOptionsItemSelected(android.view.MenuItem item) {
        if (item.getItemId()==1999) {
            com.google.android.ump.UserMessagingPlatform.showPrivacyOptionsForm(this, error -> {});
            return true;
        }
        if (item.getItemId()==2001) { openPrivacyPolicy(); return true; }
        return super.onOptionsItemSelected(item);
    }
""".Replace("@@BANNER_ID@@",bannerId);

    private static string GeneratePushService(string packageName,string appName) => """
package @@PACKAGE@@;
import com.google.firebase.messaging.FirebaseMessagingService;
import com.google.firebase.messaging.RemoteMessage;
import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Intent;
import android.os.Build;
public final class PushMessagingService extends FirebaseMessagingService {
    private static final String CHANNEL = "web2apk_push";
    @Override public void onMessageReceived(RemoteMessage message) {
        String title = message.getNotification()!=null ? message.getNotification().getTitle() : null;
        String body = message.getNotification()!=null ? message.getNotification().getBody() : null;
        if (title==null && message.getData().isEmpty()) return;
        if (title==null) title=message.getData().get("title");
        if (body==null) body=message.getData().get("body");
        NotificationManager manager = (NotificationManager)getSystemService(NOTIFICATION_SERVICE);
        if (manager==null) return;
        if (Build.VERSION.SDK_INT>=26) {
            NotificationChannel channel = new NotificationChannel(CHANNEL,"Obavijesti",NotificationManager.IMPORTANCE_DEFAULT);
            manager.createNotificationChannel(channel);
        }
        Intent launch=new Intent(this,MainActivity.class);
        launch.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
        PendingIntent pending = PendingIntent.getActivity(this,0,launch,PendingIntent.FLAG_UPDATE_CURRENT|PendingIntent.FLAG_IMMUTABLE);
        Notification.Builder notification=Build.VERSION.SDK_INT>=26
            ? new Notification.Builder(this,CHANNEL) : new Notification.Builder(this);
        notification.setContentTitle(title==null?"@@APPNAME@@":title).setContentText(body==null?"":body)
            .setSmallIcon(android.R.drawable.ic_dialog_info).setContentIntent(pending).setAutoCancel(true);
        manager.notify((int)(System.currentTimeMillis()%Integer.MAX_VALUE),notification.build());
    }
}
""".Replace("@@PACKAGE@@",packageName)
    .Replace("@@APPNAME@@",appName.Replace("\"","").Replace("\n"," "));

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
