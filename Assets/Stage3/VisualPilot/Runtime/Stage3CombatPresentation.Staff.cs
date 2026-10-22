using Not3A.Stage2.Integration;
using UnityEngine;

namespace Not3A.Stage3.VisualPilot
{
    public sealed partial class Stage3CombatPresentation
    {
        private static readonly Color StaffBoltColor = new Color(.73f, .40f, 1f);
        public int ActiveStaffFlights
        {
            get { var count = 0; foreach (var flight in flights.Values) if (flight.Staff) count++; return count; }
        }
        public int ActiveGuildStaffFlights
        {
            get { var count = 0; foreach (var flight in guildFlights) if (flight.Staff) count++; return count; }
        }
        private bool IsStaffHeroShot(Vector2 origin)
        {
            if (heroAvatar == null || Vector2.Distance(origin, heroAvatar.transform.position) >= .01f) return false;
            var equipment = composition.GetComponent<Stage2EquipmentDriver>()?.Rules;
            return equipment?.ActiveWeaponType == Stage2ItemType.Wand && equipment.ActiveWeapon != null;
        }
        private Transform StaffBolt(string label)
        {
            // Reuse the approved local faceted spark geometry: a compact violet bolt with a pale core,
            // not a bow shaft/feathers or a flying UI medallion. The existing flight observer moves it.
            var body = MeshObject(label, stoneMesh, StaffBoltColor);
            body.localScale = new Vector3(.18f, .18f, .68f);
            var core = MeshObject("Arcane bolt core", stoneMesh, new Color(.96f, .87f, 1f));
            core.SetParent(body, false); core.localScale = new Vector3(.45f, .45f, 1.12f);
            return body;
        }
    }
}
