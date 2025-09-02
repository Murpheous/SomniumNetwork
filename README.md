# SomniumNetwork

Interim Somnium Networked UI  

This is a snapshot of the network synchronisation UI objects that I am exploring to find the best way to sync UI and states in Somnium.

The goal is to develop a set of objects that can later be adapted for full network synchronisation, which will be implemented in the future.

The objects in this repository utilise the synced rigid body functionality of Photon Fusion 2, which is already incorporated in Somnium projects.

## Rationale

The core rationale is to provide controls that can synchronise UI states and settings across the Somnium network by utilising a minimal and common mechanism already built into the Somnium platform, as showcased by Denevraut's Community Networking asset in the Somnium Community Hub. The project here includes one code module, adapted from Denevraut's asset, SceneNetworking.cs, which also utilises a debug display module, DebugUI.cs, from xCommando's Community Asset of the same name.

Since the current Somnium SDK network bridge partly exposes the functionality of Photon Fusion by allowing the use of network-synced transforms and rigid bodies, the examples here function by embedding a synced rigid body into each UI control. The rigid body is stripped back to a basic kinematic Rigidbody without a collider, where the control setting encodes the rotation of the Rigidbody's transform.

The core model is that the UI element controls the rotation of the Rigid body transform; Fusion syncs the rotation across the network, updating the rigid body transform accordingly.  

From there, the code in each Synced Control polls the transform—hasChanged flag to invoke the control state events to subscribed components.

## Arbitration

There is currently minimal arbitration; the Network Authority (roughly equivalent to Ownership in other platforms) of each control is requested when the UI pointer enters the control, hence the last user to hover a pointer gets authority - the early bird may get the worm, but the second mouse gets the cheese.

## Dependence on Photon Fusion2

To make use of network functionality in live projects, you need the Photon Fusion 2.0.5 package installed in your Unity project. However, to allow testing without Fusion2 installed, the code in this repository includes conditional compilation to enable local execution.

