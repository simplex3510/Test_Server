using Fusion;
using Fusion.XR.Host.Rig;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Grabbing
{
    // Store the info describbing a grabbing state
    [System.Serializable]
    public struct GrabInfo : INetworkStruct
    {
        public NetworkBehaviourId grabbedObjectId;
        public Vector3 localPositionOffset;
        public Quaternion localRotationOffset;
        // We want the local user accurate ungrab position to be enforced on the network, and so shared in the input (to avoid the grabbable following "too long" the grabber)
        public Vector3 ungrabPosition;
        public Quaternion ungrabRotation; 
        public Vector3 ungrabVelocity;
        public Vector3 ungrabAngularVelocity;
    }

    /**
     * 
     * Allows a NetworkHand to grab NetworkGrabbable objects
     * 
     **/

    [RequireComponent(typeof(NetworkHand))]
    [DefaultExecutionOrder(NetworkGrabber.EXECUTION_ORDER)]
    public class NetworkGrabber : NetworkBehaviour
    {
        public const int EXECUTION_ORDER = NetworkHand.EXECUTION_ORDER + 10;
        [Networked]
        public GrabInfo GrabInfo { get; set; }

        public enum GrabbingKind
        {
            KinematicOnly,
            PhysicsAndKinematic
        }
        public GrabbingKind supportedgrabbingKind = GrabbingKind.PhysicsAndKinematic;

        NetworkGrabbable grabbedObject;
        public NetworkTransform networkTransform;
        public NetworkHand hand;
        ChangeDetector changeDetector;

        private void Awake()
        {
            networkTransform = GetComponent<NetworkTransform>();
            hand = GetComponentInParent<NetworkHand>();
        }

        public override void Spawned()
        {
            base.Spawned();
            if (hand.IsLocalNetworkRig)
            {
                hand.LocalHardwareHand.grabber.networkGrabber = this;
            }
            changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
            HandleGrabInfoChange(GrabInfo);
            if (supportedgrabbingKind == GrabbingKind.PhysicsAndKinematic)
            {
                // The hands need to be simulated to be at the appropriate position during FUN when a grabbable follow them (physics grabbable are fully simulated)
                Runner.SetIsSimulated(Object, true);
                if (Object.HasInputAuthority == false)
                {
                    // As the object is now simulated, the render time frame will take the localy simulated ticks by default.
                    // But we don't really have data to guess in advance the position of the hand for remote users
                    // So we still want to interpolate between the state we received from the server (the remote time frame)
                    Object.RenderTimeframe = RenderTimeframe.Remote;
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (Runner.IsForward)
            {
                // We only detect grabbing changes in forward, to avoid multiple Grab calls (that would have side effects in current implementation)
                foreach (var changedPropertyName in changeDetector.DetectChanges(this))
                {
                    if (changedPropertyName == nameof(GrabInfo))
                    {
                        // Grab info is filled by the NetworkRig, based on the input, and the input are filled with the Hardware rig Grabber GrabInfo
                        HandleGrabInfoChange(GrabInfo);
                    }
                }
            }
        }

        void HandleGrabInfoChange(GrabInfo newGrabInfo)
        {
            if (grabbedObject != null)
            {
                grabbedObject.Ungrab(this, newGrabInfo);
                grabbedObject = null;
            }

            // We have to look for the grabbed object has it has changed
            // If an object is grabbed, we look for it through the runner with its Id
            if (newGrabInfo.grabbedObjectId != NetworkBehaviourId.None && Object.Runner.TryFindBehaviour(newGrabInfo.grabbedObjectId, out NetworkGrabbable newGrabbedObject))
            {
                grabbedObject = newGrabbedObject;

                if (grabbedObject != null)
                {
                    grabbedObject.Grab(this, newGrabInfo);
                }
            }
        }
    }
}
