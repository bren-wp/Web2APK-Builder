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
import androidx.core.splashscreen.SplashScreen;
import org.json.JSONArray;
import org.json.JSONObject;
import java.nio.charset.StandardCharsets;
import java.time.Instant;
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
    private CheckBox javascript,admob,allowHttp;
    private String exportedJson;

    @Override public void onCreate(Bundle saved) {
        SplashScreen.installSplashScreen(this);
        super.onCreate(saved);
        getWindow().setStatusBarColor(BG);
        getWindow().setNavigationBarColor(BG);
        try { projects=new JSONArray(getPreferences(0).getString("projects","[]")); }
        catch(Exception ignored){ projects=new JSONArray(); }
        dashboard();
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
        if(preview!=null){preview.destroy();preview=null;}
        LinearLayout root=new LinearLayout(this);root.setOrientation(LinearLayout.VERTICAL);root.setBackgroundColor(BG);
        LinearLayout header=new LinearLayout(this);header.setPadding(dp(18),dp(15),dp(18),dp(14));
        header.setGravity(Gravity.CENTER_VERTICAL);header.setBackgroundColor(0xFF111B35);
        TextView logo=text("B",23,Color.WHITE,true);logo.setGravity(Gravity.CENTER);
        logo.setBackground(background(PRIMARY,11));
        header.addView(logo,new LinearLayout.LayoutParams(dp(42),dp(42)));
        LinearLayout identity=new LinearLayout(this);identity.setOrientation(LinearLayout.VERTICAL);
        LinearLayout.LayoutParams idp=new LinearLayout.LayoutParams(0,-2,1);idp.setMargins(dp(11),0,0,0);
        identity.addView(text("Brendigo",15,Color.WHITE,true));
        identity.addView(text("Web2APK Builder · Mobile",11,0xFFC6ADFF,true));
        header.addView(identity,idp);
        Button home=button("⌂",this::dashboard,false);
        header.addView(home,new LinearLayout.LayoutParams(dp(48),dp(43)));
        root.addView(header);
        ScrollView scroll=new ScrollView(this);scroll.setFillViewport(true);
        content=new LinearLayout(this);content.setOrientation(LinearLayout.VERTICAL);
        content.setPadding(dp(19),dp(22),dp(19),dp(32));
        content.addView(text(title,26,TEXT,true));
        label(subtitle,12,MUTED,0,17);
        scroll.addView(content);
        root.addView(scroll,new LinearLayout.LayoutParams(-1,0,1));
        setContentView(root);
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
        b.setBackground(background(primary?PRIMARY:0xFF1D2D50,11));
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
        page("Web → Android","Upravljajte projektima s telefona. Izradu APK/AAB paketa dovršite u Windows Studiju.");
        LinearLayout hero=card();hero.setBackground(background(0xFF262762,17));
        info(hero,"Brendigo Web2APK Builder","Jednostavnije planiranje Android projekata, HTTPS pregled i siguran izvoz JSON konfiguracija.");
        addButton(hero,"＋  Novi projekt",()->editor(null),true);
        addButton(hero,"▣  Moji projekti ("+projects.length()+")",this::list,false);
        LinearLayout tools=card();
        info(tools,"Brzi alati","Bez vanjske pohrane, bez registracije. Projekti su spremljeni lokalno.");
        addButton(tools,"↥  Uvezi projekt iz Windowsa",this::importProject,false);
        addButton(tools,"⚙  Postavke i mogućnosti",this::settings,false);
        label("Brendigo · brendigo.com · Android Mobile v0.3.0",11,MUTED,15,0);
    }
    private void list(){
        page("Moji projekti","Spremljeni Android projekti · samo na ovom uređaju");
        LinearLayout actions=card();addButton(actions,"＋  Novi projekt",()->editor(null),true);
        addButton(actions,"↓  Uvezi JSON konfiguraciju",this::importProject,false);
        if(projects.length()==0){label("Nema spremljenih projekata.",14,MUTED,10,0);return;}
        for(int i=0;i<projects.length();i++) {
            JSONObject p=projects.optJSONObject(i);if(p==null)continue;
            String id=p.optString("Id"),address=p.optString("Url","https://example.com");
            LinearLayout c=card();info(c,p.optString("Name","Projekt"),p.optString("PackageName")+"\n"+address);
            addButton(c,"Uredi projekt",()->editor(id),true);
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
             .put("FileUploads",true).put("Downloads",true);
        } catch(Exception ignored){}
        return p;
    }
    private void editor(String id){
        editing=id==null?blank():find(id);
        if(editing==null){list();return;}
        page("Novi Android projekt","Osnovne postavke, WebView i opcionalne integracije.");
        LinearLayout c=card();
        info(c,"Osnovne informacije","Konfiguracija je kompatibilna s Brendigo Windows Studijem.");
        name=field(c,"Naziv aplikacije",editing.optString("Name"));
        packageName=field(c,"Package ID",editing.optString("PackageName"));
        url=field(c,"HTTPS web-adresa",editing.optString("Url"));
        version=field(c,"Verzija",editing.optString("VersionName","1.0.0"));
        LinearLayout opts=card();info(opts,"WebView i sigurnost","Provjera HTTPS-a uključena je prema zadanim postavkama.");
        javascript=check(opts,"JavaScript",editing.optBoolean("JavaScript",true));
        allowHttp=check(opts,"Dopusti HTTP (nesigurno)",editing.optBoolean("AllowHttp",false));
        LinearLayout ads=card();info(ads,"Google AdMob","Za stvarne oglase potreban je vlastiti račun i korisnička privola.");
        admob=check(ads,"Uključi AdMob banner",editing.optBoolean("AdMobEnabled",false));
        adApp=field(ads,"AdMob App ID (ca-app-pub-…~…)",editing.optString("AdMobAppId"));
        adBanner=field(ads,"Banner Ad Unit ID (ca-app-pub-…/…)",editing.optString("AdMobBannerId"));
        LinearLayout footer=card();
        addButton(footer,"✓  Spremi projekt",this::saveEdit,true);
        addButton(footer,"←  Odustani",this::list,false);
        label("Firebase Push: google-services.json datoteka odabire se u Windows Studiju. " +
            "Lozinke i privatni ključevi ne prenose se mobilnim JSON izvozom.",12,MUTED,9,0);
    }
    private boolean validUrl(String value, boolean http){
        try {
            Uri uri=Uri.parse(value);
            String proto=uri.getScheme();
            return uri.getHost()!=null &&
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
            if(admob.isChecked() &&
                (!adApp.getText().toString().matches("ca-app-pub-[0-9]{16}~[0-9]{10}") ||
                 !adBanner.getText().toString().matches("ca-app-pub-[0-9]{16}/[0-9]{10}")))
                throw new IllegalArgumentException("AdMob App ID ili Banner ID nije ispravan.");
            editing.put("Name",title).put("PackageName",pkg).put("Url",address)
                .put("VersionName",version.getText().toString().trim())
                .put("JavaScript",javascript.isChecked()).put("AllowHttp",allowHttp.isChecked())
                .put("AdMobEnabled",admob.isChecked())
                .put("AdMobAppId",adApp.getText().toString().trim())
                .put("AdMobBannerId",adBanner.getText().toString().trim())
                .put("UpdatedAt",Instant.now().toString());
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
        preview.getSettings().setDomStorageEnabled(true);
        preview.getSettings().setAllowFileAccess(false);
        preview.getSettings().setAllowContentAccess(false);
        preview.getSettings().setAllowUniversalAccessFromFileURLs(false);
        preview.setWebViewClient(new WebViewClient(){
            @Override public boolean shouldOverrideUrlLoading(WebView v, android.webkit.WebResourceRequest request) {
                Uri u=request.getUrl();
                return !validUrl(u.toString(),p.optBoolean("AllowHttp"));
            }
        });
        LinearLayout.LayoutParams lp=new LinearLayout.LayoutParams(-1,dp(450));
        content.addView(preview,lp);preview.loadUrl(address);
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
                    byte[] buffer=in.readNBytes(256*1024+1);
                    if(buffer.length>256*1024)throw new Exception("Datoteka je prevelika.");
                    raw=new String(buffer,StandardCharsets.UTF_8);
                }
                JSONObject p=new JSONObject(raw);
                String src=p.optString("SourceType","url");
                if(!"url".equals(src)||!validUrl(p.optString("Url"),p.optBoolean("AllowHttp")))
                    throw new Exception("Podržani su samo valjani URL projekti.");
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
        info(c,"Android 7.0+ (SDK 24)","Aplikacija se prilagođava širini zaslona i koristi izvorne Android kontrole.");
        info(c,"Android gradnja","Stvarni APK/AAB paket izrađuje Windows Studio pomoću Gradlea.");
        addButton(c,"Uvezi JSON projekt",this::importProject,true);
        LinearLayout about=card();info(about,"Autor · Brendigo","brendigo.com · Brendigo Web2APK Builder Mobile v0.3.0");
        addButton(about,"Otvori brendigo.com",()->{
            startActivity(new Intent(Intent.ACTION_VIEW,Uri.parse("https://brendigo.com")));
        },false);
    }
    @Override public void onBackPressed(){
        if(preview!=null && preview.canGoBack())preview.goBack();else dashboard();
    }
    @Override protected void onDestroy(){if(preview!=null){preview.destroy();preview=null;}super.onDestroy();}
}
