using UnityEngine;

namespace Not3A.Stage1
{
    public sealed class FixedRoute : MonoBehaviour
    {
        [SerializeField] private Vector2[] points;

        public Vector2[] Points => points;

        public void Configure(Vector2[] routePoints)
        {
            points = routePoints;
        }

        private void OnValidate()
        {
            if (points == null)
            {
                points = System.Array.Empty<Vector2>();
            }
        }
    }
}
