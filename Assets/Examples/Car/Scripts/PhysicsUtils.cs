using UnityEngine;

namespace NetickLeague
{
    public static class PhysicsUtils
    {
        static readonly RaycastHit[] _hits = new RaycastHit[64];

        public static bool Raycast(Ray ray, out RaycastHit hit, float maxDistance, Collider except)
        {
            var count = Physics.RaycastNonAlloc(ray, _hits, maxDistance);

            RaycastHit? closestHit = null;

            for (var i = 0; i < count; i++)
            {
                if (_hits[i].collider == except)
                    continue;

                if (_hits[i].distance < (closestHit?.distance ?? float.MaxValue))
                    closestHit = _hits[i];
            }

            if (closestHit.HasValue)
            {
                hit = closestHit.Value;
                return true;
            }

            hit = default;
            return false;
        }
    }
}
