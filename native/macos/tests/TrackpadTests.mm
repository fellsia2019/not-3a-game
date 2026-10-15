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
        Not3ATrackpad_Shutdown();
        std::puts("PASS: AppKit monitor lifecycle, gesture ratios, focus/cancel reset and invalid input.");
    }
}
