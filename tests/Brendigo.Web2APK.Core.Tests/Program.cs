using Brendigo.Web2APK.Core;

static void Test(bool valid, string name) {
    if (!valid) throw new InvalidOperationException("FAILED: "+name);
    Console.WriteLine("PASS "+name);
}
static bool Throws(Action action) {
    try { action(); return false; } catch (ArgumentException) { return true; }
}
if (args.Length == 2 && args[0]=="--fixture") {
    var fixture = new ProjectDefinition { Name="Build Smoke", PackageName="com.brendigo.smoketest",
        Url="https://example.com", Camera=true, Microphone=true, Location=true,
        FileUploads=true, Downloads=true, Splash=true };
    AndroidProjectGenerator.Generate(fixture,args[1]);
    Console.WriteLine("Fixture generated: "+args[1]);
    return;
}
if (args.Length == 2 && args[0]=="--ads-fixture") {
    var fixture = new ProjectDefinition {
        Name="AdMob Build Smoke", PackageName="com.brendigo.adsfixture",
        Url="https://example.com", AdMobEnabled=true,
        AdMobAppId="ca-app-pub-3940256099942544~3347511713",
        AdMobBannerId="ca-app-pub-3940256099942544/6300978111"
    };
    AndroidProjectGenerator.Generate(fixture,args[1]);
    Console.WriteLine("AdMob test fixture generated");
    return;
}
var project = new ProjectDefinition { Name="Primjer", PackageName="com.brendigo.primjer", Url="https://example.com" };
project.Validate();
Test(project.MinSdk==24 && project.TargetSdk==36,"SDK defaults");
Test(Throws(()=>new ProjectDefinition{Name="Test",Url="http://example.com"}.Validate()),"HTTP denied");
Test(Throws(()=>new ProjectDefinition{Name="Test",PackageName="../attack"}.Validate()),"Package validated");
Test(Throws(()=>new ProjectDefinition{Name="Test",Url="javascript:alert(1)"}.Validate()),"Non-https denied");
Test(Throws(()=>new ProjectDefinition{Name="Test",TargetSdk=35}.Validate()),"Old target denied");
var root=Path.Combine(Path.GetTempPath(),"brendigo-tests-"+Guid.NewGuid().ToString("N"));
try {
    Directory.CreateDirectory(root);
    var store=new ProjectStore(Path.Combine(root,"data"));
    var saved=store.Save(project);
    Test(store.List().Count==1 && store.List()[0].Id==saved.Id,"Save/load");
    var next=saved.Copy();next.Name="Nova verzija";store.Save(next);
    Test(store.List().Count==1 && store.List()[0].Name=="Nova verzija","Update");
    var path=AndroidProjectGenerator.Generate(project,Path.Combine(root,"android"));
    var manifest=File.ReadAllText(Path.Combine(path,"app","src","main","AndroidManifest.xml"));
    var java=File.ReadAllText(Path.Combine(path,"app","src","main","java","com","brendigo","primjer","MainActivity.java"));
    var gradle=File.ReadAllText(Path.Combine(path,"app","build.gradle"));
    Test(manifest.Contains("usesCleartextTraffic=\"false\""),"HTTPS default");
    Test(java.Contains("setAllowUniversalAccessFromFileURLs(false)"),"WebView isolation");
    Test(gradle.Contains("targetSdk 36") && gradle.Contains("minSdk 24"),"SDK config");
    var permissions=new ProjectDefinition { Name="All permissions", PackageName="com.brendigo.permissions",
        Url="https://example.com", Camera=true, Microphone=true, Location=true,
        FileUploads=true, Downloads=true, Splash=true };
    var enabled=AndroidProjectGenerator.Generate(permissions,Path.Combine(root,"permissions"));
    var permittedManifest=File.ReadAllText(Path.Combine(enabled,"app","src","main","AndroidManifest.xml"));
    var permittedJava=File.ReadAllText(Path.Combine(enabled,"app","src","main","java","com","brendigo","permissions","MainActivity.java"));
    Test(permittedManifest.Contains("android.permission.CAMERA") &&
         permittedManifest.Contains("android.permission.RECORD_AUDIO") &&
         permittedManifest.Contains("android.permission.ACCESS_COARSE_LOCATION"),"Permission manifest");
    Test(permittedJava.Contains("onPermissionRequest") &&
         permittedJava.Contains("onShowFileChooser") &&
         permittedJava.Contains("DownloadManager") &&
         permittedJava.Contains("onGeolocationPermissionsShowPrompt"),"WebView permission implementations");
    Test(File.Exists(Path.Combine(enabled,"app","src","main","res","drawable","launch_background.xml")),"Splash drawable generated");
    Test(Throws(()=>new ProjectDefinition { Name="Invalid AdMob",
        AdMobEnabled=true }.Validate()),"AdMob identifiers required");
    var ads=new ProjectDefinition { Name="AdMob Test",
        PackageName="com.brendigo.ads", Url="https://example.com",
        AdMobEnabled=true,
        AdMobAppId="ca-app-pub-3940256099942544~3347511713",
        AdMobBannerId="ca-app-pub-3940256099942544/6300978111" };
    var adsOutput=AndroidProjectGenerator.Generate(ads,Path.Combine(root,"ads"));
    var adsGradle=File.ReadAllText(Path.Combine(adsOutput,"app","build.gradle"));
    var adsManifest=File.ReadAllText(Path.Combine(adsOutput,"app","src","main","AndroidManifest.xml"));
    var adsJava=File.ReadAllText(Path.Combine(adsOutput,"app","src","main","java","com","brendigo","ads","MainActivity.java"));
    Test(adsGradle.Contains("play-services-ads:25.5.0") &&
        adsGradle.Contains("user-messaging-platform:4.0.0"),"Ads SDK and UMP dependencies");
    Test(adsManifest.Contains("com.google.android.gms.ads.APPLICATION_ID") &&
        adsJava.Contains("requestConsentInfoUpdate") &&
        adsJava.Contains("showPrivacyOptionsForm"),"Consent-gated AdMob integration");
    var firebaseJson=Path.Combine(root,"google-services.json");
    File.WriteAllText(firebaseJson,"{\"client\":[{\"client_info\":{\"android_client_info\":{\"package_name\":\"com.brendigo.firebase\"}}}]}");
    var firebase=new ProjectDefinition { Name="Firebase Test",
        PackageName="com.brendigo.firebase", Url="https://example.com",
        FirebasePushEnabled=true, FirebaseConfigPath=firebaseJson };
    var firebaseOutput=AndroidProjectGenerator.Generate(firebase,Path.Combine(root,"firebase"));
    Test(File.Exists(Path.Combine(firebaseOutput,"app","google-services.json")),"Firebase config copied locally");
    Test(File.ReadAllText(Path.Combine(firebaseOutput,"app","build.gradle")).Contains("firebase-messaging"),"Firebase SDK integration");
    Test(File.ReadAllText(Path.Combine(firebaseOutput,"app","src","main","AndroidManifest.xml"))
        .Contains("android.permission.POST_NOTIFICATIONS"),"FCM Android 13 notification permission");
    Test(File.Exists(Path.Combine(firebaseOutput,"app","src","main","java","com","brendigo","firebase","PushMessagingService.java")),
        "Firebase message handler generated");
    Test(File.Exists(Path.Combine(adsOutput,"app","src","main","res","mipmap-anydpi-v26","ic_launcher.xml")),
        "Adaptive launcher icon generated");
    var web=Path.Combine(root,"web");Directory.CreateDirectory(web);
    File.WriteAllText(Path.Combine(web,"index.html"),"<h1>Offline</h1>");
    File.WriteAllText(Path.Combine(web,".env"),"secret");
    var off=new ProjectDefinition { Name="Offline",PackageName="com.brendigo.offline",SourceType="local",LocalPath=web };
    var folder=AndroidProjectGenerator.Generate(off,Path.Combine(root,"offline"));
    Test(File.Exists(Path.Combine(folder,"app","src","main","assets","index.html")),"Offline HTML copied");
    Test(!File.Exists(Path.Combine(folder,"app","src","main","assets",".env")),"Secrets excluded");
    store.Delete(saved.Id);Test(store.List().Count==0,"Delete");
} finally { try { Directory.Delete(root,true); } catch(IOException){} }
Console.WriteLine("Brendigo native core tests passed.");
