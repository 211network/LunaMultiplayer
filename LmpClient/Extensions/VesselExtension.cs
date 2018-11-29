﻿using LmpClient.Events;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using UnityEngine;

namespace LmpClient.Extensions
{
    public static class VesselExtension
    {
        /// <summary>
        /// Advance the orbit epoch to the specified time sent as parameter
        /// </summary>
        public static void AdvanceShipPosition(this Vessel vessel, double time)
        {
            if (vessel.situation <= Vessel.Situations.PRELAUNCH) return;

            var obtPos = vessel.orbit.getRelativePositionAtUT(time);
            var obtVel = vessel.orbit.getOrbitalVelocityAtUT(time);

            if (!vessel.packed)
                vessel.GoOnRails();

            vessel.orbit.UpdateFromStateVectors(obtPos, obtVel, vessel.mainBody, time);
            vessel.orbitDriver.updateFromParameters();

            FloatingOrigin.SetOffset(vessel.transform.position);
            OrbitPhysicsManager.CheckReferenceFrame();
            OrbitPhysicsManager.HoldVesselUnpack(10);
            vessel.IgnoreGForces(20);
        }

        /// <summary>
        /// Get vessel cost
        /// </summary>
        public static float GetShipCosts(this Vessel vessel, out float dryCost, out float fuelCost)
        {
            dryCost = 0f;
            fuelCost = 0f;

            foreach (var part in vessel.parts)
            {
                var partPrice = part.partInfo.cost + part.GetModuleCosts(part.partInfo.cost);
                var partResourcePrice = 0f;
                foreach (var resource in part.Resources)
                {
                    partPrice -= resource.info.unitCost * (float)resource.maxAmount;
                    partResourcePrice += resource.info.unitCost * (float)resource.amount;
                }
                
                dryCost += partPrice;
                fuelCost += partResourcePrice;
            }

            return dryCost + fuelCost;
        }

        /// <summary>
        /// Freeze a vessel position
        /// </summary>
        public static void FreezePosition(this Vessel vessel)
        {
            if (vessel != null && !vessel.packed && vessel.parts.Count > 0)
            {
                if (vessel.rootPart && vessel.rootPart.Rigidbody && vessel.rootPart.Rigidbody.constraints == RigidbodyConstraints.None)
                {
                    vessel.parts?.ForEach(p => p.Rigidbody.constraints = RigidbodyConstraints.FreezeAll);
                }
            }
        }

        /// <summary>
        /// Freeze a vessel position
        /// </summary>
        public static void UnfreezePosition(this Vessel vessel)
        {
            if (vessel != null && !vessel.packed && vessel.parts.Count > 0)
            {
                if (vessel.rootPart && vessel.rootPart.Rigidbody && vessel.rootPart.Rigidbody.constraints == RigidbodyConstraints.FreezeAll)
                {
                    vessel.parts?.ForEach(p => p.Rigidbody.constraints = RigidbodyConstraints.None);
                }
            }
        }


        /// <summary>
        /// Freeze a vessel position
        /// </summary>
        public static Part FindPart(this Vessel vessel, uint partFlightId)
        {
            if (vessel != null && !vessel.packed && vessel.parts.Count > 0)
            {
                for (var i = 0; i < vessel.parts.Count; i++)
                {
                    if (vessel.parts[i].flightID == partFlightId)
                        return vessel.parts[i];
                }
            }

            return null;
        }

        public static void RemoveAllCrew(this Vessel vessel)
        {
            foreach (var part in vessel.Parts)
            {
                foreach (var crew in part.protoModuleCrew)
                {
                    vessel.RemoveCrew(crew);
                }
            }

            foreach (var crew in vessel.GetVesselCrew())
            {
                vessel.RemoveCrew(crew);
            }

            vessel.DespawnCrew();
        }

        public static bool IsImmortal(this Vessel vessel)
        {
            if (vessel.rootPart)
                return float.IsPositiveInfinity(vessel.rootPart.crashTolerance);

            if (!LockSystem.LockQuery.UpdateLockExists(vessel.id)) return false;

            return !LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName);
        }

        /// <summary>
        /// Set all vessel parts to unbreakable or not (makes the vessel immortal or not)
        /// </summary>
        public static void SetImmortal(this Vessel vessel, bool immortal)
        {
            if (vessel == null) return;

            if (vessel.rootPart)
            {
                var isCurrentlyImmortal = float.IsPositiveInfinity(vessel.rootPart.crashTolerance);
                if (isCurrentlyImmortal == immortal) return;
            }
            
            var buoyancy = vessel.GetComponent<PartBuoyancy>();
            if (buoyancy)
            {
                buoyancy.enabled = !immortal;
            }

            var collisionEnhancer = vessel.GetComponent<CollisionEnhancer>();
            if (collisionEnhancer)
            {
                collisionEnhancer.enabled = !immortal;
            }

            var flightIntegrator = vessel.GetComponent<FlightIntegrator>();
            if (flightIntegrator)
            {
                flightIntegrator.enabled = !immortal;
            }

            if (vessel.loaded)
            {
                LunaLog.Log($"Making vessel {vessel.id} {(immortal ? "immortal" : "mortal")}");
                foreach (var part in vessel.Parts)
                {
                    part.SetImmortal(immortal);
                }
            }

            if (immortal)
            {
                ImmortalEvent.onVesselImmortal.Fire(vessel);
            }
            else
            {
                ImmortalEvent.onVesselMortal.Fire(vessel);
            }
        }
    }
}
