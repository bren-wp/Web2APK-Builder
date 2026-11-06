package com.brendigo.web2apk.client;

import android.content.Context;
import android.content.SharedPreferences;
import android.util.Base64;
import org.json.JSONObject;
import java.nio.charset.StandardCharsets;
import java.security.KeyFactory;
import java.security.MessageDigest;
import java.security.PublicKey;
import java.security.Signature;
import java.security.spec.X509EncodedKeySpec;
import java.util.Map;

/** Offline verification of signed BL2 entitlements. The issuer private key is never on Android. */
public final class LicenseVerifier {
    private static final String PUBLIC_SPKI = "MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAlz/Lh4EZM5h29mbVGielXikIkmh0qrlCVnhwhOodnSSo+AP4KrDCCTFC0kxhJ85a9jaV3i5vHrRXHfV1BnE7pl0yLv43AiwCKIp6kblBp8mg94LtGvxWdC/fsQymdINRI0hi5ACADRTHnjka3PXUUJS7bLVysbSEaOUSCQ7PNJZFhIYfcuddOMz018BEDxMnINtkR0+M0WaKRSDYuHKD0M88Yl4ezztpExMMRg2KmlYBB+Rk5hHyGFXhq8rMhlImI9MoNZwrf+7zM+b/JzElvtZM1ptEENEs8fS+9uBz9fSeZiGo3uYl3FXOrVdsS4UzNxUyYc7L2H0EqTMMBeCsY8Kvo+3Y4fCl3lsYLolWcRj2NUsQey9UYWgOhZnFHZRhsH7YkFSEUaDVZdrcUn/k6s0vgFd9rSFJAnmDEjpJRFznOdSPAupn375Rd98vjkDYlDUoAGsA3q6UEU9+KYuIeVzTPKAwBp/rWnO+sB8IY8o2WOnRax7GiJAdiTvMux/NAgMBAAE=";
    private LicenseVerifier() {}
    public static final class Claims {
        public final String serial,tier,packageId;
        Claims(String s,String t,String p) {serial=s;tier=t;packageId=p;}
    }
    public static Claims inspect(String token) {
        if(token==null || token.length()>4096)return null;
        try {
            String[] parts=token.trim().split("\\.",-1);
            if(parts.length!=3 || !"BL2".equals(parts[0]))return null;
            if(!parts[1].matches("[A-Za-z0-9_-]+") || !parts[2].matches("[A-Za-z0-9_-]+"))return null;
            byte[] payload=Base64.decode(parts[1],Base64.URL_SAFE|Base64.NO_WRAP|Base64.NO_PADDING);
            byte[] signature=Base64.decode(parts[2],Base64.URL_SAFE|Base64.NO_WRAP|Base64.NO_PADDING);
            if(payload.length>1024 || signature.length!=384)return null;
            PublicKey pub=KeyFactory.getInstance("RSA").generatePublic(new X509EncodedKeySpec(
                Base64.decode(PUBLIC_SPKI,Base64.DEFAULT)));
            Signature verifier=Signature.getInstance("SHA256withRSA");
            verifier.initVerify(pub);
            verifier.update(payload);
            if(!verifier.verify(signature))return null;
            JSONObject json=new JSONObject(new String(payload,StandardCharsets.UTF_8));
            if(json.getInt("v")!=2)return null;
            String serial=json.getString("s"),tier=json.getString("t"),pkg=json.getString("p"),
                audience=json.getString("a");
            long issued=json.getLong("iat"),expires=json.getLong("exp");
            if(!serial.matches("[0-9a-f]{32}") || !(audience.equals("both")||audience.equals("android")) ||
                issued<=0 || issued>System.currentTimeMillis()/1000+300 || expires!=0)return null;
            if(tier.equals("developer")) {
                if(!pkg.equals("*"))return null;
            } else {
                if(!(tier.equals("demo")||tier.equals("basic")||tier.equals("pro")) ||
                   !pkg.matches("[a-z][a-z0-9_]*(\\.[a-z][a-z0-9_]*)+"))return null;
            }
            return new Claims(serial,tier,pkg);
        }catch(Exception ignored){return null;}
    }
    private static SharedPreferences prefs(Context context) {
        return context.getSharedPreferences("brendigo_license",Context.MODE_PRIVATE);
    }
    public static boolean verify(String token){return inspect(token)!=null;}
    public static boolean activated(Context context){return !available(context).isEmpty();}
    public static java.util.List<Claims> available(Context context) {
        java.util.ArrayList<Claims> result=new java.util.ArrayList<>();
        for(Map.Entry<String,?> row:prefs(context).getAll().entrySet()) {
            if(!(row.getValue() instanceof String))continue;
            Claims c=inspect((String)row.getValue());
            if(c!=null)result.add(c);
        }
        return result;
    }
    public static boolean activate(Context context,String token) {
        Claims c=inspect(token);
        if(c==null)return false;
        return prefs(context).edit().putString("license_"+c.serial,token.trim()).commit();
    }
    public static String masked(Context context,String serial){
        String token=prefs(context).getString("license_"+serial,"");
        if(token.length()<18)return "********";
        return token.substring(0,Math.min(10,token.length()))+"***************"+
            token.substring(token.length()-8);
    }
    public static boolean replace(Context context,String oldSerial,String token) {
        Claims previous=null;
        for(Claims c:available(context))if(c.serial.equals(oldSerial)){previous=c;break;}
        Claims next=inspect(token);
        if(previous==null || next==null)return false;
        if(!activate(context,token))return false;
        if(!oldSerial.equals(next.serial))
            return prefs(context).edit().remove("license_"+oldSerial).commit();
        return true;
    }
    public static Claims forPackage(Context ctx,String packageId) {
        Claims result=null;
        for(Claims c:available(ctx)) {
            if(!c.tier.equals("developer") && !c.packageId.equals(packageId))continue;
            if(result==null || rank(c.tier)>rank(result.tier))result=c;
        }
        return result;
    }
    private static int rank(String tier) {
        switch(tier){case "developer":return 4;case "pro":return 3;case "basic":return 2;default:return 1;}
    }
    public static boolean allows(Context ctx,String packageId,String feature) {
        Claims c=forPackage(ctx,packageId);
        if(c==null)return false;
        if(c.tier.equals("developer"))return true;
        if(feature.equals("apk"))return true;
        if(feature.equals("aab")||feature.equals("branding"))
            return c.tier.equals("basic")||c.tier.equals("pro");
        if(feature.equals("pull_refresh")||feature.equals("firebase_push"))
            return c.tier.equals("pro");
        return false;
    }
    public static String packageError(Context ctx,JSONObject project) {
        String id=project.optString("PackageName");
        Claims c=forPackage(ctx,id);
        if(c==null)return "Nije dostupna licenca za package ID: "+id;
        if(c.tier.equals("demo") &&
            (!project.optBoolean("Splash",true) ||
             !project.optBoolean("BrendigoSplashBranding",true) ||
             project.optInt("SplashDurationMs",700)!=700 ||
             !project.optString("Theme","dark").equals("dark") ||
             !project.optString("Accent","#7253FF").equalsIgnoreCase("#7253FF") ||
             !project.optString("IconPath","").isEmpty()))
            return "Demo zahtijeva nepromjenjivu Brendigo ikonu i splash zaslon.";
        if(project.optBoolean("AdMobEnabled") && !allows(ctx,id,"admob"))
            return "AdMob je dostupan samo u paketu Developer.";
        if(project.optBoolean("FirebasePushRequested") && !allows(ctx,id,"firebase_push"))
            return "Push obavijesti dostupne su u paketima Profesionalac i Developer.";
        if(project.optBoolean("PullRefresh") && !allows(ctx,id,"pull_refresh"))
            return "Povucite za osvježavanje dostupno je u paketima Profesionalac i Developer.";
        if(project.optBoolean("Camera")||project.optBoolean("Microphone")||project.optBoolean("Location"))
            if(!c.tier.equals("developer"))return "Kamera, mikrofon i lokacija dostupni su samo u paketu Developer.";
        return null;
    }
}
