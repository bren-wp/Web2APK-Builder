"use strict";
const test=require("node:test");
const assert=require("node:assert/strict");
const fs=require("node:fs");
const os=require("node:os");
const path=require("node:path");
const {normalize,ProjectStore,validURL}=require("../src/core/projects.cjs");
const {generateProject}=require("../src/core/generator.cjs");
test("secure URL policy and valid package name",()=>{
  assert.equal(validURL("https://example.com",false),true);
  assert.equal(validURL("http://example.com",false),false);
  assert.equal(validURL("javascript:alert(1)",true),false);
  assert.equal(validURL("https://user:password@example.com",true),false);
  assert.throws(()=>normalize({name:"App",packageName:"not a package"}));
  assert.throws(()=>normalize({name:"App",url:"http://example.com"}));
  assert.equal(normalize({name:"App",url:"https://example.com"}).targetSdk,36);
});
test("project CRUD persists only locally",()=>{
  const tmp=fs.mkdtempSync(path.join(os.tmpdir(),"web2apk-store-"));
  try{
    const db=new ProjectStore(tmp);
    const p=db.save({name:"Sample",packageName:"com.sample.demo",url:"https://example.com"});
    assert.equal(db.all().length,1);
    assert.equal(db.get(p.id).name,"Sample");
    assert.equal(db.remove(p.id),true);
    assert.equal(db.all().length,0);
  }finally{fs.rmSync(tmp,{recursive:true,force:true});}
});
test("generates Android Gradle project with escaped labels and safe WebView defaults",()=>{
  const tmp=fs.mkdtempSync(path.join(os.tmpdir(),"web2apk-generated-"));
  try{
    generateProject({name:"A & B <Studio>",packageName:"com.example.studio",url:"https://example.com"},tmp);
    const manifest=fs.readFileSync(path.join(tmp,"app/src/main/AndroidManifest.xml"),"utf8");
    const java=fs.readFileSync(path.join(tmp,"app/src/main/java/com/example/studio/MainActivity.java"),"utf8");
    assert.ok(manifest.includes('usesCleartextTraffic="false"'));
    assert.ok(java.includes("setAllowUniversalAccessFromFileURLs(false)"));
    assert.ok(fs.readFileSync(path.join(tmp,"app/src/main/res/values/strings.xml"),"utf8").includes("A &amp; B &lt;Studio&gt;"));
    assert.ok(fs.existsSync(path.join(tmp,"settings.gradle")));
  }finally{fs.rmSync(tmp,{recursive:true,force:true});}
});
test("local HTML folder is copied without symlinks or development artifacts",()=>{
  const source=fs.mkdtempSync(path.join(os.tmpdir(),"web2apk-site-"));
  const dest=fs.mkdtempSync(path.join(os.tmpdir(),"web2apk-site-output-"));
  try{
    fs.writeFileSync(path.join(source,"index.html"),"<h1>Hello</h1>");
    fs.writeFileSync(path.join(source,".env"),"SECRET");
    generateProject({name:"Offline",packageName:"com.example.offline",sourceType:"local",localPath:source},dest);
    assert.ok(fs.existsSync(path.join(dest,"app/src/main/assets/index.html")));
    assert.equal(fs.existsSync(path.join(dest,"app/src/main/assets/.env")),false);
  }finally{fs.rmSync(source,{recursive:true,force:true});fs.rmSync(dest,{recursive:true,force:true});}
});
