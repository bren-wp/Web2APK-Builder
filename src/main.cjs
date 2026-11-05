"use strict";
const {app,BrowserWindow,ipcMain,dialog,shell}=require("electron");
const path=require("node:path");
const fs=require("node:fs");
const {ProjectStore,validURL}=require("./core/projects.cjs");
const {generateProject}=require("./core/generator.cjs");
const {checkTools,devices,buildAndroid}=require("./core/build.cjs");
let win,store,busy=false;
app.setAppUserModelId("com.brendigo.web2apk.builder");
function createWindow(){
  win=new BrowserWindow({
    width:1440,height:900,minWidth:1000,minHeight:700,backgroundColor:"#090F21",
    title:"Brendigo Web2APK Builder",show:false,
    titleBarStyle:"hidden",titleBarOverlay:{color:"#090F21",symbolColor:"#D2DCFA",height:38},
    webPreferences:{preload:path.join(__dirname,"preload.cjs"),nodeIntegration:false,contextIsolation:true,sandbox:true,webSecurity:true}
  });
  win.webContents.setWindowOpenHandler(()=>({action:"deny"}));
  win.webContents.on("will-navigate",e=>e.preventDefault());
  win.loadFile(path.join(__dirname,"ui","index.html"));
  win.once("ready-to-show",()=>win.show());
}
app.whenReady().then(()=>{
  store=new ProjectStore(app.getPath("userData"));
  ipcMain.handle("projects:list",()=>store.all());
  ipcMain.handle("projects:save",(_e,p)=>store.save(p));
  ipcMain.handle("projects:delete",(_e,id)=>store.remove(id));
  ipcMain.handle("ui:pick-local",async()=>{
    const r=await dialog.showOpenDialog(win,{properties:["openDirectory"]});
    return r.canceled?"":r.filePaths[0];
  });
  ipcMain.handle("ui:pick-keystore",async()=>{
    const r=await dialog.showOpenDialog(win,{properties:["openFile"],filters:[{name:"Keystore",extensions:["jks","keystore","p12"]}]});
    return r.canceled?"":r.filePaths[0];
  });
  ipcMain.handle("tools:check",()=>checkTools());
  ipcMain.handle("tools:devices",()=>devices());
  ipcMain.handle("project:export",async(_e,id)=>{
    const project=store.get(id);if(!project)throw new Error("Projekt nije pronađen.");
    const selection=await dialog.showOpenDialog(win,{title:"Odaberi mapu za Android projekt",properties:["openDirectory","createDirectory"]});
    if(selection.canceled)return null;
    const dest=path.join(selection.filePaths[0],project.packageName.replace(/\./g,"-")+"-android");
    if(fs.existsSync(dest))throw new Error("Odredišna mapa već postoji. Odaberi drugu mapu.");
    generateProject(project,dest);
    return dest;
  });
  ipcMain.handle("project:build",async(e,{id,format,signing})=>{
    if(busy)throw new Error("Izrada je već u tijeku.");
    const p=store.get(id); if(!p)throw new Error("Projekt nije pronađen.");
    const target=await dialog.showOpenDialog(win,{title:"Odaberi mapu za APK/AAB",properties:["openDirectory","createDirectory"]});
    if(target.canceled)return null;
    busy=true;
    try{
      const root=path.join(app.getPath("userData"),"builds",p.id);
      fs.mkdirSync(root,{recursive:true});
      e.sender.send("build:log","Pokrenuta lokalna izrada: "+format+"\n");
      const result=await buildAndroid(p,root,format,signing,t=>{if(!e.sender.isDestroyed())e.sender.send("build:log",t);});
      const final=path.join(target.filePaths[0],path.basename(result.output));
      fs.copyFileSync(result.output,final);
      return {...result,output:final};
    }finally{busy=false;}
  });
  ipcMain.handle("ui:open-link",async(_e,url)=>{
    if(url==="https://brendigo.com")return shell.openExternal(url);
    if(!store.all().some(p=>p.sourceType==="url"&&p.url===url&&validURL(url,p.allowHttp)))throw new Error("URL nije dopušten.");
    return shell.openExternal(url);
  });
  ipcMain.handle("ui:open-path",async(_e,id)=>{
    const p=store.get(id);if(!p)throw new Error("Projekt nije pronađen.");
    const folder=path.join(app.getPath("userData"),"builds",p.id);
    return fs.existsSync(folder)?shell.openPath(folder):"Još nema izrađenih datoteka.";
  });
  createWindow();
});
app.on("window-all-closed",()=>app.quit());
