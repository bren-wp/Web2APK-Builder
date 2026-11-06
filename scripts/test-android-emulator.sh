#!/usr/bin/env bash
# Real Android emulator interaction smoke test; no mock UI captures.
set -euo pipefail
mkdir -p emulator-artifacts

capture() {
  local label="$1"
  adb exec-out screencap -p >"emulator-artifacts/${label}.png"
  adb shell uiautomator dump /sdcard/brendigo-ui.xml >/dev/null 2>&1 || true
  adb exec-out cat /sdcard/brendigo-ui.xml >"emulator-artifacts/${label}.xml" 2>/dev/null || true
}
check_process() {
  local package="$1"
  if ! adb shell pidof "$package" | grep -Eq '[0-9]'; then
    echo "FAIL: Android process stopped unexpectedly: $package" >&2
    adb logcat -d -t 200
    exit 1
  fi
}
wait_for_label() {
  local label="$1"
  local file="emulator-artifacts/current.xml"
  for attempt in $(seq 1 8); do
    adb shell uiautomator dump /sdcard/brendigo-ui.xml >/dev/null 2>&1 || true
    adb exec-out cat /sdcard/brendigo-ui.xml >"$file" 2>/dev/null || true
    if grep -Fq "$label" "$file"; then return 0; fi
    sleep 2
  done
  echo "FAIL: Android view is missing the expected text: $label" >&2
  exit 1
}
check_crashes() {
  local package="$1"
  adb logcat -d -v brief >"emulator-artifacts/${package##*.}-logcat.txt"
  if grep -Fq "Process: $package," "emulator-artifacts/${package##*.}-logcat.txt"; then
    echo "FAIL: Fatal crash of $package" >&2
    exit 1
  fi
}

adb wait-for-device
adb shell settings put global window_animation_scale 0
adb shell settings put global transition_animation_scale 0
adb shell settings put global animator_duration_scale 0

adb install -r android/app/build/outputs/apk/debug/app-debug.apk
adb logcat -c
adb shell am start -W -n com.brendigo.web2apk.client/.MainActivity
wait_for_label "Aktivacija"
check_process "com.brendigo.web2apk.client"
capture "client-default"

# Simulate compact phone and wider tablet-density profiles, then verify the
# actual Android Activity and activation UI survive configuration changes.
for spec in "720x1280 240 phone-compact" "1600x2560 320 tablet-wide"; do
  read -r dimensions density suffix <<<"$spec"
  adb shell wm size "$dimensions"
  adb shell wm density "$density"
  adb shell am force-stop com.brendigo.web2apk.client
  adb shell am start -W -n com.brendigo.web2apk.client/.MainActivity
  wait_for_label "Aktivacija"
  check_process "com.brendigo.web2apk.client"
  capture "client-$suffix"
done
check_crashes "com.brendigo.web2apk.client"

# A second, separately compiled Android app must render real bundled HTML
# without remote web access, including a project name containing an apostrophe.
adb shell wm size 1080x1920
adb shell wm density 420
adb install -r emulator-fixture/project/app/build/outputs/apk/debug/app-debug.apk
adb logcat -c
adb shell am start -W -n com.brendigo.offlinefixture/.MainActivity
wait_for_label "BRENDIGO_OFFLINE_EMULATOR_READY"
check_process "com.brendigo.offlinefixture"
capture "generated-offline-webview"
check_crashes "com.brendigo.offlinefixture"

echo "PASS: Android client activation on 3 resolutions and generated offline WebView."
