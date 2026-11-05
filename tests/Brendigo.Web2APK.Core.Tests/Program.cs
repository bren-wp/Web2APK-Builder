using Brendigo.Web2APK.Core;

static void Test(bool valid, string name) {
    if (!valid) throw new InvalidOperationException("FAILED: "+name);
    Console.WriteLine("PASS "+name);
}
static bool Throws(Action action) {
    try { action(); return false; } catch (ArgumentException) { return true; }
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
