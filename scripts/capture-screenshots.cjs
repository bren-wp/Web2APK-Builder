"use strict";
const { chromium } = require("playwright");
const path = require("node:path");
const fs = require("node:fs");
const { pathToFileURL } = require("node:url");
(async () => {
  const dest=path.join(__dirname,"..","docs","screenshots");
  fs.mkdirSync(dest,{recursive:true});
  const browser=await chromium.launch({headless:true});
  try{
    const page=await browser.newPage({viewport:{width:1440,height:900},deviceScaleFactor:1});
    await page.goto(pathToFileURL(path.join(__dirname,"..","src","ui","index.html")).href);
    await page.waitForTimeout(350);
    await page.screenshot({path:path.join(dest,"dashboard.png")});
    await page.locator('[data-nav="new"]').first().click();
    await page.screenshot({path:path.join(dest,"new-project.png")});
    const screens=[["appearance","appearance.png"],["webview","webview.png"],["features","features.png"],["permissions","permissions.png"],["build","build.png"]];
    for(const [name,file] of screens){
      await page.locator('[data-nav="'+name+'"]').first().click();
      await page.screenshot({path:path.join(dest,file)});
    }
    await page.locator('[data-nav="projects"]').first().click();
    await page.screenshot({path:path.join(dest,"projects.png")});
    await page.locator('[data-nav="settings"]').first().click();
    await page.screenshot({path:path.join(dest,"settings.png")});
    console.log("Captured actual Brendigo UI renderer screens to "+dest);
  }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
