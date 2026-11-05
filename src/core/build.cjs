"use strict";
const fs=require("node:fs");
const path=require("node:path");
const {spawn}=require("node:child_process");
const {generateProject}=require("./generator.cjs");
function tool(command,args,options={}) {
  return new Promise(resolve=>{
    const ps=spawn(command,args,{...options,windowsHide:true,shell:false});
    let output="";
    ps.stdout.on("data",v=>output+=v.toString());
    ps.stderr.on("data",v=>output+=v.toString());
    ps.on("error",()=>resolve({ok:false,output:"Alat nije pronađen: "+command}));
    ps.on("close",code=>resolve({ok:code===0,output:output.slice(0,12000)}));
  });
}
async function checkTools() {
  const win=process.platform==="win32";
  const get=(cmd,args)=>win?tool("cmd.exe",["/d","/s","/c",cmd,...args]):tool(cmd,args);
  const [java,gradle,adb]=await Promise.all([get("java",["-version"]),get("gradle",["--version"]),get("adb",["version"])]);
  return {java,gradle,adb,sdk:process.env.ANDROID_HOME||process.env.ANDROID_SDK_ROOT||""};
}
async function devices() {
  const result=process.platform==="win32"?await tool("cmd.exe",["/d","/s","/c","adb","devices","-l"]):await tool("adb",["devices","-l"]);
  return result;
}
async function buildAndroid(p,dest,format,signing,onLog) {
  if(!["apk","aab"].includes(format)) throw new Error("Nepoznat format.");
  const projectDir=path.join(dest,"android-project");
  fs.rmSync(projectDir,{recursive:true,force:true});
  generateProject(p,projectDir);
  const task=format==="apk"?":app:assembleRelease":":app:bundleRelease";
  const win=process.platform==="win32";
  const args=win?["/d","/s","/c","gradle","--no-daemon",task]:["--no-daemon",task];
  const env={...process.env};
  if(signing && signing.keystore && signing.alias && signing.storePassword && signing.keyPassword) {
    env.BRENDIGO_KEYSTORE=signing.keystore;
    env.BRENDIGO_KEY_ALIAS=signing.alias;
    env.BRENDIGO_STORE_PASSWORD=signing.storePassword;
    env.BRENDIGO_KEY_PASSWORD=signing.keyPassword;
  }
  const command=win?"cmd.exe":"gradle";
  await new Promise((resolve,reject)=>{
    let ended=false;
    const ps=spawn(command,args,{cwd:projectDir,env,shell:false,windowsHide:true});
    const log=data=>onLog(String(data).slice(0,16000));
    ps.stdout.on("data",log); ps.stderr.on("data",log);
    ps.on("error",err=>{if(!ended){ended=true;reject(new Error("Gradle se ne može pokrenuti. Provjeri JDK, Android SDK i Gradle 9.6+. "+err.message));}});
    ps.on("close",code=>{if(!ended){ended=true;code===0?resolve():reject(new Error("Gradle nije dovršio izradu (exit "+code+")."));}});
  });
  const folder=path.join(projectDir,"app","build","outputs",format==="apk"?"apk":"bundle","release");
  const matching=(fs.existsSync(folder)?fs.readdirSync(folder):[]).filter(f=>f.endsWith("."+format));
  if(!matching.length) throw new Error("Gradle nije proizveo "+format.toUpperCase()+" datoteku.");
  const signed=!!env.BRENDIGO_KEYSTORE;
  const name=p.packageName+"-"+p.versionName+(signed?"":"-unsigned")+"."+format;
  const file=path.join(dest,name);
  fs.copyFileSync(path.join(folder,matching[0]),file);
  return {output:file,signed,source:projectDir};
}
module.exports={checkTools,devices,buildAndroid};
