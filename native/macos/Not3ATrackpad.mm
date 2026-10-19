#import <AppKit/AppKit.h>
#include <algorithm>
#include <cmath>

// App-local magnify events only. Unity enables capture only over its focused Game view.
static id eventMonitor = nil;
static bool captureEnabled = false;
static bool capturePan = false;
static bool lastScrollPrecise = false;
static double gestureScale = 1.0;
static double pendingLogScale = 0.0;
static double pendingPanX = 0.0, pendingPanY = 0.0;

static void ResetGesture()
{
    gestureScale = 1.0;
    pendingLogScale = 0.0;
    pendingPanX = pendingPanY = 0.0;
}

static bool AccumulateScroll(double x, double y, bool precise, double pixelScale)
{
    lastScrollPrecise = precise;
    if (!captureEnabled || !capturePan || !precise) return false;
    if (std::isfinite(x) && std::isfinite(y) && std::isfinite(pixelScale) && pixelScale > 0)
    {
        pendingPanX += x * pixelScale;
        pendingPanY -= y * pixelScale; // AppKit +Y is up; map GUI +Y is down.
    }
    return true;
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
    capturePan = false;
    lastScrollPrecise = false;
    ResetGesture();
    if (eventMonitor == nil)
        eventMonitor = [NSEvent addLocalMonitorForEventsMatchingMask:NSEventMaskMagnify | NSEventMaskScrollWheel
            handler:^NSEvent *(NSEvent *event) {
                if (![NSApp isActive]) { ResetGesture(); return event; }
                if (event.type == NSEventTypeScrollWheel)
                {
                    double scale = event.window ? event.window.backingScaleFactor : 1.0;
                    return AccumulateScroll(event.scrollingDeltaX, event.scrollingDeltaY,
                        event.hasPreciseScrollingDeltas, scale) ? nil : event;
                }
                if (!captureEnabled) return event;
                AccumulateMagnification(event.magnification, event.phase);
                return nil; // One active Game surface owns magnification, never the Editor toolbar.
            }];
    return eventMonitor != nil ? 1 : 0;
}

TRACKPAD_EXPORT void Not3ATrackpad_SetPanEnabled(int enabled)
{
    bool next = enabled != 0;
    if (capturePan != next) ResetGesture(); // Switching world/map cannot replay another surface's input.
    capturePan = next;
}

TRACKPAD_EXPORT void Not3ATrackpad_ReadPan(float *x, float *y)
{
    *x = static_cast<float>(pendingPanX);
    *y = static_cast<float>(pendingPanY);
    pendingPanX = pendingPanY = 0.0;
}

TRACKPAD_EXPORT int Not3ATrackpad_LastScrollWasPrecise() { return lastScrollPrecise ? 1 : 0; }

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
    capturePan = false;
    lastScrollPrecise = false;
    ResetGesture();
    if (eventMonitor != nil) [NSEvent removeMonitor:eventMonitor];
    eventMonitor = nil;
}
