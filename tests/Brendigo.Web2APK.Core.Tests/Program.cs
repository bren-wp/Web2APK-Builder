using Brendigo.Web2APK.Core;

static void Test(bool valid, string name) {
    if (!valid) throw new InvalidOperationException("FAILED: "+name);
    Console.WriteLine("PASS "+name);
}
static bool ThrowsInvalidData(Action action) {
    try { action(); return false; } catch (InvalidDataException) { return true; }
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
if (args.Length == 2 && args[0]=="--firebase-fixture") {
    Directory.CreateDirectory(args[1]);
    var cfg=Path.GetFullPath(Path.Combine(args[1],"google-services-ci.json"));
    // Test-only identifiers: this file has no real Firebase credentials and must never be used in production.
    File.WriteAllText(cfg, """
{
  "project_info":{"project_number":"123456789012","project_id":"brendigo-ci-placeholder","storage_bucket":"brendigo-ci-placeholder.appspot.com"},
  "client":[{"client_info":{"mobilesdk_app_id":"1:123456789012:android:1a2b3c4d5e6f7890","android_client_info":{"package_name":"com.brendigo.firebasefixture"}},"oauth_client":[],"api_key":[{"current_key":"AIzaSyDUMMYTESTKEY012345678901234567890"}],"services":{"appinvite_service":{"other_platform_oauth_client":[]}}}],
  "configuration_version":"1"
}
""");
    var app=new ProjectDefinition { Name="FCM Build Smoke",PackageName="com.brendigo.firebasefixture",
        FirebasePushEnabled=true,FirebaseConfigPath=cfg,Url="https://example.com" };
    AndroidProjectGenerator.Generate(app,Path.Combine(args[1],"project"));
    File.Delete(cfg);
    Console.WriteLine("FCM test fixture generated");
    return;
}
if (args.Length == 2 && args[0]=="--refresh-fixture") {
    var fixture=new ProjectDefinition { Name="Refresh Build Smoke",
        PackageName="com.brendigo.refreshfixture",Url="https://example.com",
        PullRefresh=true };
    AndroidProjectGenerator.Generate(fixture,args[1]);
    Console.WriteLine("Refresh test fixture generated");
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
Test(!BrendigoLicense.TryVerify("invalid","windows",out _),"Invalid activation rejected");
Test(!BrendigoLicense.TryVerify("BL1.eyJ2IjoxLCJzIjoiOGFkOWMzN2IwMDgyMDdhZGIyMTM3OTQ4ZmY0MTk2NGQiLCJhIjoiYm90aCIsImlhdCI6MTc5Mzg3ODkwNSwiZXhwIjoxNzkzODkyNTA1fQ.UQUXxdvcS3iU_a62c5h7rU38IpNPf9A03ixM1JcGsf90VE8dfbEUohSRAVglMRnLUjVoi5u4YDZ7Q_ymCidoGtqN5Cz6TzbZFNK0XrHWSZcr4eXam0ignnxX8bDtoR6YhSjs9mnH0n39Vu1bFjmgH1NrprPFH5Nm8AqgvlSUGre8MB1FBWaRyAQY6K1hRF3UyL31dMqiyPYOaXdekUlVBiJa3JHUyjYah9Mn6SEMG6ujtQ_9oouhdCFCG6tfH0mSZWeyLl1BPVyj_jsYx0iF_IOl8bMy-hceDMR-BtOQWbTjuI4hVbl023bMcNVvx42XWXlACyyswWOfLVc5e63Uh0xIr9GQEk6_MJ1X9_Akj7yH790ShUtVrkFldBxU9w4PDeuUQtNb5ORQBR_uyZhm7k5WS6P9BvgklLn5r89lUmC7b7ytZ_8IUwFdSxXHPZ0qx1sbYGgSYqcdAIk22uoMf1nV2QOmPZxiWFjfEWmH38Pe4IPKAiwIlXVX2dEpQibw","windows",out _),"Authentic but expired license rejected");
using (var forged=System.Security.Cryptography.RSA.Create(3072))
{
    Test(!BrendigoLicense.OwnerMatches(forged),"Unknown issuer key rejected");
    bool refused=false;
    try { BrendigoLicense.Issue(forged,"0102030405060708090a0b0c0d0e0f10","both",
        DateTimeOffset.UtcNow.ToUnixTimeSeconds(),0,"developer","*"); }
    catch(System.Security.Cryptography.CryptographicException) {refused=true;}
    Test(refused,"Forged RSA issuer rejected before signing");
}
var now=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
var demo=new BrendigoLicense.Claims(new string('a',32),"both",now,0,"demo","com.example.one");
var basic=demo with { Tier="basic" };
var pro=demo with { Tier="pro" };
var owner=demo with { Tier="developer",PackageName="*" };
var triplePro=pro with { PackageName="com.demo.one,com.demo.two,com.demo.three" };
var agency=demo with {Tier="agency",PackageName="*",ExpiresAt=now+30L*86400};
Test(LicenseEntitlements.ValidPackage("pro",triplePro.PackageName),"Three package IDs accepted");
Test(LicenseEntitlements.ValidPackage("demo",LicenseEntitlements.AutomaticScope) &&
    LicenseEntitlements.ValidPackage("basic",LicenseEntitlements.AutomaticScope) &&
    LicenseEntitlements.ValidPackage("pro",LicenseEntitlements.AutomaticScope),
    "Automatic signed scope accepted for limited tiers");
Test(!LicenseEntitlements.ValidPackage("agency",LicenseEntitlements.AutomaticScope) &&
    !LicenseEntitlements.ValidPackage("developer",LicenseEntitlements.AutomaticScope),
    "Unlimited tiers require wildcard scope");
Test(!LicenseEntitlements.ValidPackage("pro","com.demo.one,com.demo.two,com.demo.three,com.demo.four"),
    "Four package IDs rejected");
Test(!LicenseEntitlements.ValidPackage("pro","com.demo.one,com.demo.one"),
    "Duplicated package ID rejected");
Test(!LicenseEntitlements.ValidPackage("agency","com.demo.one"),
    "Agency requires wildcard scope");
Test(LicenseEntitlements.PackageMatches(triplePro,"com.demo.two") &&
    !LicenseEntitlements.PackageMatches(triplePro,"com.demo.four"),
    "Professional signed package scope enforced");
Test(LicenseEntitlements.ApplicationLimit("pro")==3 &&
    LicenseEntitlements.ApplicationLimit("agency")==int.MaxValue &&
    LicenseEntitlements.ApplicationLimit("developer")==int.MaxValue,"Plan application limits");
Test(LicenseEntitlements.AgencyPriceEur(1)==39 && LicenseEntitlements.AgencyPriceEur(12)==390,
    "Agency proposed monthly and yearly rates");
Test(LicenseEntitlements.Check(agency,new ProjectDefinition{Name="Agency test",
    PackageName="com.agency.client"},"source",out _),"Agency unrestricted source access");
var agencyExpired=agency with {ExpiresAt=now-1};
Test(!LicenseEntitlements.Check(agencyExpired,new ProjectDefinition{Name="Expired agency",
    PackageName="com.agency.client"},"apk",out _),"Expired agency cannot export new apps");
Test(LicenseEntitlements.PriceEur("demo")==0 && LicenseEntitlements.PriceEur("basic")==49 &&
    LicenseEntitlements.PriceEur("pro")==69,"Tier price metadata");
Test(LicenseEntitlements.Allows(demo,"apk") && !LicenseEntitlements.Allows(demo,"aab") &&
    !LicenseEntitlements.Allows(demo,"source"),"Demo restricted to APK");
Test(LicenseEntitlements.Allows(basic,"aab") && LicenseEntitlements.Allows(basic,"custom_icon") &&
    !LicenseEntitlements.Allows(basic,"firebase_push"),"Basic APK/AAB/branding only");
Test(LicenseEntitlements.Allows(pro,"firebase_push") && LicenseEntitlements.Allows(pro,"pull_refresh") &&
    LicenseEntitlements.Allows(pro,"admob") && LicenseEntitlements.Allows(pro,"advanced"),"Professional push, ads and advanced permissions");
Test(LicenseEntitlements.Allows(owner,"admob") && LicenseEntitlements.Allows(owner,"aab") &&
    LicenseEntitlements.Allows(owner,"unlimited_projects"),"Developer unrestricted entitlements");
var other=new ProjectDefinition { Name="Other package",PackageName="com.other.product" };
Test(!LicenseEntitlements.Check(demo,other,"apk",out _),"Per-app license package binding");
Test(LicenseEntitlements.Check(triplePro,new ProjectDefinition{Name="Third app",
    PackageName="com.demo.three"},"aab",out _),"Pro third Android app allowed");
Test(!LicenseEntitlements.Check(triplePro,new ProjectDefinition{Name="Fourth app",
    PackageName="com.demo.four"},"apk",out _),"Pro fourth Android app blocked");
Test(!LicenseEntitlements.Allows(demo,"source") && LicenseEntitlements.Allows(basic,"source")
    && LicenseEntitlements.Allows(pro,"source") && LicenseEntitlements.Allows(agency,"source"),
    "Source browsing and download restricted to paid tiers");
Test(LicenseEntitlements.Check(owner,other,"aab",out _),"Owner Developer unlimited package IDs");
Test(new ProjectDefinition().BrendigoSplashBranding,"Default projects use Brendigo splash branding");
var tamperedDemo=new ProjectDefinition{Name="Demo change",PackageName="com.example.one",
    BrendigoSplashBranding=false};
Test(!LicenseEntitlements.Check(demo,tamperedDemo,"apk",out _),
    "Demo cannot remove mandatory Brendigo splash branding");
tamperedDemo.BrendigoSplashBranding=true;
Test(LicenseEntitlements.Check(demo,tamperedDemo,"apk",out _),
    "Demo with default Brendigo icon and splash allowed");
tamperedDemo.IconPath="not-a-real-file.png";
Test(!LicenseEntitlements.Check(demo,tamperedDemo,"apk",out _),
    "Demo cannot use custom launcher icon");
var adsBlocked=new ProjectDefinition { Name="Ads",PackageName="com.example.one",AdMobEnabled=true };
Test(LicenseEntitlements.Check(pro,adsBlocked,"apk",out _),"Professional can use AdMob");
var pushAllowed=new ProjectDefinition { Name="Push",PackageName="com.example.one",FirebasePushEnabled=true };
Test(LicenseEntitlements.Check(pro,pushAllowed,"aab",out _),"Professional push authorized");
Test(!BrendigoLicense.TryVerify("BL1.invalid.invalid","windows",out _),"Legacy unscoped license rejected");
var project = new ProjectDefinition { Name="Primjer", PackageName="com.brendigo.primjer", Url="https://example.com" };
project.Validate();
Test(project.MinSdk==24 && project.TargetSdk==36,"SDK defaults");
Test(!LicenseActivationStore.MaskToken("BL2.testPayload.testSignature").Equals("BL2.testPayload.testSignature"),
    "License key displayed as masked only");
Test(LicenseActivationStore.MaskToken("tiny")=="********","Short license never exposed");
Test(ToolchainService.LocalJdkHome.Contains("Brendigo") &&
    ToolchainService.LocalSdkRoot.Contains("android-sdk"),
    "Integrated JDK and Android SDK locations are private to user profile");
Test(typeof(BuildResult).GetProperty("Debug")!=null,"Build metadata distinguishes debug signing");
Test(LicenseEntitlements.Allows(demo,"apk") && !LicenseEntitlements.Allows(demo,"aab"),
    "Demo can use only APK variants");
Test(Throws(()=>new ProjectDefinition{Name="Test",Url="http://example.com"}.Validate()),"HTTP denied");
Test(Throws(()=>new ProjectDefinition{Name="Test",PackageName="../attack"}.Validate()),"Package validated");
Test(Throws(()=>new ProjectDefinition{Name="Test",Url="javascript:alert(1)"}.Validate()),"Non-https denied");
Test(Throws(()=>new ProjectDefinition{Name="Test",TargetSdk=35}.Validate()),"Old target denied");
var root=Path.Combine(Path.GetTempPath(),"brendigo-tests-"+Guid.NewGuid().ToString("N"));
try {
    Directory.CreateDirectory(root);
    if(OperatingSystem.IsWindows())
    {
        var auto=demo with {PackageName=LicenseEntitlements.AutomaticScope};
        var seats=Path.Combine(root,"seat-test");
        Test(AutomaticLicenseSeats.TryUse(auto,"com.demo.first",false,out var before,out _,seats) && before==0,
            "Preview does not consume a license seat");
        Test(AutomaticLicenseSeats.TryUse(auto,"com.demo.first",true,out var one,out _,seats) && one==1,
            "First package reserves one seat");
        Test(AutomaticLicenseSeats.TryUse(auto,"com.demo.first",true,out var again,out _,seats) && again==1,
            "Repeated build of same package is free");
        Test(!AutomaticLicenseSeats.TryUse(auto,"com.demo.second",true,out _,out _,seats),
            "Demo cannot use a second package");
        Test(AutomaticLicenseSeats.Usage(auto,out var used,out _,seats) && used==1,
            "Seat count persists");
        var proAuto=auto with {Tier="pro",Serial=new string('b',32)};
        Test(AutomaticLicenseSeats.TryUse(proAuto,"com.demo.first",true,out _,out _,seats) &&
            AutomaticLicenseSeats.TryUse(proAuto,"com.demo.second",true,out _,out _,seats) &&
            AutomaticLicenseSeats.TryUse(proAuto,"com.demo.third",true,out _,out _,seats) &&
            !AutomaticLicenseSeats.TryUse(proAuto,"com.demo.fourth",true,out _,out _,seats),
            "Professional max three unique packages");
        Test(AutomaticLicenseSeats.Usage(proAuto,out var three,out _,seats) && three==3,
            "Professional usage persisted");
        var encrypted=Path.Combine(seats,
            Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(proAuto.Serial)))+".dpapi");
        Test(File.Exists(encrypted),"Encrypted Professional ledger file exists");
        Test(!System.Text.Encoding.UTF8.GetString(File.ReadAllBytes(encrypted))
            .Contains("com.demo.first",StringComparison.Ordinal),
            "Windows seat ledger encrypted");
        File.WriteAllText(encrypted,"tampered");
        Test(!AutomaticLicenseSeats.Usage(proAuto,out _,out _,seats),
            "Corrupted seat ledger fails closed");
        var tamperedJson=System.Text.Encoding.UTF8.GetBytes(
            "{\"Serial\":\"wrong\",\"Packages\":[\"com.demo.first\"]}");
        var entropy=System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(proAuto.Serial));
        File.WriteAllBytes(encrypted,System.Security.Cryptography.ProtectedData.Protect(
            tamperedJson,entropy,System.Security.Cryptography.DataProtectionScope.CurrentUser));
        Test(!AutomaticLicenseSeats.Usage(proAuto,out _,out _,seats),
            "Valid DPAPI blob with altered internal serial fails closed");
    }
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
    Test(java.Contains("BRAND_SPLASH = true") && java.Contains("showBrendigoSplashBrand()"),
        "Generated Android splash shows default Brendigo branding");
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
    var check=GooglePlayReadiness.Review(project);
    Test(check.Any(f=>f.Code=="TARGET_API" && f.Severity=="PASS"),"Google Play target API 36");
    Test(check.Any(f=>f.Code=="PRIVACY" && f.Severity=="WARN"),"Missing privacy policy flagged");
    Test(check.Any(f=>f.Code=="SIGNATURE" && f.Severity=="WARN"),"Unsigned release flagged");
    var policyProject = new ProjectDefinition { Name="Privacy policy",PackageName="com.brendigo.privacy",
        Url="https://example.com",PrivacyPolicyUrl="https://example.com/privacy" };
    policyProject.Validate();
    var policyOutput=AndroidProjectGenerator.Generate(policyProject,Path.Combine(root,"privacy"));
    var privacyJava=File.ReadAllText(Path.Combine(policyOutput,"app","src","main","java","com","brendigo","privacy","MainActivity.java"));
    Test(privacyJava.Contains("Politika privatnosti") &&
        privacyJava.Contains("https://example.com/privacy"),"In-app Google Play privacy menu generated");
    Test(privacyJava.Contains("applySafeAreaInsets"),"Android 16 edge-to-edge safe insets");
    Test(File.Exists(Path.Combine(policyOutput,"GOOGLE_PLAY_CHECKLIST.txt")),"Play readiness checklist generated");
    Test(Throws(()=>new ProjectDefinition { Name="Bad policy",PrivacyPolicyUrl="http://bad.example.com/privacy" }.Validate()),
        "Only HTTPS privacy policy accepted");
    var refresh=new ProjectDefinition { Name="Refresh",
        PackageName="com.brendigo.refresh",PullRefresh=true };
    var refreshFolder=AndroidProjectGenerator.Generate(refresh,Path.Combine(root,"refresh"));
    Test(File.ReadAllText(Path.Combine(refreshFolder,"app","build.gradle"))
        .Contains("swiperefreshlayout:1.2.0"),"Refresh AndroidX dependency generated");
    Test(File.ReadAllText(Path.Combine(refreshFolder,"app","src","main","java",
        "com","brendigo","refresh","MainActivity.java")).Contains("setOnRefreshListener"),
        "Native Android pull-to-refresh handler generated");
    var web=Path.Combine(root,"web");Directory.CreateDirectory(web);
    File.WriteAllText(Path.Combine(web,"index.html"),"<h1>Offline</h1>");
    File.WriteAllText(Path.Combine(web,".env"),"secret");
    var off=new ProjectDefinition { Name="Offline",PackageName="com.brendigo.offline",SourceType="local",LocalPath=web };
    var folder=AndroidProjectGenerator.Generate(off,Path.Combine(root,"offline"));
    Test(File.Exists(Path.Combine(folder,"app","src","main","assets","index.html")),"Offline HTML copied");
    Test(!File.Exists(Path.Combine(folder,"app","src","main","assets",".env")),"Secrets excluded");
    store.Delete(saved.Id);Test(store.List().Count==0,"Delete");
    var parallelDir=Path.Combine(root,"parallel-store");
    var jobs=Enumerable.Range(0,24).Select(i=>Task.Run(()=>
        new ProjectStore(parallelDir).Save(new ProjectDefinition {
            Name="Projekt "+i,PackageName="com.brendigo.parallel"+i
        }))).ToArray();
    Task.WaitAll(jobs);
    var parallel=new ProjectStore(parallelDir);
    Test(parallel.List().Count==24 && parallel.List().Select(p=>p.Id).Distinct().Count()==24,
        "Concurrent writes from separate store instances preserve all projects");
    var dbFile=Path.Combine(parallelDir,"projects.json");
    var dbBackup=dbFile+".bak";
    Test(File.Exists(dbBackup) &&
        System.Text.Json.JsonDocument.Parse(File.ReadAllText(dbBackup))
            .RootElement.GetArrayLength()==23,
        "Atomic writes keep the previous complete JSON database backup");
    var original=File.ReadAllText(dbFile);
    File.WriteAllText(dbFile,original[..Math.Min(30,original.Length)]);
    Test(ThrowsInvalidData(()=>parallel.List()),
        "Truncated project database is rejected instead of silently resetting");
    File.WriteAllText(dbFile,original);
    var projectsJson=System.Text.Json.JsonSerializer.Serialize(parallel.List());
    var projectNode=System.Text.Json.JsonDocument.Parse(projectsJson).RootElement[0].GetRawText();
    File.WriteAllText(dbFile,"["+projectNode+","+projectNode+"]");
    Test(ThrowsInvalidData(()=>parallel.List()),
        "Duplicate project IDs are rejected");
    File.WriteAllText(dbFile,new string('x',8*1024*1024+1));
    Test(ThrowsInvalidData(()=>parallel.List()),
        "Oversized project databases are rejected before deserialization");
    File.WriteAllText(dbFile,original);
    Test(parallel.List().Count==24,"A valid database can be recovered without project loss");
} finally { try { Directory.Delete(root,true); } catch(IOException){} }
Console.WriteLine("Brendigo native core tests passed.");
