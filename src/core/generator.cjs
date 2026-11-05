"use strict";
const fs = require("node:fs");
const path = require("node:path");
const { normalize } = require("./projects.cjs");
function write(root, file, content) {
  const output = path.join(root, file);
  fs.mkdirSync(path.dirname(output), {recursive:true});
  fs.writeFileSync(output, content, "utf8");
}
function xml(value) { return String(value).replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;").replace(/'/g,"&apos;"); }
function java(value) { return JSON.stringify(String(value)); }
function copyWebsite(source, destination) {
  let total = 0, files = 0;
  const blocked = new Set([".git",".env","node_modules",".idea",".gradle","build","dist",".vscode"]);
  function visit(from, to) {
    for (const item of fs.readdirSync(from, {withFileTypes:true})) {
      if (blocked.has(item.name) || item.name.startsWith(".")) continue;
      const input = path.join(from,item.name), output = path.join(to,item.name);
      if (item.isSymbolicLink()) throw new Error("Simboličke poveznice nisu dopuštene u lokalnom web projektu.");
      if (item.isDirectory()) { fs.mkdirSync(output,{recursive:true}); visit(input,output); continue; }
      if (!item.isFile()) continue;
      if (++files > 1500) throw new Error("HTML projekt premašuje 1500 datoteka.");
      const stat = fs.statSync(input); total += stat.size;
      if (total > 80 * 1024 * 1024) throw new Error("HTML projekt premašuje 80 MB.");
      fs.copyFileSync(input, output);
    }
  }
  fs.mkdirSync(destination,{recursive:true});
  visit(source,destination);
}
function generateProject(input, destination) {
  const p = normalize(input);
  fs.mkdirSync(destination,{recursive:true});
  write(destination,"settings.gradle", "pluginManagement { repositories { google(); mavenCentral(); gradlePluginPortal() } }\ndependencyResolutionManagement { repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS); repositories { google(); mavenCentral() } }\nrootProject.name = 'BrendigoGeneratedApp'\ninclude ':app'\n");
  write(destination,"build.gradle", "plugins { id 'com.android.application' version '9.5.0' apply false }\n");
  write(destination,"gradle.properties", "org.gradle.jvmargs=-Xmx2048m -Dfile.encoding=UTF-8\nandroid.useAndroidX=true\n");
  write(destination,".gitignore",".gradle/\nbuild/\napp/build/\nlocal.properties\n*.jks\n*.keystore\n");
  const signing = "if (System.getenv('BRENDIGO_KEYSTORE')) { signingConfigs { release { storeFile file(System.getenv('BRENDIGO_KEYSTORE')); storePassword System.getenv('BRENDIGO_STORE_PASSWORD'); keyAlias System.getenv('BRENDIGO_KEY_ALIAS'); keyPassword System.getenv('BRENDIGO_KEY_PASSWORD') } } }";
  const buildType = "if (System.getenv('BRENDIGO_KEYSTORE')) { signingConfig signingConfigs.release }";
  write(destination,"app/build.gradle",
    "plugins { id 'com.android.application' }\n"+
    "android {\n  namespace '"+p.packageName+"'\n  compileSdk "+Math.max(36,p.targetSdk)+"\n"+
    "  defaultConfig { applicationId '"+p.packageName+"'; minSdk "+p.minSdk+"; targetSdk "+p.targetSdk+"; versionCode "+p.versionCode+"; versionName '"+p.versionName+"' }\n"+
    "  "+signing+"\n  buildTypes { release { minifyEnabled false; "+buildType+" } }\n"+
    "  compileOptions { sourceCompatibility JavaVersion.VERSION_17; targetCompatibility JavaVersion.VERSION_17 }\n}\n"+
    "dependencies { implementation 'androidx.webkit:webkit:1.14.0' }\n");
  const appAttrs = "android:label=\"@string/app_name\" android:icon=\"@drawable/ic_launcher\" android:allowBackup=\"false\" android:usesCleartextTraffic=\""+p.allowHttp+"\" android:theme=\"@style/AppTheme\"";
  const activityAttrs = "android:name=\".MainActivity\" android:exported=\"true\" android:screenOrientation=\""+p.orientation+"\"";
  write(destination,"app/src/main/AndroidManifest.xml",
    '<?xml version="1.0" encoding="utf-8"?>\n<manifest xmlns:android="http://schemas.android.com/apk/res/android">\n'+
    '  <uses-permission android:name="android.permission.INTERNET"/>\n'+
    (p.camera?'  <uses-permission android:name="android.permission.CAMERA"/>\n':'')+
    '  <application '+appAttrs+'>\n    <activity '+activityAttrs+'><intent-filter><action android:name="android.intent.action.MAIN"/><category android:name="android.intent.category.LAUNCHER"/></intent-filter></activity>\n  </application>\n</manifest>\n');
  write(destination,"app/src/main/res/values/strings.xml",'<?xml version="1.0" encoding="utf-8"?><resources><string name="app_name">'+xml(p.name)+'</string></resources>\n');
  write(destination,"app/src/main/res/values/styles.xml",'<resources><style name="AppTheme" parent="'+(p.theme==="dark"?"android:style/Theme.Material.NoActionBar":"android:style/Theme.Material.Light.NoActionBar")+'"><item name="android:fontFamily">sans</item><item name="android:colorAccent">'+xml(p.accent)+'</item></style></resources>\n');
  write(destination,"app/src/main/res/drawable/ic_launcher.xml",'<vector xmlns:android="http://schemas.android.com/apk/res/android" android:width="108dp" android:height="108dp" android:viewportWidth="108" android:viewportHeight="108"><path android:fillColor="#7253FF" android:pathData="M0,0h108v108h-108z"/><path android:strokeColor="#FFFFFF" android:strokeWidth="6" android:fillColor="@android:color/transparent" android:pathData="M30,26h26q24,0 10,20q17,14 -10,32h-26z"/><path android:strokeColor="#74DCFF" android:strokeWidth="3" android:fillColor="@android:color/transparent" android:pathData="M32,51h36 M32,67h31"/></vector>\n');
  const webUrl = p.sourceType==="local"?"https://appassets.androidplatform.net/assets/index.html":p.url;
  const local = p.sourceType==="local";
  const javaSrc =
    "package "+p.packageName+";\n"+
    "import android.app.Activity;\nimport android.os.Bundle;\nimport android.content.Intent;\nimport android.net.Uri;\nimport android.webkit.*;\nimport android.view.View;\nimport androidx.webkit.WebViewAssetLoader;\n\n"+
    "public final class MainActivity extends Activity {\n"+
    "  private WebView web;\n  private static final String HOME = "+java(webUrl)+";\n"+
    "  @Override public void onCreate(Bundle state) {\n    super.onCreate(state);\n"+
    (p.fullscreen?"    getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_FULLSCREEN);\n":"")+
    "    web = new WebView(this);\n    web.getSettings().setJavaScriptEnabled("+p.javascript+");\n"+
    "    web.getSettings().setDomStorageEnabled("+p.domStorage+");\n"+
    "    web.getSettings().setAllowFileAccess(false);\n    web.getSettings().setAllowContentAccess(false);\n"+
    "    web.getSettings().setAllowFileAccessFromFileURLs(false);\n    web.getSettings().setAllowUniversalAccessFromFileURLs(false);\n"+
    "    web.getSettings().setJavaScriptCanOpenWindowsAutomatically(false);\n    WebView.setWebContentsDebuggingEnabled(false);\n"+
    (local?"    final WebViewAssetLoader loader = new WebViewAssetLoader.Builder().addPathHandler(\"/assets/\", new WebViewAssetLoader.AssetsPathHandler(this)).build();\n":"")+
    "    web.setWebViewClient(new WebViewClient() {\n"+
    (local?"      @Override public WebResourceResponse shouldInterceptRequest(WebView view, WebResourceRequest req) { return loader.shouldInterceptRequest(req.getUrl()); }\n":"")+
    "      @Override public boolean shouldOverrideUrlLoading(WebView view, WebResourceRequest req) {\n"+
    "        Uri u=req.getUrl(); String scheme=u.getScheme();\n"+
    "        if (!\"https\".equalsIgnoreCase(scheme)"+(p.allowHttp?" && !\"http\".equalsIgnoreCase(scheme)":"")+") return true;\n"+
    (p.openExternal && local ? "        if (!\"appassets.androidplatform.net\".equalsIgnoreCase(u.getHost())) { startActivity(new Intent(Intent.ACTION_VIEW,u)); return true; }\n" : "")+
    "        return false;\n      }\n    });\n"+
    "    setContentView(web);\n    web.loadUrl(HOME);\n  }\n"+
    "  @Override public void onBackPressed() { if (web!=null && web.canGoBack()) web.goBack(); else super.onBackPressed(); }\n"+
    "  @Override protected void onDestroy() { if(web!=null) { web.destroy(); web=null; } super.onDestroy(); }\n}\n";
  write(destination,"app/src/main/java/"+p.packageName.replace(/\./g,"/")+"/MainActivity.java",javaSrc);
  if (local) copyWebsite(p.localPath,path.join(destination,"app/src/main/assets"));
  write(destination,"README.txt",
    "Generated by Brendigo Web2APK Builder.\nBuild requirements: JDK 17+, Android SDK 36, Gradle >=9.6, online access to Google Maven.\n"+
    "Build unsigned artifacts: gradle :app:assembleRelease :app:bundleRelease\n"+
    "For signed releases provide BRENDIGO_KEYSTORE, BRENDIGO_STORE_PASSWORD, BRENDIGO_KEY_ALIAS and BRENDIGO_KEY_PASSWORD as process environment variables.\n"+
    "Never commit keystores or credentials.\n");
  return {directory:destination,project:p};
}
module.exports={generateProject,xml,java,copyWebsite};
