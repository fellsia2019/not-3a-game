using System;
using UnityEngine;

namespace Not3A.Stage2.Map
{
    public enum ResourceProductionBuilding { Sawmill, Quarry, IronMine }

    /// <summary>A locked typed attachment reservation consumed by the optional production system.</summary>
    public readonly struct ResourceProductionPoint
    {
        public ResourceProductionPoint(int districtIndex, MapResourceKind resource,
            Vector2 position, Vector2 clearanceSize, float facingDegrees)
        {
            if (clearanceSize.x <= 0 || clearanceSize.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(clearanceSize));
            DistrictIndex = districtIndex;
            Resource = resource;
            Building = resource switch
            {
                MapResourceKind.Wood => ResourceProductionBuilding.Sawmill,
                MapResourceKind.Stone => ResourceProductionBuilding.Quarry,
                MapResourceKind.Metal => ResourceProductionBuilding.IronMine,
                _ => throw new ArgumentOutOfRangeException(nameof(resource))
            };
            Position = position;
            Clearance = new Rect(position - clearanceSize * .5f, clearanceSize);
            FacingDegrees = facingDegrees;
        }

        public int DistrictIndex { get; }
        public MapResourceKind Resource { get; }
        public ResourceProductionBuilding Building { get; }
        public Vector2 Position { get; }
        public Rect Clearance { get; }
        public float FacingDegrees { get; }
        public bool IsLocked => true;
        public bool Accepts(ResourceProductionBuilding building) => building == Building;
    }
}
