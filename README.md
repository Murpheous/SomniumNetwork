# SomniumNetwork

Interim Somnium Networked UI  

This is a snapshot of the network synchronisation UI objects that I am exploring to find the best way to sync UI and states in Somnium.

The goal is to develop a set of objects that can later be adapted for full network synchronisation, which will be implemented in the future.

The objects in this repository utilise the synced rigid body functionality of Photon Fusion 2, which is already incorporated in Somnium projects.

## Rationale

The core rationale is to provide some controls that can synchronise UI states and settings across the Somnium network by utilising a minimal and common simple mechnanism that is already built-in to the Somnium platform and is showcased by Denevraut's Community Networking asset in the Somnium Community Hub. The project here includes one code modules adapted from Denevraut's asset: SceneNetworking.cs which also utilises a debug display module DebugUI.cs from xCommando's Community Asset of the same name.

Since the current Somnium SDK newtork bridge partly exposes the functionality of Photon Fusion by allowing the use of network-synced transforms and rigidbodies, the examples here function by embedding a synced rigid body into each UI control. The rigid nody is stripped-back to be a basic kinematic rigid body without a collider, and the with the control setting the rotation of the body's transform.

The core model is that the UI element controls the rotation of the Rigid body transform, Fusion syncs the rotation across the network, updating the rigid body transform.  

From there, the code in each Synced Control polls the transform.hasChanged flag to invoke the control state events to subscribed components.  

## Dependence on Photon Fusion2

To make use of network functionality in live projects you need Photon Fusion 2.0.5 package installed in your Unity project. However, to allow testing without Fusion2 installed, the code in this repository contains conditional compilation to allow it to run locally.

