namespace Not3A.Stage3.VisualPilot
{
    /// <summary>Progress belongs to this world; input and rendering do not own unlocks.</summary>
    public sealed class Stage3TravelState
    {
        public readonly struct Snapshot
        {
            internal readonly bool Built, Studied, Discovered;
            internal Snapshot(Stage3TravelState state)
            { Built = state.WorkshopBuilt; Studied = state.Researched; Discovered = state.RemoteDiscovered; }
        }
        public Snapshot CaptureSnapshot() => new Snapshot(this);
        public void RestoreSnapshot(Snapshot snapshot)
        { WorkshopBuilt = snapshot.Built; Researched = snapshot.Studied; RemoteDiscovered = snapshot.Discovered; }
        public bool WorkshopBuilt { get; private set; }
        public bool Researched { get; private set; }
        public bool RemoteDiscovered { get; private set; }
        public bool CanTravel => Researched && RemoteDiscovered;
        public bool BuildWorkshop()
        {
            if (WorkshopBuilt) return false;
            WorkshopBuilt = true;
            return true;
        }
        public bool Study()
        {
            if (!WorkshopBuilt || Researched) return false;
            Researched = true;
            return true;
        }
        public void Discover() => RemoteDiscovered = true;
        public void Reset() { WorkshopBuilt = false; Researched = false; RemoteDiscovered = false; }
    }
}
