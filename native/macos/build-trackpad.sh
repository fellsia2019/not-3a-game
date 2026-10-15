#!/bin/bash
set -euo pipefail
task_root="$(cd "$(dirname "$0")/../.." && pwd)"
task_plugin="$task_root/Assets/Plugins/macOS/libNot3ATrackpad.dylib"
mkdir -p "$(dirname "$task_plugin")"
xcrun clang++ -std=c++17 -fobjc-arc -fblocks -fvisibility=hidden \
  -dynamiclib -arch arm64 -arch x86_64 -mmacosx-version-min=11.0 \
  -framework AppKit -install_name @rpath/libNot3ATrackpad.dylib \
  "$task_root/native/macos/Not3ATrackpad.mm" -o "$task_plugin"
codesign --force --sign - "$task_plugin"
lipo -info "$task_plugin"
