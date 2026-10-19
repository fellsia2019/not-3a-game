#include "../Not3ATrackpad.mm"
#include <cassert>
#include <cstdio>

static void Near(float actual, float expected)
{
    assert(std::abs(actual - expected) < 0.0001f);
}

int main()
{
    @autoreleasepool
    {
        [NSApplication sharedApplication];
        assert(Not3ATrackpad_Initialize() == 1);
        id first = eventMonitor;
        assert(Not3ATrackpad_Initialize() == 1 && eventMonitor == first);
        Near(Not3ATrackpad_ReadScale(), 1.0f);
        Not3ATrackpad_SetEnabled(1);
        AccumulateMagnification(0.25, NSEventPhaseBegan);
        AccumulateMagnification(0.25, NSEventPhaseChanged);
        Near(Not3ATrackpad_ReadScale(), 1.5f);
        Near(Not3ATrackpad_ReadScale(), 1.0f); // Reading twice must not repeat input.
        AccumulateMagnification(0.5, NSEventPhaseChanged);
        Near(Not3ATrackpad_ReadScale(), 2.0f / 1.5f); // Same overall scale, independent of polling.
        AccumulateMagnification(-0.5, NSEventPhaseChanged);
        Near(Not3ATrackpad_ReadScale(), 0.75f);
        AccumulateMagnification(0.2, NSEventPhaseChanged);
        Not3ATrackpad_SetEnabled(0); // Losing Game-view focus discards pending input.
        AccumulateMagnification(0.5, NSEventPhaseChanged);
        Near(Not3ATrackpad_ReadScale(), 1.0f);
        Not3ATrackpad_SetEnabled(1);
        AccumulateMagnification(0.2, NSEventPhaseBegan);
        AccumulateMagnification(0, NSEventPhaseCancelled);
        Near(Not3ATrackpad_ReadScale(), 1.0f);
        AccumulateMagnification(NAN, NSEventPhaseBegan);
        AccumulateMagnification(INFINITY, NSEventPhaseChanged);
        Near(Not3ATrackpad_ReadScale(), 1.0f);
        AccumulateMagnification(-1000, NSEventPhaseChanged);
        assert(std::isfinite(Not3ATrackpad_ReadScale()));
        Not3ATrackpad_Shutdown();
        assert(eventMonitor == nil);
        Not3ATrackpad_Shutdown(); // Repeated teardown/domain reload must be safe.
        assert(Not3ATrackpad_Initialize() == 1);
        Near(Not3ATrackpad_ReadScale(), 1.0f);
        Not3ATrackpad_SetEnabled(1);
        Not3ATrackpad_SetPanEnabled(1);
        assert(AccumulateScroll(4, -3, true, 2));
        assert(AccumulateScroll(1, -2, true, 2));
        float panX, panY;
        Not3ATrackpad_ReadPan(&panX, &panY);
        Near(panX, 10); Near(panY, 10); // Backing pixels and top-left GUI coordinates.
        Near(Not3ATrackpad_ReadScale(), 1); // A two-finger scroll never magnifies.
        Not3ATrackpad_ReadPan(&panX, &panY);
        Near(panX, 0); Near(panY, 0); // Poll once, never replay.
        assert(Not3ATrackpad_LastScrollWasPrecise() == 1);
        assert(!AccumulateScroll(0, 1, false, 2)); // Physical wheel passes through to Unity.
        assert(Not3ATrackpad_LastScrollWasPrecise() == 0);
        assert(AccumulateScroll(NAN, INFINITY, true, 2));
        Not3ATrackpad_ReadPan(&panX, &panY); Near(panX, 0); Near(panY, 0);
        AccumulateMagnification(.25, NSEventPhaseBegan);
        AccumulateScroll(10, 20, true, 2);
        Not3ATrackpad_SetPanEnabled(0); // Map closes: both pending gestures are discarded.
        Near(Not3ATrackpad_ReadScale(), 1);
        Not3ATrackpad_ReadPan(&panX, &panY); Near(panX, 0); Near(panY, 0);
        assert(!AccumulateScroll(10, 20, true, 2)); // Ordinary world scroll stays with Unity.
        Not3ATrackpad_Shutdown();
        std::puts("PASS: AppKit lifecycle, pinch ratios, precise pan/wheel classification, backing scale, ownership reset and invalid input.");
    }
}
