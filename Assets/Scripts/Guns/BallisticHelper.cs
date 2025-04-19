using UnityEngine;

namespace Guns
{
    public static class BallisticHelper
    {
        public static Vector3? CalculateBallisticVelocity(Vector3 origin, Vector3 target, float projectileSpeed)
        {
            Vector3 toTarget = target - origin;
            Vector3 toTargetXZ = new Vector3(toTarget.x, 0, toTarget.z);

            float y = toTarget.y;
            float xz = toTargetXZ.magnitude;
            float gravity = Mathf.Abs(Physics.gravity.y);

            float speedSquared = projectileSpeed * projectileSpeed;
            float underSqrt = speedSquared * speedSquared - gravity * (gravity * xz * xz + 2 * y * speedSquared);

            if (underSqrt < 0)
                return null;

            float sqrt = Mathf.Sqrt(underSqrt);
            float lowAngle = Mathf.Atan2(speedSquared - sqrt, gravity * xz);

            Vector3 velocity = toTargetXZ.normalized * projectileSpeed * Mathf.Cos(lowAngle);
            velocity.y = projectileSpeed * Mathf.Sin(lowAngle);

            return velocity;
        }
        
        /// <summary>
        /// Prédit la position future du joueur pour viser une cible en mouvement.
        /// </summary>
        public static Vector3 PredictFuturePosition(
            Vector3 shooterPos,
            Vector3 targetPos,
            Vector3 targetVelocity,
            float projectileSpeed)
        {
            Vector3 displacement = targetPos - shooterPos;
            float targetMoveAngle = Vector3.Angle(-displacement, targetVelocity) * Mathf.Deg2Rad;

            // Résolution analytique de l'équation du tir : t = temps d'impact estimé
            float velocityMagnitude = targetVelocity.magnitude;
            float a = projectileSpeed * projectileSpeed - velocityMagnitude * velocityMagnitude;
            float b = 2 * Vector3.Dot(displacement, targetVelocity);
            float c = -displacement.sqrMagnitude;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return targetPos; // Impossible de toucher, vise la position actuelle

            float sqrtDisc = Mathf.Sqrt(discriminant);
            float t1 = (-b + sqrtDisc) / (2 * a);
            float t2 = (-b - sqrtDisc) / (2 * a);

            float t = Mathf.Max(t1, t2);
            if (t < 0)
                t = Mathf.Min(t1, t2);

            if (t > 0)
                return targetPos + targetVelocity * t;
            else
                return targetPos;
        }

        public static Vector3? CalculateBallisticDirection(Vector3 origin, Vector3 target, float projectileSpeed)
        {
            var velocity = CalculateBallisticVelocity(origin, target, projectileSpeed);
            if (velocity.HasValue)
                return velocity.Value.normalized;
            return null;
        }
    }
}