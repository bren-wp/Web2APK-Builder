"use strict";
const {contextBridge,ipcRenderer}=require("electron");
const call=(name,...args)=>ipcRenderer.invoke(name,...args);
contextBridge.exposeInMainWorld("brendigo",Object.freeze({
  listProjects:()=>call("projects:list"),
  saveProject:p=>call("projects:save",p),
  deleteProject:id=>call("projects:delete",id),
  pickLocal:()=>call("ui:pick-local"),
  pickKeystore:()=>call("ui:pick-keystore"),
  checkTools:()=>call("tools:check"),
  listDevices:()=>call("tools:devices"),
  exportProject:id=>call("project:export",id),
  buildProject:(id,format,signing)=>call("project:build",{id,format,signing}),
  openLink:url=>call("ui:open-link",url),
  openPath:id=>call("ui:open-path",id),
  onBuildLog:fn=>{const handler=(_e,text)=>fn(text);ipcRenderer.on("build:log",handler);return ()=>ipcRenderer.removeListener("build:log",handler);}
}));
