"use strict";
(function(){
  const api=window.brendigo||null;
  const asset="../../assets/brand-icon.svg";
  const menu=[["dashboard","Početna","home"],["new","Novi projekt","plus"],["projects","Moji projekti","folder"],["tools","Emulator","phone"],["devices","Uređaji","monitor"],["settings","Postavke","settings"],["about","O programu","info"]];
  const steps=[["new","Osnovno"],["appearance","Izgled"],["webview","WebView"],["features","Funkcije"],["permissions","Dozvole"],["build","Izrada"]];
  const initial={name:"Moja web aplikacija",packageName:"com.brendigo.mojaaplikacija",sourceType:"url",url:"https://example.com",localPath:"",versionName:"1.0.0",versionCode:1,minSdk:24,targetSdk:36,orientation:"portrait",theme:"dark",accent:"#7253FF",javascript:true,domStorage:true,pullRefresh:false,allowHttp:false,openExternal:true,camera:false,statusBar:true,splash:true,fullscreen:false};
  let page="dashboard",projects=[],draft={...initial},savedId=null,tools=null,deviceInfo=null,signing={keystore:"",alias:"",storePassword:"",keyPassword:""},buildLog="",buildState="idle",buildFile="";
  const ico={
    home:"<path d='m3 10 9-7 9 7v10H3z'/><path d='M9 20v-7h6v7'/>",
    plus:"<path d='M12 5v14M5 12h14'/>",
    folder:"<path d='M3 7V5a2 2 0 0 1 2-2h5l2 3h7a2 2 0 0 1 2 2v11H3z'/>",
    phone:"<rect x='6' y='2' width='12' height='20' rx='2'/><path d='M11 18h2'/>",
    monitor:"<rect x='2' y='3' width='20' height='15' rx='2'/><path d='M8 22h8M12 18v4'/>",
    settings:"<circle cx='12' cy='12' r='3'/><path d='M19 13a7 7 0 0 1-.5 2l1.5 2-3 3-2-1.5a7 7 0 0 1-2 .5l-1 2h-2l-1-2a7 7 0 0 1-2-.5L5 20l-3-3 1.5-2a7 7 0 0 1-.5-2l-2-1v-2l2-1a7 7 0 0 1 .5-2L2 5l3-3 2 1.5a7 7 0 0 1 2-.5l1-2h2l1 2a7 7 0 0 1 2 .5L17 2l3 3-1.5 2a7 7 0 0 1 .5 2l2 1v2z'/>",
    info:"<circle cx='12' cy='12' r='9'/><path d='M12 11v6M12 7v1'/>",
    cube:"<path d='m12 2 9 5v10l-9 5-9-5V7zM3 7l9 5 9-5M12 12v10'/>",
    shield:"<path d='M12 22c5-2 8-6 8-12V5l-8-3-8 3v5c0 6 3 10 8 12z'/><path d='m8 12 3 3 5-6'/>",
    clock:"<circle cx='12' cy='12' r='9'/><path d='M12 7v5l4 3'/>",
    globe:"<circle cx='12' cy='12' r='9'/><path d='M3 12h18M12 3c-5 5-5 13 0 18M12 3c5 5 5 13 0 18'/>",
    arrow:"<path d='M4 12h16m-7-7 7 7-7 7'/>",
    check:"<path d='m4 12 5 5 11-11'/>",
    copy:"<rect x='8' y='8' width='12' height='12' rx='2'/><path d='M16 8V4H4v12h4'/>",
    code:"<path d='m8 8-4 4 4 4m8-8 4 4-4 4m-3-11-2 14'/>",
    palette:"<path d='M12 3C6 3 3 7 3 12s4 9 9 9h2a2 2 0 0 0 2-2 2 2 0 0 0-2-2c-1 0-1-2 0-2h3a5 5 0 0 0 5-5c0-4-5-7-10-7z'/>",
    refresh:"<path d='M21 11a9 9 0 1 0-3 7m3-7V4m0 7h-7'/>",
    bell:"<path d='M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4'/>"
  };
  function icon(name){return '<svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">'+(ico[name]||ico.info)+'</svg>';}
  function esc(s){return String(s==null?"":s).replace(/[&<>"']/g,c=>({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"}[c]));}
  function notify(s){const el=document.querySelector("#notice");el.textContent=s;el.classList.add("show");clearTimeout(notify.t);notify.t=setTimeout(()=>el.classList.remove("show"),5500);}
  function button(label,action,type=""){return '<button class="btn '+type+'" data-action="'+action+'">'+label+'</button>';}
  function field(key,label,type="text",extra=""){
    const val=key in signing?signing[key]:draft[key];
    if(type==="select"){
      let options=[];
      if(key==="minSdk")options=[24,26,28,30,33,34,35,36].map(x=>[x,String(x)]);
      if(key==="targetSdk")options=[[36,"36 (preporučeno)"],[37,"37"]];
      if(key==="orientation")options=[["portrait","Portret"],["landscape","Pejzaž"],["unspecified","Automatski"]];
      if(key==="theme")options=[["dark","Tamna"],["light","Svijetla"]];
      return '<div class="field '+extra+'"><label for="'+key+'">'+label+'</label><select class="control" id="'+key+'" data-field="'+key+'">'+options.map(x=>'<option value="'+esc(x[0])+'" '+(String(val)===String(x[0])?"selected":"")+'>'+esc(x[1])+'</option>').join("")+'</select></div>';
    }
    const ftype=type==="password"?"password":type==="number"?"number":"text";
    return '<div class="field '+extra+'"><label for="'+key+'">'+label+'</label><input class="control" id="'+key+'" data-field="'+key+'" type="'+ftype+'" value="'+esc(val)+'"></div>';
  }
  function toggle(key,label,desc){
    const checked=draft[key]===true;
    return '<div class="toggle-row"><div><b>'+label+'</b><small>'+desc+'</small></div><button type="button" class="switch '+(checked?"on":"")+'" data-toggle="'+key+'" aria-pressed="'+checked+'" aria-label="'+esc(label)+'"></button></div>';
  }
  function chrome(){
    const selected=steps.some(x=>x[0]===page)?"new":page;
    const nav1=menu.slice(0,6).map(m=>'<button class="side-item '+(selected===m[0]?"active":"")+'" data-nav="'+m[0]+'">'+icon(m[2])+m[1]+'</button>').join("");
    const nav2='<button class="side-item '+(page==="docs"?"active":"")+'" data-nav="docs">'+icon("folder")+'Dokumentacija</button>'+
      '<button class="side-item '+(page==="about"?"active":"")+'" data-nav="about">'+icon("info")+'O programu</button>';
    return '<header class="titlebar"><img src="'+asset+'" alt="">Brendigo Web2APK Builder <span class="pro">PRO</span></header>'+
      '<div class="layout"><aside class="sidebar"><div class="brand"><img src="'+asset+'" alt="Brendigo"><div><b>Brendigo<br><span>Web2APK</span></b><small>Builder</small></div></div>'+
      '<div class="nav-caption">RADNI PROSTOR</div><nav class="nav">'+nav1+'</nav><div class="nav-caption" style="margin-top:27px">POMOĆ</div><nav class="nav">'+nav2+'</nav>'+
      '<div class="sidebar-foot"><span class="status-pill">'+icon("check")+' Lokalno · privatno</span><br><b>Brendigo Web2APK Builder</b><small>Windows Studio · v0.1.0</small><small>brendigo.com</small></div></aside>'+
      '<main class="main"><div class="content"><div class="topline"><div class="crumb">Radni prostor '+icon("arrow")+' <span>'+esc(pageTitle())+'</span></div>'+
      '<div class="topright"><span class="sdk-pill">'+icon("shield")+' SDK 36+</span><button class="icon-button" data-nav="about" aria-label="O programu">'+icon("bell")+'</button><span class="avatar">BR</span></div></div>'+
      renderPage()+'</div></main></div>';
  }
  function pageTitle(){const fromStep=steps.find(x=>x[0]===page);if(fromStep)return "Novi projekt · "+fromStep[1];const m=menu.find(x=>x[0]===page);return m?m[1]:page==="docs"?"Dokumentacija":"O programu";}
  function header(title,sub,right){return '<div class="page-head"><div><h1>'+title+'</h1><p>'+sub+'</p></div>'+(right||"")+'</div>';}
  function dashboard(){
    const p=projects.slice(0,5);const stat=[["folder",projects.length,"Ukupno projekata"],["cube",projects.length?projects.filter(x=>x.buildDate).length:0,"Izvezenih paketa"],["shield",projects.filter(x=>x.allowHttp===false).length,"HTTPS konfiguracija"],["clock",projects.filter(x=>x.updatedAt).length,"Spremljeni projekti"]];
    return '<section class="hero"><div class="hero-text"><span class="tag">✧ STUDIO · NOVI PROJEKT</span><h1>Od web-stranice do <span>Android aplikacije.</span></h1>'+
      '<p>Povežite URL ili HTML projekt, oblikujte izgled i pripremite Android paket u jasnom procesu.</p><div class="btn-row">'+button(icon("plus")+" Novi projekt","create","primary")+button(icon("folder")+" Moji projekti","projects")+'</div></div>'+
      '<div class="hero-phone"><i class="hero-notch"></i><img src="'+asset+'" alt=""><div class="hero-lines"><i></i><i></i></div></div></section>'+
      '<div class="stats">'+stat.map(s=>'<div class="stat"><div class="stat-icon">'+icon(s[0])+'</div><strong>'+s[1]+'</strong><small>'+s[2]+'</small></div>').join("")+'</div>'+
      '<div class="section-head"><h2>Posljednji projekti</h2>'+button(icon("arrow")+" Svi projekti","projects","small")+'</div><div class="panel table-panel">'+table(p)+'</div>';
  }
  function table(rows){
    if(!rows.length)return '<div class="empty">'+icon("folder")+'<h3>Još nema projekata</h3><p>Stvori svoj prvi projekt i pretvori web u Android aplikaciju.</p>'+button(icon("plus")+" Novi projekt","create","primary")+'</div>';
    return '<table class="projects-table"><thead><tr><th>Projekt</th><th>Izvor</th><th>Vrsta</th><th>Datum</th><th>Status</th><th></th></tr></thead><tbody>'+
      rows.map(p=>'<tr><td><span class="project-icon">'+icon("globe")+'</span>'+esc(p.name)+'</td><td>'+esc(p.sourceType==="local"?"Lokalni HTML":host(p.url))+'</td><td>WebView</td><td>'+esc((p.updatedAt||"").slice(0,10))+'</td><td><span class="pill green">Spremljeno</span></td><td><button class="btn small" data-open="'+esc(p.id)+'">Uredi</button></td></tr>').join("")+'</tbody></table>';
  }
  function host(u){try{return new URL(u).hostname;}catch{return u;}}
  function stepsHeader(){
    return '<div class="wizard-steps">'+steps.map((s,i)=>(i?'<i class="wizard-line"></i>':'')+
      '<button class="wizard-step '+(page===s[0]?"current":"")+'" data-nav="'+s[0]+'"><span>'+(i+1)+'</span>'+s[1]+'</button>').join("")+'</div>';
  }
  function summary(){
    return '<aside class="panel"><h2>Pregled projekta</h2><p class="lead">Pregled osnovnih postavki prije nastavka.</p>'+
      '<div class="summary-project"><img src="'+asset+'" alt=""><div><b>'+esc(draft.name)+'</b><span class="muted">'+esc(draft.packageName)+'</span></div></div>'+
      '<div class="summary-row"><div><strong>Izvor</strong><small>'+esc(draft.sourceType==="local"?"Lokalni HTML":"URL web-stranice")+'</small></div><span class="pill green">'+(draft.sourceType==="local"?"HTML":draft.allowHttp?"HTTP":"HTTPS")+'</span></div>'+
      '<div class="summary-row"><div><strong>Target SDK</strong><small>Postavka Android projekta</small></div><span class="pill">'+esc(draft.targetSdk)+'</span></div>'+
      '<div class="summary-row"><div><strong>Izvoz</strong><small>APK i AAB po izboru</small></div>'+icon("cube")+'</div>'+
      '<div class="summary-foot">'+button(icon("arrow")+" Spremi i nastavi","next","primary")+'</div></aside>';
  }
  function wizardPanel(body,footer){
    const index=steps.findIndex(x=>x[0]===page);
    return header("Novi Android projekt","Korak "+(index+1)+" od 6 · "+steps[index][1],'<span class="sdk-pill">'+icon("check")+' Lokalna pohrana</span>')+
      stepsHeader()+'<div class="wizard-layout"><section class="panel">'+body+(footer||"")+'</section>'+summary()+'</div>';
  }
  function basics(){
    const source= '<div class="choice-row"><button class="choice '+(draft.sourceType==="url"?"active":"")+'" data-source="url">'+icon("globe")+' Web-stranica · URL</button><button class="choice '+(draft.sourceType==="local"?"active":"")+'" data-source="local">'+icon("code")+' Lokalni HTML projekt</button></div>';
    const urlField=draft.sourceType==="url"?field("url","URL web-stranice *","text","wide")+'<div class="field wide hint">HTTPS je preporučen prije objave.</div>':'<div class="field wide"><label>Lokalna HTML mapa *</label><div class="btn-row"><input class="control" style="flex:1" value="'+esc(draft.localPath)+'" readonly aria-label="Mapa projekta">'+button("Odaberi mapu","local","small")+'</div><small>Mapa mora sadržavati index.html. Datoteke se kopiraju lokalno.</small></div>';
    return wizardPanel('<h2>Osnovne informacije</h2><p class="lead">Odredite izvor sadržaja i identitet Android aplikacije.</p>'+
      '<div class="fields">'+field("name","Naziv aplikacije *")+field("packageName","Paket (applicationId) *")+'</div>'+source+'<div class="fields" style="margin-top:14px">'+urlField+
      field("versionName","Verzija aplikacije")+field("versionCode","Version code","number")+field("minSdk","Minimalni Android SDK","select","wide")+'</div>');
  }
  function appearance(){
    return wizardPanel('<h2>Izgled i brending</h2><p class="lead">Odaberite izgled, orijentaciju i boju Android aplikacije.</p>'+
      '<div class="fields">'+field("theme","Tema aplikacije","select")+field("orientation","Orijentacija","select")+'</div>'+
      '<div class="field" style="margin-top:22px"><label>Primarna boja</label><div class="colors">'+["#7253FF","#3A86FF","#31D4FF","#29D6A9","#FF8B5D"].map(c=>'<button class="swatch '+(draft.accent===c?"chosen":"")+'" title="'+c+'" data-color="'+c+'" style="background:'+c+'"></button>').join("")+'</div></div>'+
      toggle("splash","Splash zaslon","Uključi početni ekran u konfiguraciju projekta.")+
      toggle("statusBar","Statusna traka","Zadrži Android statusnu traku.")+
      '<div class="note">Brendigo ikona predstavlja privremeni izgled generiranog projekta. Prilagodljive ikone i animirani splash dolaze u sljedećem razvoju.</div>');
  }
  function webview(){
    return wizardPanel('<h2>WebView postavke</h2><p class="lead">Sigurnosne i funkcionalne postavke Android preglednika.</p>'+
      toggle("javascript","JavaScript","Omogućuje interaktivne web-stranice.")+
      toggle("domStorage","DOM Storage","Lokalna pohrana unutar WebViewa.")+
      toggle("openExternal","Vanjske poveznice","Vanjske poveznice za lokalni projekt otvaraju se u pregledniku.")+
      toggle("allowHttp","Dopusti HTTP","Nesigurna HTTP komunikacija — koristite samo kada je nužno.")+
      '<div class="note">Pristup lokalnim datotekama iz WebViewa i univerzalni file URL pristup su isključeni. Za lokalni HTML koristi se WebViewAssetLoader.</div>');
  }
  function features(){
    return wizardPanel('<h2>Funkcionalnosti aplikacije</h2><p class="lead">Osnovne mogućnosti koje se ugrađuju u generirani Android projekt.</p>'+
      toggle("fullscreen","Puni zaslon","Sakrij sistemsku statusnu traku.")+
      '<div class="feature-list" style="margin-top:20px"><div class="feature">'+icon("cube")+'<h3>APK / AAB izvoz</h3><p>Lokalna Gradle izrada uz instaliran Android SDK i JDK.</p></div>'+
      '<div class="feature">'+icon("shield")+'<h3>Potpisivanje paketa</h3><p>Opcionalni lokalni keystore; lozinke se ne spremaju.</p></div>'+
      '<div class="feature">'+icon("globe")+'<h3>Offline HTML</h3><p>Ugrađivanje lokalne web-mape u projekt.</p></div>'+
      '<div class="feature">'+icon("info")+'<h3>AdMob / Firebase</h3><p>Nisu implementirani u ovom početnom izdanju.</p></div></div>');
  }
  function permissions(){
    return wizardPanel('<h2>Dozvole i privatnost</h2><p class="lead">Minimalne dozvole i jasne sigurnosne postavke.</p>'+
      '<div class="notice-ok">'+icon("shield")+' Internet dopuštenje automatski se dodaje za mrežne projekte.</div>'+
      toggle("camera","Kamera","Dodaje CAMERA dozvolu u Android manifest; zahtijeva posebnu implementaciju WebChromeClient prije upotrebe.")+
      '<div class="note">Pristup mikrofonu, lokaciji, datotekama i obavijestima nije implementiran. Ne označavaj te funkcije aktivnima u aplikaciji objavljenoj korisnicima.</div>'+
      '<div class="field" style="margin-top:22px">'+field("targetSdk","Target Android SDK","select")+'</div>');
  }
  function buildPage(){
    return wizardPanel('<h2>Izrada Android aplikacije</h2><p class="lead">Spremite projekt, izvezite izvorni kod ili lokalno pokrenite Gradle izradu.</p>'+
      '<div class="notice-ok">'+icon("shield")+' Za APK/AAB potrebno je imati JDK 17, Gradle 9.6+ i Android SDK 36.</div>'+
      '<div class="btn-row">'+button(icon("folder")+" Izvezi Android projekt","export")+
      button(icon("cube")+" Izradi APK","apk","primary")+button(icon("cube")+" Izradi AAB","aab","primary")+'</div>'+
      '<div style="margin-top:24px"><h2>Opcionalno potpisivanje</h2><p class="lead">Lozinke ostaju samo u memoriji do zatvaranja aplikacije.</p>'+
      '<div class="fields"><div class="field wide"><label>Keystore datoteka</label><div class="btn-row"><input class="control" style="flex:1" value="'+esc(signing.keystore)+'" readonly>'+button("Odaberi","keystore","small")+'</div></div>'+
      field("alias","Alias certifikata")+field("storePassword","Lozinka keystorea","password")+field("keyPassword","Lozinka ključa","password")+'</div></div>'+
      '<div class="section-head"><h2>Gradle zapisnik</h2><span class="pill '+(buildState==="done"?"green":"")+'">'+(buildState==="running"?"Izrada u tijeku":buildState==="done"?"Izrađeno":buildState==="error"?"Pogreška":"Spremno")+'</span></div>'+
      '<div class="log" id="build-log">'+esc(buildLog||"Odaberi APK, AAB ili izvezi projekt. Gradle izlaz prikazat će se ovdje.")+'</div>'+
      (buildFile?'<div class="notice-ok">Datoteka: '+esc(buildFile)+'</div>':""));
  }
  function projectsPage(){return header("Moji projekti","Svi lokalno spremljeni Android projekti.",button(icon("plus")+" Novi projekt","create","primary"))+
    '<div class="panel table-panel">'+table(projects)+'</div>'+(projects.length?'<div class="section-head">'+button("Obriši odabrani projekt","delete","danger")+'</div>':"");}
  function devicesPage(){
    return header("Uređaji","Lokalna dijagnostika Android uređaja i emulatora.",button(icon("refresh")+" Osvježi","devices","small"))+
      '<div class="panel"><h2>ADB uređaji</h2><p class="lead">Potrebni su instalirani Android Platform Tools i USB debugging na uređaju.</p>'+
      '<div class="log">'+esc(deviceInfo?deviceInfo.output:"Klikni Osvježi za otkrivanje povezanih uređaja.")+'</div></div>';
  }
  function toolCard(name,res){
    return '<div class="tool-card"><strong>'+name+'</strong><small>'+(res?(res.ok?"Pronađeno":"Nije pronađeno"):"Nije provjereno")+'</small><pre>'+esc(res?res.output.slice(0,300):"")+'</pre></div>';
  }
  function toolsPage(){
    return header("Razvojni alati","Provjeri lokalni Android razvojni sustav.",button(icon("refresh")+" Provjeri alate","check-tools","primary"))+
      '<div class="tool-list">'+toolCard("Java / JDK 17+",tools&&tools.java)+toolCard("Gradle 9.6+",tools&&tools.gradle)+toolCard("ADB",tools&&tools.adb)+'</div>'+
      '<div class="panel" style="margin-top:18px"><h2>Android SDK</h2><p class="info">Putanja: '+esc(tools&&tools.sdk?tools.sdk:"ANDROID_HOME ili ANDROID_SDK_ROOT još nije postavljen.")+'</p>'+
      '<div class="note">Potreban je Android SDK (platforma 36), Gradle 9.6+ i Java 17. Studio ne instalira te alate automatski.</div></div>';
  }
  function settingsPage(){
    return header("Postavke","Lokalno okruženje i način rada aplikacije.")+
      '<div class="settings-grid"><section class="panel"><h2>Studio</h2><p class="lead">Pravila sigurnog rada.</p>'+
      '<div class="toggle-row"><div><b>Lokalna pohrana</b><small>Projekti ostaju na računalu.</small></div><span class="pill green">Aktivno</span></div>'+
      '<div class="toggle-row"><div><b>Izolirani renderer</b><small>Node pristup isključen i kontekst izoliran.</small></div><span class="pill green">Aktivno</span></div>'+
      '<div class="toggle-row"><div><b>Automatska telemetrija</b><small>Ne šalju se analitički događaji.</small></div><span class="pill green">Isključeno</span></div></section>'+
      '<section class="panel"><h2>Alati za izradu</h2><p class="lead">Ovisnosti se otkrivaju iz operacijskog sustava.</p>'+button("Provjeri Java / Gradle / SDK","check-tools","primary")+
      '<div class="note">Aplikacija ne preuzima, ne sprema ni ne šalje privatne certifikate na vanjske servise.</div></section></div>';
  }
  function aboutPage(){
    return header("O programu","Brendigo Web2APK Builder · Windows Studio")+
      '<div class="panel" style="max-width:780px;min-height:390px"><img src="'+asset+'" alt="Brendigo" style="width:82px;height:82px"><h1 style="font-size:29px">Brendigo Web2APK Builder</h1>'+
      '<p class="info">Verzija 0.1.0 · Windows desktop aplikacija<br>Autor: <strong>Brendigo</strong> · <strong>brendigo.com</strong></p>'+
      '<p class="info">Profesionalno lokalno sučelje za stvaranje Android WebView projekata i APK/AAB paketa. Ova početna verzija omogućuje rad s URL-ovima, lokalnim HTML projektima, postavkama Android aplikacije, izvozom i Gradle izradom.</p>'+
      button(icon("globe")+" brendigo.com","website","primary")+'</div>';
  }
  function docsPage(){
    return header("Dokumentacija","Kratke upute za izradu prve Android aplikacije.")+
      '<section class="panel" style="max-width:980px"><h2>Kako izraditi Android aplikaciju?</h2>'+
      '<p class="info"><strong>1.</strong> Odaberi Novi projekt, upiši naziv, applicationId i HTTPS URL ili lokalnu HTML mapu.<br>'+
      '<strong>2.</strong> Odaberi izgled, WebView opcije, minimalne dozvole i target SDK 36.<br>'+
      '<strong>3.</strong> Izvezi Android Gradle projekt ili pokreni izradu APK/AAB paketa.<br>'+
      '<strong>4.</strong> Za stvarnu izradu potrebni su JDK 17, Gradle 9.6+ i Android SDK 36.<br>'+
      '<strong>5.</strong> Za potpisane pakete odaberi vlastiti keystore i unesi lozinke samo u tijeku izrade.</p>'+
      '<div class="warning">Nepotpisani release APK nije spreman za distribuciju na Google Playu. Projekt prije objave treba testirati i ispuniti Google Play pravila.</div>'+
      '</section>';
  }
  function renderPage(){
    if(page==="dashboard")return dashboard();
    if(page==="new")return basics();
    if(page==="appearance")return appearance();
    if(page==="webview")return webview();
    if(page==="features")return features();
    if(page==="permissions")return permissions();
    if(page==="build")return buildPage();
    if(page==="projects")return projectsPage();
    if(page==="devices")return devicesPage();
    if(page==="tools")return toolsPage();
    if(page==="settings")return settingsPage();
    if(page==="docs")return docsPage();
    return aboutPage();
  }
  function render(){document.querySelector("#app").innerHTML=chrome();}
  async function reload(){if(api)projects=await api.listProjects();render();}
  async function save(){
    if(!api){notify("Za spremanje projekta pokreni Windows aplikaciju.");return null;}
    const p=await api.saveProject(draft);
    savedId=p.id;draft={...p};await reload();
    return p;
  }
  function startNew(){draft={...initial};savedId=null;page="new";buildLog="";buildState="idle";render();}
  async function action(name){
    if(name==="create"){startNew();return;}
    if(name==="projects"){page="projects";render();return;}
    if(name==="next"){
      const p=await save();if(!p)return;
      const i=steps.findIndex(x=>x[0]===page);
      if(i<steps.length-1)page=steps[i+1][0];
      render();return;
    }
    if(name==="prev"){const i=steps.findIndex(x=>x[0]===page);page=steps[Math.max(0,i-1)][0];render();return;}
    if(name==="local"){if(!api)return notify("Pokreni Windows aplikaciju.");const folder=await api.pickLocal();if(folder){draft.localPath=folder;render();}return;}
    if(name==="keystore"){if(!api)return notify("Pokreni Windows aplikaciju.");const f=await api.pickKeystore();if(f){signing.keystore=f;render();}return;}
    if(name==="website"){if(api)await api.openLink("https://brendigo.com");return;}
    if(name==="check-tools"){if(!api)return notify("Provjera je dostupna u Windows aplikaciji.");tools=await api.checkTools();page="tools";render();return;}
    if(name==="devices"){if(!api)return notify("ADB je dostupan u Windows aplikaciji.");deviceInfo=await api.listDevices();page="devices";render();return;}
    if(name==="export"){
      const p=await save();if(!p)return;
      const dest=await api.exportProject(p.id);
      if(dest){buildLog+="\nAndroid projekt izvezen: "+dest+"\n";render();notify("Android projekt uspješno izvezen.");}
      return;
    }
    if(name==="apk"||name==="aab"){
      const p=await save();if(!p)return;
      if(buildState==="running")return;
      buildState="running";buildFile="";buildLog="Priprema lokalne izrade "+name.toUpperCase()+"...\n";render();
      try{
        const result=await api.buildProject(p.id,name,{...signing});
        if(!result){buildState="idle";render();return;}
        buildState="done";buildFile=result.output;
        buildLog+="\nUspješno izrađeno. "+(result.signed?"Paket je potpisan.":"Paket NIJE potpisan.")+"\n";
        render();notify("Izrada uspješno završena.");
      }catch(e){buildState="error";buildLog+="\nPOGREŠKA: "+e.message;render();notify(e.message);}
      return;
    }
    if(name==="delete"){
      if(!savedId)return notify("Za brisanje prvo otvori projekt s popisa.");
      if(!window.confirm("Obrisati odabrani projekt iz lokalnog popisa?"))return;
      await api.deleteProject(savedId);savedId=null;draft={...initial};await reload();notify("Projekt je uklonjen.");return;
    }
  }
  document.addEventListener("input",e=>{
    const el=e.target.closest("[data-field]");if(!el)return;
    const key=el.dataset.field;
    if(Object.prototype.hasOwnProperty.call(signing,key)){signing[key]=el.value;return;}
    if(Object.prototype.hasOwnProperty.call(draft,key)){draft[key]=["versionCode","minSdk","targetSdk"].includes(key)?Number(el.value):el.value;}
  });
  document.addEventListener("change",e=>{
    const el=e.target.closest("[data-field]");if(!el)return;
    const key=el.dataset.field;
    if(Object.prototype.hasOwnProperty.call(draft,key))draft[key]=["versionCode","minSdk","targetSdk"].includes(key)?Number(el.value):el.value;
  });
  document.addEventListener("click",async e=>{
    const el=e.target.closest("[data-action],[data-nav],[data-open],[data-toggle],[data-source],[data-color]");
    if(!el)return;
    try{
      if(el.dataset.action){await action(el.dataset.action);return;}
      if(el.dataset.open){
        const p=projects.find(x=>x.id===el.dataset.open);
        if(p){savedId=p.id;draft={...p};page="new";render();}return;
      }
      if(el.dataset.source){draft.sourceType=el.dataset.source;render();return;}
      if(el.dataset.color){draft.accent=el.dataset.color;render();return;}
      if(el.dataset.toggle){const k=el.dataset.toggle;draft[k]=!draft[k];render();return;}
      if(el.dataset.nav){
        if(el.dataset.nav==="new" && !savedId){startNew();return;}
        page=el.dataset.nav;render();
        if(page==="devices" && api)await action("devices");
        return;
      }
    }catch(err){notify(err&&err.message?err.message:"Neočekivana pogreška.");}
  });
  if(api){
    api.onBuildLog(msg=>{buildLog+=String(msg).slice(0,8000);buildLog=buildLog.slice(-35000);const box=document.querySelector("#build-log");if(box){box.textContent=buildLog;box.scrollTop=box.scrollHeight;}});
    reload().catch(e=>notify(e.message));
  }else render();
})();
