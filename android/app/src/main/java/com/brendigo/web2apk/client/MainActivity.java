package com.brendigo.web2apk.client;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Color;
import android.graphics.Typeface;
import android.graphics.drawable.GradientDrawable;
import android.net.Uri;
import android.os.Bundle;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.ProgressBar;
import java.util.ArrayList;
import androidx.core.splashscreen.SplashScreen;
import org.json.JSONArray;
import org.json.JSONObject;
import java.nio.charset.StandardCharsets;
import java.util.Date;
import java.text.SimpleDateFormat;
import java.util.TimeZone;
import java.util.Locale;
import java.io.ByteArrayOutputStream;
import java.util.UUID;

public final class MainActivity extends Activity {
    private static final int BG=0xFF090F21, PANEL=0xFF151F3A, BORDER=0xFF354573,
        PRIMARY=0xFF7853F6, TEXT=0xFFF1F4FF, MUTED=0xFFA7B6D6;
    private static final int OPEN_JSON=2001, SAVE_JSON=2002;
    private LinearLayout content;
    private JSONArray projects = new JSONArray();
    private JSONObject editing;
    private WebView preview;
    private EditText name,packageName,url,version,adApp,adBanner;
    private CheckBox javascript,admob,allowHttp,camera,microphone,location,uploads,downloads,splash,firebase,dom; 
    private EditText splashDuration,privacyUrl;
    private CheckBox ownerConfirmed,dataSafetyChecked;
    private String exportedJson;
    private int wizardStep=-1;
    private final ArrayList<LinearLayout> wizardSections = new ArrayList<>();
    private LinearLayout wizardNavigation;
    private LinearLayout wizardProgress;
    private TextView wizardHeading;
    private ScrollView pageScroll;
    private String currentPage = "";
    private static final String[] STEPS={"Osnovni podaci","Izgled aplikacije",
        "WebView postavke","Funkcije","Android dozvole","Izrada aplikacije"};

    @Override public void onCreate(Bundle saved) {
        SplashScreen.installSplashScreen(this);
        super.onCreate(saved);
        getWindow().setStatusBarColor(BG);
        getWindow().setNavigationBarColor(BG);
        try { projects=new JSONArray(getPreferences(0).getString("projects","[]")); }
        catch(Exception ignored){ projects=new JSONArray(); }
        if(!getPreferences(0).getBoolean("welcome_completed",false)) onboarding();
        else dashboard();
    }
    private int dp(float value){ return (int)(value*getResources().getDisplayMetrics().density+.5f); }
    private GradientDrawable background(int color,int radius) {
        GradientDrawable gd=new GradientDrawable();
        gd.setColor(color);gd.setCornerRadius(dp(radius));gd.setStroke(dp(1),BORDER);
        return gd;
    }
    private TextView text(String value,int sp,int color,boolean bold){
        TextView v=new TextView(this);v.setText(value);v.setTextColor(color);v.setTextSize(sp);
        v.setTypeface(Typeface.DEFAULT,bold?Typeface.BOLD:Typeface.NORMAL);
        v.setLineSpacing(dp(2),1f);return v;
    }
    private void page(String title, String subtitle) {
        currentPage=title;
        if(preview!=null){preview.destroy();preview=null;}
        LinearLayout root=new LinearLayout(this);root.setOrientation(LinearLayout.VERTICAL);
        root.setBackground(new GradientDrawable(GradientDrawable.Orientation.TL_BR,
            new int[]{0xFF0C1230,0xFF191743,0xFF0E1C3C}));
        if(android.os.Build.VERSION.SDK_INT>=35){
            root.setOnApplyWindowInsetsListener((view,insets)->{
                android.graphics.Insets safe=insets.getInsets(android.view.WindowInsets.Type.systemBars());
                view.setPadding(safe.left,safe.top,safe.right,safe.bottom);
                return insets;
            });
        }
        LinearLayout header=new LinearLayout(this);header.setPadding(dp(18),dp(15),dp(18),dp(14));
        header.setGravity(Gravity.CENTER_VERTICAL);header.setBackgroundColor(0xFF111B35);
        TextView logo=text("B",23,Color.WHITE,true);logo.setGravity(Gravity.CENTER);
        logo.setBackground(background(PRIMARY,11));
        header.addView(logo,new LinearLayout.LayoutParams(dp(42),dp(42)));
        LinearLayout identity=new LinearLayout(this);identity.setOrientation(LinearLayout.VERTICAL);
        LinearLayout.LayoutParams idp=new LinearLayout.LayoutParams(0,-2,1);idp.setMargins(dp(11),0,0,0);
        identity.addView(text("Brendigo",15,Color.WHITE,true));
        identity.addView(text("Web2APK Builder",11,0xFFC6ADFF,true));
        header.addView(identity,idp);
        Button home=button("⌂",this::dashboard,false);
        header.addView(home,new LinearLayout.LayoutParams(dp(48),dp(43)));
        root.addView(header);
        ScrollView scroll=new ScrollView(this);scroll.setFillViewport(true);
        pageScroll=scroll;
        content=new LinearLayout(this);content.setOrientation(LinearLayout.VERTICAL);
        int screenWidth=getResources().getConfiguration().screenWidthDp;
        int inset=screenWidth>690 ? Math.max(20,(screenWidth-620)/2) : 19;
        content.setPadding(dp(inset),dp(22),dp(inset),dp(32));
        content.addView(text(title,26,TEXT,true));
        label(subtitle,12,MUTED,0,17);
        scroll.addView(content);
        root.addView(scroll,new LinearLayout.LayoutParams(-1,0,1));
        root.addView(bottomNavigation(title));
        setContentView(root);
    }

    private LinearLayout bottomNavigation(String current) {
        LinearLayout nav=new LinearLayout(this);
        nav.setOrientation(LinearLayout.HORIZONTAL);
        nav.setBackgroundColor(0xFF10182F);
        nav.setPadding(dp(4),dp(6),dp(4),dp(6));
        final String[] names={"Početna","Projekti","Novo","Uređaji","Postavke"};
        final String[] glyphs={"⌂","▢","+","▣","⚙"};
        final Runnable[] routes={this::dashboard,this::list,()->editor(null),this::devices,this::settings};
        for(int i=0;i<names.length;i++){
            LinearLayout item=new LinearLayout(this);
            item.setOrientation(LinearLayout.VERTICAL);item.setGravity(Gravity.CENTER);
            boolean selected=(i==0&&current.equals("Web → Android")) ||
                (i==1&&current.equals("Moji projekti")) ||
                (i==2&&current.equals("Novi Android projekt")) ||
                (i==3&&current.equals("Uređaji")) ||
                (i==4&&current.equals("Postavke"));
            TextView ico=text(glyphs[i],i==2?24:19,selected?0xFFAB97FF:MUTED,true);
            ico.setGravity(Gravity.CENTER);item.addView(ico);
            TextView caption=text(names[i],10,selected?TEXT:MUTED,false);
            caption.setGravity(Gravity.CENTER);item.addView(caption);
            item.setContentDescription(names[i]);
            final Runnable route=routes[i];
            item.setOnClickListener(v->route.run());
            nav.addView(item,new LinearLayout.LayoutParams(0,dp(51),1));
        }
        return nav;
    }
    private void onboarding(){
        page("Vaši projekti. Uvijek uz vas.",
            "Izradite i uredite projekt na telefonu, a Android paket pripremite putem Windows aplikacije.");
        LinearLayout hero=card();
        hero.setBackground(background(0xFF2B2567,19));
        TextView mark=text("B",68,Color.WHITE,true);mark.setGravity(Gravity.CENTER);
        hero.addView(mark,new LinearLayout.LayoutParams(-1,dp(130)));
        info(hero,"Brendigo Web2APK Builder",
            "Jedan projekt, dosljedan dizajn i siguran prijenos između uređaja.");
        addButton(hero,"Započni  →",()->{
            getPreferences(0).edit().putBoolean("welcome_completed",true).apply();
            dashboard();
        },true);
    }

    private void label(String value,int sp,int color,int top,int bottom){
        TextView v=text(value,sp,color,false);
        LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(-1,-2);
        lp.setMargins(0,dp(top),0,dp(bottom));content.addView(v,lp);
    }
    private LinearLayout card(){
        LinearLayout c=new LinearLayout(this);c.setOrientation(LinearLayout.VERTICAL);
        c.setPadding(dp(17),dp(17),dp(17),dp(17));c.setBackground(background(PANEL,15));
        LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(-1,-2);lp.bottomMargin=dp(13);
        content.addView(c,lp);return c;
    }
    private Button button(String label,Runnable click,boolean primary){
        Button b=new Button(this);
        b.setText(label);b.setTextColor(TEXT);b.setTextSize(12);b.setAllCaps(false);
        if(primary) {
            GradientDrawable gradient=new GradientDrawable(GradientDrawable.Orientation.LEFT_RIGHT,
                new int[]{0xFF4D8CFF,0xFF8752F3});
            gradient.setCornerRadius(dp(11));
            b.setBackground(gradient);
        } else b.setBackground(background(0xFF1D2D50,11));
        b.setOnClickListener(v->click.run());return b;
    }
    private void addButton(LinearLayout parent,String title,Runnable run,boolean primary){
        Button b=button(title,run,primary);
        LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(-1,dp(46));
        lp.bottomMargin=dp(9);parent.addView(b,lp);
    }
    private void info(LinearLayout c,String title,String sub){
        TextView h=text(title,15,TEXT,true);c.addView(h);
        TextView desc=text(sub,12,MUTED,false);
        LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(-1,-2);lp.topMargin=dp(6);lp.bottomMargin=dp(12);
        c.addView(desc,lp);
    }
    private EditText field(LinearLayout c,String title,String value){
        TextView caption=text(title,12,0xFFCBD7F4,true);
        LinearLayout.LayoutParams cp=new LinearLayout.LayoutParams(-1,-2);
        cp.topMargin=dp(13);cp.bottomMargin=dp(5);c.addView(caption,cp);
        EditText edit=new EditText(this);edit.setSingleLine(true);edit.setText(value);
        edit.setTextSize(13);edit.setTextColor(TEXT);edit.setHintTextColor(MUTED);
        edit.setPadding(dp(13),dp(8),dp(13),dp(8));edit.setBackground(background(0xFF0C1731,9));
        c.addView(edit,new LinearLayout.LayoutParams(-1,dp(44)));
        return edit;
    }
    private CheckBox check(LinearLayout c,String title,boolean checked){
        CheckBox box=new CheckBox(this);
        box.setText(title);box.setTextColor(TEXT);box.setTextSize(12);box.setChecked(checked);
        c.addView(box);return box;
    }
    private void persist(){
        if(!getPreferences(0).edit().putString("projects",projects.toString()).commit())
            Toast.makeText(this,"Spremanje nije uspjelo",Toast.LENGTH_LONG).show();
    }
    private JSONObject find(String id){
        for(int i=0;i<projects.length();i++) {
            JSONObject p=projects.optJSONObject(i);
            if(p!=null && id.equals(p.optString("Id")))return p;
        }
        return null;
    }
    private void dashboard(){
        page("Web → Android","Vaš Android studio za pripremu i pregled projekata.");
        LinearLayout hero=card();hero.setBackground(background(0xFF262762,17));
        info(hero,"Web u aplikaciju, od ideje do izdanja.","Uredite projekt i nastavite izradu na računalu.");
        addButton(hero,"＋  Novi projekt",()->editor(null),true);
        addButton(hero,"▣  Moji projekti ("+projects.length()+")",this::list,false);
        LinearLayout stats=new LinearLayout(this);
        stats.setOrientation(LinearLayout.HORIZONTAL);
        LinearLayout.LayoutParams statParams=new LinearLayout.LayoutParams(-1,-2);
        statParams.setMargins(0,0,0,dp(12));
        content.addView(stats,statParams);
        LinearLayout statOne=new LinearLayout(this);statOne.setOrientation(LinearLayout.VERTICAL);
        statOne.setBackground(background(PANEL,14));statOne.setPadding(dp(14),dp(14),dp(14),dp(14));
        statOne.addView(text(String.valueOf(projects.length()),23,TEXT,true));
        statOne.addView(text("Ukupno projekata",11,MUTED,false));
        stats.addView(statOne,new LinearLayout.LayoutParams(0,dp(86),1));
        LinearLayout statTwo=new LinearLayout(this);statTwo.setOrientation(LinearLayout.VERTICAL);
        statTwo.setBackground(background(PANEL,14));statTwo.setPadding(dp(14),dp(14),dp(14),dp(14));
        int httpsCount=0;
        for(int i=0;i<projects.length();i++){
            JSONObject p=projects.optJSONObject(i);
            if(p!=null && !p.optBoolean("AllowHttp")) httpsCount++;
        }
        statTwo.addView(text(String.valueOf(httpsCount),23,TEXT,true));
        statTwo.addView(text("HTTPS projekti",11,MUTED,false));
        LinearLayout.LayoutParams statTwoParams=new LinearLayout.LayoutParams(0,dp(86),1);
        statTwoParams.leftMargin=dp(10);stats.addView(statTwo,statTwoParams);
        LinearLayout recent=card();
        info(recent,"Posljednji projekti","Brzi pristup spremljenim aplikacijama.");
        for(int i=projects.length()-1;i>=Math.max(0,projects.length()-3);i--){
            JSONObject project=projects.optJSONObject(i);
            if(project==null)continue;
            String id=project.optString("Id");
            addButton(recent,"▣  "+project.optString("Name","Projekt"),()->details(id),false);
        }
        if(projects.length()==0)
            recent.addView(text("Izradite prvi projekt i pojavit će se ovdje.",12,MUTED,false));
        LinearLayout tools=card();
        info(tools,"Brzi alati","Bez vanjske pohrane, bez registracije. Projekti su spremljeni lokalno.");
        addButton(tools,"↥  Uvezi projekt iz Windowsa",this::importProject,false);
        addButton(tools,"⚙  Postavke i mogućnosti",this::settings,false);
        label("Brendigo · brendigo.com",11,MUTED,15,0);
    }
    private void list(){
        page("Moji projekti","Pregledajte, uredite i izvezite svoje projekte.");
        LinearLayout actions=card();addButton(actions,"＋  Novi projekt",()->editor(null),true);
        addButton(actions,"↓  Uvezi JSON konfiguraciju",this::importProject,false);
        if(projects.length()==0){label("Nema spremljenih projekata.",14,MUTED,10,0);return;}
        for(int i=0;i<projects.length();i++) {
            JSONObject p=projects.optJSONObject(i);if(p==null)continue;
            String id=p.optString("Id"),address=p.optString("Url","https://example.com");
            LinearLayout c=card();info(c,p.optString("Name","Projekt"),p.optString("PackageName")+"\n"+address);
            addButton(c,"Detalji projekta",()->details(id),true);
            addButton(c,"Uredi projekt",()->editor(id),false);
            addButton(c,"Pregled web-stranice",()->preview(id),false);
            addButton(c,"Izvezi konfiguraciju",()->exportProject(id),false);
            addButton(c,"Obriši projekt",()->delete(id),false);
        }
    }
    private JSONObject blank(){
        JSONObject p=new JSONObject();
        try {
            p.put("Id",UUID.randomUUID().toString()).put("Name","Moja web aplikacija")
             .put("PackageName","com.brendigo.mojaaplikacija").put("Url","https://example.com")
             .put("SourceType","url").put("VersionName","1.0.0").put("VersionCode",1)
             .put("MinSdk",24).put("TargetSdk",36).put("JavaScript",true)
             .put("DomStorage",true).put("AllowHttp",false).put("OpenExternal",true)
             .put("Theme","dark").put("Accent","#7253FF")
             .put("AdMobEnabled",false).put("FirebasePushEnabled",false)
             .put("Splash",true).put("SplashDurationMs",700)
             .put("FirebasePushRequested",false)
             .put("PrivacyPolicyUrl","").put("WebsitePublishingPermissionConfirmed",false)
             .put("DataSafetyReviewed",false)
             .put("FileUploads",true).put("Downloads",true);
        } catch(Exception ignored){}
        return p;
    }
    private void editor(String id){
        editing=id==null?blank():find(id);
        if(editing==null){list();return;}
        wizardStep=0;
        wizardSections.clear();
        page("Novi Android projekt","Postavite izgled, funkcije i dozvole u šest koraka.");
        wizardHeading=text("",16,TEXT,true);
        content.addView(wizardHeading);
        wizardProgress=new LinearLayout(this);
        wizardProgress.setOrientation(LinearLayout.HORIZONTAL);
        LinearLayout.LayoutParams progressParams=new LinearLayout.LayoutParams(-1,dp(7));
        progressParams.setMargins(0,dp(12),0,dp(16));
        content.addView(wizardProgress,progressParams);
        LinearLayout c=card();
        info(c,"Osnovne informacije","Konfiguracija je kompatibilna s Brendigo Windows Studijem.");
        name=field(c,"Naziv aplikacije",editing.optString("Name"));
        packageName=field(c,"Package ID",editing.optString("PackageName"));
        url=field(c,"HTTPS web-adresa",editing.optString("Url"));
        version=field(c,"Verzija",editing.optString("VersionName","1.0.0"));
        LinearLayout opts=card();info(opts,"WebView i sigurnost","Provjera HTTPS-a uključena je prema zadanim postavkama.");
        javascript=check(opts,"JavaScript",editing.optBoolean("JavaScript",true));
        dom=check(opts,"DOM Storage",editing.optBoolean("DomStorage",true));
        allowHttp=check(opts,"Dopusti HTTP (nesigurno)",editing.optBoolean("AllowHttp",false));
        LinearLayout permissions=card();
        info(permissions,"Android dozvole","Odaberite samo dozvole koje su potrebne generiranoj aplikaciji.");
        camera=check(permissions,"Kamera / WebRTC",editing.optBoolean("Camera",false));
        microphone=check(permissions,"Mikrofon / WebRTC",editing.optBoolean("Microphone",false));
        location=check(permissions,"Približna lokacija",editing.optBoolean("Location",false));
        uploads=check(permissions,"Odabir datoteka",editing.optBoolean("FileUploads",true));
        downloads=check(permissions,"Preuzimanja",editing.optBoolean("Downloads",true));
        LinearLayout style=card();
        info(style,"Splash i pokretanje","Prilagodite trajanje izvornog Android splash zaslona.");
        splash=check(style,"Dinamički splash",editing.optBoolean("Splash",true));
        splashDuration=field(style,"Splash trajanje u milisekundama (0–3000)",
            String.valueOf(editing.optInt("SplashDurationMs",700)));
        LinearLayout play=card();
        info(play,"Google Play · politika privatnosti",
            "Za objavu je potrebna javna politika privatnosti i točne deklaracije o podacima.");
        privacyUrl=field(play,"HTTPS URL politike privatnosti",editing.optString("PrivacyPolicyUrl"));
        ownerConfirmed=check(play,"Imam dopuštenje vlasnika za web-stranicu",
            editing.optBoolean("WebsitePublishingPermissionConfirmed",false));
        dataSafetyChecked=check(play,"Pregledao/la sam Data safety i Google SDK-ove",
            editing.optBoolean("DataSafetyReviewed",false));
        LinearLayout ads=card();info(ads,"Google AdMob","Za stvarne oglase potreban je vlastiti račun i korisnička privola.");
        admob=check(ads,"Uključi AdMob banner",editing.optBoolean("AdMobEnabled",false));
        adApp=field(ads,"AdMob App ID (ca-app-pub-…~…)",editing.optString("AdMobAppId"));
        adBanner=field(ads,"Banner Ad Unit ID (ca-app-pub-…/…)",editing.optString("AdMobBannerId"));
        LinearLayout push=card();
        info(push,"Firebase Push","Za rad obavijesti Windows Studio treba odgovarajući google-services.json.");
        firebase=check(push,"Zahtijevaj Firebase Push u Windows Studiju",
            editing.optBoolean("FirebasePushRequested",false));
        LinearLayout footer=card();
        info(footer,"Spremno za izvoz",
            "Spremite konfiguraciju, a APK/AAB izradite i potpišite u Brendigo Windows aplikaciji.");
        addButton(footer,"✓  Spremi projekt",this::saveEdit,true);
        addButton(footer,"←  Odustani",this::list,false);
        label("Privatni ključevi i lozinke ne prenose se izvozom projekta.",12,MUTED,9,0);
        wizardSections.add(c);
        wizardSections.add(style);
        wizardSections.add(opts);
        wizardSections.add(ads);
        wizardSections.add(push);
        wizardSections.add(permissions);
        wizardSections.add(play);
        wizardSections.add(footer);
        wizardNavigation=card();
        showWizardStep(0);
    }
    private void showWizardStep(int step){
        wizardStep=Math.max(0,Math.min(5,step));
        wizardHeading.setText(STEPS[wizardStep]+"    "+(wizardStep+1)+"/6");
        wizardProgress.removeAllViews();
        for(int i=0;i<6;i++){
            View segment=new View(this);
            segment.setBackground(background(i<=wizardStep?0xFF7760FF:0xFF263253,4));
            LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(0,dp(5),1);
            lp.setMargins(dp(2),0,dp(2),0);wizardProgress.addView(segment,lp);
        }
        for(int i=0;i<wizardSections.size();i++){
            boolean visible= (wizardStep==0&&i==0) || (wizardStep==1&&i==1) ||
                (wizardStep==2&&i==2) || (wizardStep==3&&(i==3||i==4)) ||
                (wizardStep==4&&(i==5||i==6)) || (wizardStep==5&&i==7);
            wizardSections.get(i).setVisibility(visible?View.VISIBLE:View.GONE);
        }
        wizardNavigation.removeAllViews();
        if(wizardStep>0)addButton(wizardNavigation,"←  Natrag",
            ()->showWizardStep(wizardStep-1),false);
        if(wizardStep<5)addButton(wizardNavigation,"Nastavi  →",
            ()->showWizardStep(wizardStep+1),true);
        if(wizardStep==5)addButton(wizardNavigation,"Spremi i dovrši",
            this::saveEdit,true);
        if(pageScroll!=null)pageScroll.smoothScrollTo(0,0);
    }

    private static String utcNow() {
        SimpleDateFormat date=new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss'Z'",Locale.US);
        date.setTimeZone(TimeZone.getTimeZone("UTC"));
        return date.format(new Date());
    }
    private boolean validUrl(String value, boolean http){
        try {
            Uri uri=Uri.parse(value);
            String proto=uri.getScheme();
            return uri.getHost()!=null && value.length()<=2000 &&
                ("https".equalsIgnoreCase(proto)||(http&&"http".equalsIgnoreCase(proto))) &&
                uri.getUserInfo()==null;
        } catch(Exception ignored){return false;}
    }
    private void saveEdit(){
        try {
            String title=name.getText().toString().trim(),pkg=packageName.getText().toString().trim();
            String address=url.getText().toString().trim();
            if(title.length()<2 || !pkg.matches("[a-z][a-z0-9_]*(\\.[a-z][a-z0-9_]*)+") ||
                !validUrl(address,allowHttp.isChecked()))
                throw new IllegalArgumentException("Provjerite naziv, package ID i HTTPS URL.");
            int ms;
            try { ms=Integer.parseInt(splashDuration.getText().toString().trim()); }
            catch(NumberFormatException ex){throw new IllegalArgumentException("Splash trajanje nije cijeli broj.");}
            if(ms<0 || ms>3000)throw new IllegalArgumentException("Splash mora trajati 0–3000 ms.");
            String appVersion=version.getText().toString().trim();
            if(!appVersion.matches("[0-9]+\\.[0-9]+(\\.[0-9]+)?([.-][a-zA-Z0-9]+)?"))
                throw new IllegalArgumentException("Verzija mora biti oblika 1.0.0.");
            String privacy=privacyUrl.getText().toString().trim();
            if(!privacy.isEmpty() && !validUrl(privacy,false))
                throw new IllegalArgumentException("Politika privatnosti mora imati valjan HTTPS URL.");
            if(admob.isChecked() &&
                (!adApp.getText().toString().matches("ca-app-pub-[0-9]{16}~[0-9]{10}") ||
                 !adBanner.getText().toString().matches("ca-app-pub-[0-9]{16}/[0-9]{10}")))
                throw new IllegalArgumentException("AdMob App ID ili Banner ID nije ispravan.");
            editing.put("Name",title).put("PackageName",pkg).put("Url",address)
                .put("VersionName",appVersion)
                .put("JavaScript",javascript.isChecked()).put("DomStorage",dom.isChecked())
                .put("Camera",camera.isChecked()).put("Microphone",microphone.isChecked())
                .put("Location",location.isChecked()).put("FileUploads",uploads.isChecked())
                .put("Downloads",downloads.isChecked()).put("Splash",splash.isChecked())
                .put("SplashDurationMs",ms).put("FirebasePushRequested",firebase.isChecked())
                .put("PrivacyPolicyUrl",privacy)
                .put("WebsitePublishingPermissionConfirmed",ownerConfirmed.isChecked())
                .put("DataSafetyReviewed",dataSafetyChecked.isChecked())
                .put("AllowHttp",allowHttp.isChecked())
                .put("AdMobEnabled",admob.isChecked())
                .put("AdMobAppId",adApp.getText().toString().trim())
                .put("AdMobBannerId",adBanner.getText().toString().trim())
                .put("UpdatedAt",utcNow());
            boolean found=false;
            for(int i=0;i<projects.length();i++) {
                if(editing.optString("Id").equals(projects.optJSONObject(i).optString("Id"))) {
                    projects.put(i,editing);found=true;break;
                }
            }
            if(!found)projects.put(editing);
            persist();Toast.makeText(this,"Projekt je spremljen.",Toast.LENGTH_SHORT).show();list();
        } catch(Exception e){Toast.makeText(this,e.getMessage(),Toast.LENGTH_LONG).show();}
    }
    private void preview(String id) {
        JSONObject p=find(id);if(p==null)return;
        String address=p.optString("Url");
        if(!validUrl(address,p.optBoolean("AllowHttp"))){Toast.makeText(this,"Neispravan URL",Toast.LENGTH_LONG).show();return;}
        page("Pregled aplikacije",p.optString("Name"));
        var c=card();info(c,"Web pregled","Pregled se učitava u lokalno izoliranom WebViewu.");
        addButton(c,"←  Natrag na projekte",this::list,false);
        preview=new WebView(this);
        preview.getSettings().setJavaScriptEnabled(p.optBoolean("JavaScript",true));
        preview.getSettings().setDomStorageEnabled(p.optBoolean("DomStorage",true));
        preview.getSettings().setAllowFileAccess(false);
        preview.getSettings().setAllowContentAccess(false);
        preview.getSettings().setAllowUniversalAccessFromFileURLs(false);
        preview.setWebViewClient(new WebViewClient(){
            @Override public boolean shouldOverrideUrlLoading(WebView v, android.webkit.WebResourceRequest request) {
                Uri u=request.getUrl();
                Uri original=Uri.parse(address);
                return !validUrl(u.toString(),p.optBoolean("AllowHttp")) ||
                    !original.getHost().equalsIgnoreCase(u.getHost());
            }
        });
        LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(-1,dp(450));
        content.addView(preview,lp);preview.loadUrl(address);
    }

    private void details(String id){
        JSONObject p=find(id);if(p==null){list();return;}
        page("Detalji projekta",p.optString("Name","Projekt"));
        LinearLayout main=card();
        info(main,"Projekt",p.optString("Name"));
        info(main,"Web-adresa",p.optString("Url"));
        info(main,"Package ID",p.optString("PackageName"));
        info(main,"Verzija",p.optString("VersionName","1.0.0"));
        addButton(main,"▷  Pregled web-stranice",()->preview(id),true);
        LinearLayout actions=card();
        info(actions,"Radnje","Uredite projekt ili izvezite konfiguraciju za drugo računalo.");
        addButton(actions,"Uredi projekt",()->editor(id),false);
        addButton(actions,"Izvezi konfiguraciju",()->exportProject(id),false);
        addButton(actions,"Obriši projekt",()->delete(id),false);
    }
    private void devices(){
        page("Uređaji","Radite s projektima na telefonu i računalu.");
        LinearLayout mine=card();
        info(mine,"Ovaj telefon",android.os.Build.MANUFACTURER+" "+android.os.Build.MODEL+
            "\nAndroid "+android.os.Build.VERSION.RELEASE);
        LinearLayout transfer=card();
        info(transfer,"Prijenos projekata",
            "Uvezite ili izvezite projekt putem sustavnog odabira datoteka. Za prijenos na računalo koristite željenu dostupnu metodu.");
        addButton(transfer,"Uvezi projekt",this::importProject,true);
        addButton(transfer,"Odaberi projekt za izvoz",this::list,false);
    }
    private void delete(String id) {
        new android.app.AlertDialog.Builder(this).setTitle("Obrisati projekt?")
            .setMessage("Projekt će biti uklonjen samo s ovog uređaja.")
            .setNegativeButton("Odustani",null)
            .setPositiveButton("Obriši",(dialog,which)->{
                JSONArray updated=new JSONArray();
                for(int i=0;i<projects.length();i++){
                    JSONObject p=projects.optJSONObject(i);
                    if(p!=null&&!id.equals(p.optString("Id")))updated.put(p);
                }
                projects=updated;persist();list();
            }).show();
    }
    private void exportProject(String id) {
        JSONObject p=find(id);if(p==null)return;
        exportedJson=p.toString();
        Intent intent=new Intent(Intent.ACTION_CREATE_DOCUMENT).setType("application/json")
            .addCategory(Intent.CATEGORY_OPENABLE).putExtra(Intent.EXTRA_TITLE,"Brendigo-"+p.optString("Name","project").replaceAll("[^a-zA-Z0-9_-]","-")+".json");
        startActivityForResult(intent,SAVE_JSON);
    }
    private void importProject() {
        Intent intent=new Intent(Intent.ACTION_OPEN_DOCUMENT).setType("application/json")
            .addCategory(Intent.CATEGORY_OPENABLE);
        startActivityForResult(intent,OPEN_JSON);
    }
    @Override protected void onActivityResult(int requestCode,int resultCode,Intent data){
        super.onActivityResult(requestCode,resultCode,data);
        if(resultCode!=RESULT_OK||data==null||data.getData()==null)return;
        Uri uri=data.getData();
        try {
            if(requestCode==SAVE_JSON) {
                try(var out=getContentResolver().openOutputStream(uri)){
                    if(out==null)throw new Exception("Datoteka nije dostupna.");
                    out.write(exportedJson.getBytes(StandardCharsets.UTF_8));out.flush();
                }
                Toast.makeText(this,"JSON konfiguracija izvezena.",Toast.LENGTH_LONG).show();
            }else if(requestCode==OPEN_JSON) {
                String raw;
                try(var in=getContentResolver().openInputStream(uri)) {
                    if(in==null)throw new Exception("Datoteka nije dostupna.");
                    ByteArrayOutputStream buffer=new ByteArrayOutputStream();
                    byte[] chunk=new byte[8192];
                    int read;
                    while((read=in.read(chunk))!=-1) {
                        if(buffer.size()+read>256*1024)throw new Exception("Datoteka je prevelika.");
                        buffer.write(chunk,0,read);
                    }
                    raw=buffer.toString("UTF-8");
                }
                JSONObject p=new JSONObject(raw);
                String src=p.optString("SourceType","url");
                if(!"url".equals(src)||!validUrl(p.optString("Url"),p.optBoolean("AllowHttp")))
                    throw new Exception("Podržani su samo valjani URL projekti.");
                if(p.optString("Name").trim().length()<2 ||
               !p.optString("PackageName").matches("[a-z][a-z0-9_]*(\\.[a-z][a-z0-9_]*)+"))
                throw new Exception("Naziv ili Android package ID nije valjan.");
            p.put("Id",UUID.randomUUID().toString());
                p.remove("FirebaseConfigPath");
                p.put("FirebasePushEnabled",false);
                projects.put(p);persist();list();
                Toast.makeText(this,"Projekt je uvezen.",Toast.LENGTH_SHORT).show();
            }
        }catch(Exception ex){Toast.makeText(this,"Greška: "+ex.getMessage(),Toast.LENGTH_LONG).show();}
    }
    private void settings() {
        page("Postavke","Brendigo Mobile · sigurnost i kompatibilnost");
        LinearLayout c=card();
        info(c,"Podaci ostaju na telefonu","Bez računa, udaljene baze i pozadinskog nadzora.");
        info(c,"Prilagođen prikaz","Sadržaj se prilagođava mobitelu i tabletu.");
        info(c,"Izrada aplikacija","APK/AAB izrađuje se u povezanoj Windows aplikaciji.");
        addButton(c,"Uvezi JSON projekt",this::importProject,true);
        addButton(c,"Privatnost i obrada podataka",this::showPrivacyInformation,false);
        LinearLayout about=card();info(about,"Autor · Brendigo","brendigo.com · Web2APK Builder");
        addButton(about,"Otvori brendigo.com",()->{
            startActivity(new Intent(Intent.ACTION_VIEW,Uri.parse("https://brendigo.com")));
        },false);
    }
    private void showPrivacyInformation() {
        new android.app.AlertDialog.Builder(this)
            .setTitle("Privatnost · Brendigo Web2APK Mobile")
            .setMessage("Projekti se čuvaju lokalno na telefonu. Mobilni klijent nema korisnički račun, Firebase integraciju, AdMob ni analitiku. " +
                "Kada otvorite pregled svoje web-stranice, WebView uspostavlja izravnu vezu s odabranom web-stranicom; " +
                "ta stranica može zasebno obrađivati vaše podatke prema vlastitim pravilima. " +
                "Ako izvezete JSON, sami određujete primatelja. Podatke možete ukloniti brisanjem pojedinačnih projekata ili podataka aplikacije. " +
                "Za prijavu pitanja o privatnosti pogledajte službenu web-stranicu brendigo.com.")
            .setPositiveButton("U redu",null).show();
    }
    @Override public void onBackPressed(){
        if(preview!=null && preview.canGoBack())preview.goBack();
        else if(currentPage.equals("Novi Android projekt") && wizardStep>0)showWizardStep(wizardStep-1);
        else dashboard();
    }
    @Override protected void onDestroy(){if(preview!=null){preview.destroy();preview=null;}super.onDestroy();}
}
