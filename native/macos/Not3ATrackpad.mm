#import <AppKit/AppKit.h>
#include <algorithm>
#include <cmath>

// App-local magnify events only. Unity enables capture only over its focused Game view.
static id eventMonitor = nil;
static bool captureEnabled = false;
static double gestureScale = 1.0;
static double pendingLogScale = 0.0;

static void ResetGesture()
{
    gestureScale = 1.0;
    pendingLogScale = 0.0;
}

static void AccumulateMagnification(double delta, NSEventPhase phase)
{
    if (!captureEnabled) return;
    if (phase & NSEventPhaseCancelled) { ResetGesture(); return; }
    if (phase & NSEventPhaseBegan) gestureScale = 1.0;
    if (std::isfinite(delta))
    {
        // AppKit supplies an additive change to the current gesture's scale.
        // Queue ratios so polling frequency cannot change the final magnification.
        double next = std::clamp(gestureScale + delta, 0.01, 100.0);
        pendingLogScale += std::log(next / gestureScale);
        gestureScale = next;
    }
    if (phase & NSEventPhaseEnded) gestureScale = 1.0;
}

#define TRACKPAD_EXPORT extern "C" __attribute__((visibility("default")))

TRACKPAD_EXPORT int Not3ATrackpad_Initialize()
{
    captureEnabled = false;
    ResetGesture();
    if (eventMonitor == nil)
        eventMonitor = [NSEvent addLocalMonitorForEventsMatchingMask:NSEventMaskMagnify
            handler:^NSEvent *(NSEvent *event) {
                if (!captureEnabled || ![NSApp isActive]) return event;
                AccumulateMagnification(event.magnification, event.phase);
                return nil; // The Game camera owns this gesture, not the Editor's view scale.
            }];
    return eventMonitor != nil ? 1 : 0;
}

TRACKPAD_EXPORT void Not3ATrackpad_SetEnabled(int enabled)
{
    captureEnabled = enabled != 0;
    if (!captureEnabled) ResetGesture();
}

TRACKPAD_EXPORT float Not3ATrackpad_ReadScale()
{
    double scale = std::exp(std::clamp(pendingLogScale, -16.0, 16.0));
    pendingLogScale = 0.0;
    return static_cast<float>(scale);
}

TRACKPAD_EXPORT void Not3ATrackpad_Shutdown()
{
    captureEnabled = false;
    ResetGesture();
    if (eventMonitor != nil) [NSEvent removeMonitor:eventMonitor];
    eventMonitor = nil;
}
