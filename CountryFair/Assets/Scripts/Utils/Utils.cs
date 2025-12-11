using UnityEngine;
using System;

/// <summary>
/// Utility class providing helper methods for common game mechanics and calculations.
/// This static class contains reusable functionality for ray casting, random number generation,
/// and other utility operations used throughout the Country Fair VR game.
/// </summary>
public static class Utils
{
    /// <summary>  
    /// Casts a ray from the user's eyes/head position in the Meta Quest 3 headset to the game world.
    /// This method is used for gaze-based interactions and raycasting in VR applications,
    /// allowing objects to be selected or interacted with based on where the user is looking.
    /// </summary>
    /// <returns>
    /// A Ray object representing the ray cast from the user's eye position (center eye anchor)
    /// in the direction they are looking (forward vector of the head).
    /// Returns an empty ray if the OVRCameraRig cannot be found in the scene.
    /// </returns>
    /// <remarks>
    /// This method depends on having an OVRCameraRig component in the scene tagged with "MainCamera".
    /// The OVRCameraRig provides the centerEyeAnchor transform which represents the user's eye position.
    /// The ray direction is the forward direction of the head, representing the direction the user is gazing.
    /// 
    /// Typical usage:
    /// <code>
    /// Ray gazRay = Utils.CastRayMetaQuest();
    /// if (Physics.Raycast(gazRay, out RaycastHit hit))
    /// {
    ///     Debug.Log("Looking at: " + hit.collider.gameObject.name);
    /// }
    /// </code>
    /// </remarks>
    /// <exception cref="NullReferenceException">
    /// Logs an error if OVRCameraRig is not found or if centerEyeAnchor is not accessible.
    /// </exception>
    public static Ray CastRayMetaQuest()
    {
        GameObject ovrCameraRig = GameObject.FindGameObjectWithTag("MainCamera");

        if (ovrCameraRig == null)
        {
            Debug.LogError("OVRCameraRig not found in the scene. Make sure you have an OVRCameraRig in your scene to use CastRayMetaQuest.");
            return new Ray();
        }
 
        Transform centerEyeTransform = ovrCameraRig.GetComponent<OVRCameraRig>().centerEyeAnchor;

        return new Ray(centerEyeTransform.position, centerEyeTransform.forward);
    }

    /// <summary>
    /// Generates a random float value within a specified range using a time-based seed.
    /// This method provides pseudo-random number generation that avoids predictable patterns
    /// by seeding the random number generator with the current system time in milliseconds.
    /// </summary>
    /// <remarks>
    /// Why use this instead of UnityEngine.Random.Range or System.Random.Next?
    /// 
    /// Computers cannot generate truly random numbers; they generate pseudo-random numbers using a seed.
    /// The default Unity random number generator may produce predictable patterns if not properly seeded.
    /// This method seeds the random number generator with Environment.TickCount (current OS time in milliseconds)
    /// on each call, which helps avoid such patterns in the generated random numbers.
    /// 
    /// WARNING: Seeding on every call has performance implications. For performance-critical code
    /// that requires many random numbers, consider calling this method less frequently or using
    /// the standard UnityEngine.Random.Range() directly if predictable patterns are acceptable.
    /// 
    /// Typical usage:
    /// <code>
    /// float randomHealth = Utils.RandomValueInRange(10f, 100f);
    /// float randomSpeed = Utils.RandomValueInRange(5f, 15f);
    /// </code>
    /// </remarks>
    /// <param name="min">The minimum float value of the range (inclusive).</param>
    /// <param name="max">The maximum float value of the range (inclusive).</param>
    /// <returns>
    /// A random float value between min and max (inclusive).
    /// If min >= max, the behavior is undefined (as per UnityEngine.Random.Range documentation).
    /// </returns>
    public static float RandomValueInRange(float min, float max)
    {   
        int seed = Environment.TickCount;

        UnityEngine.Random.InitState(seed);

        return UnityEngine.Random.Range(min, max);
    }


     /// <summary>
    /// The GetChildren method is responsible for retrieving the children of a game object.
    /// </summary>
    /// <param name="parent">The transform component of the game object whose children are to be retrieved.</param>
    /// <returns>The children of the specified game object.</returns>
    public static GameObject[] GetChildren(Transform parent)
    {
        GameObject[] children = new GameObject[parent.childCount];

        for (int i = 0; i < parent.childCount; i++)
        {
            children[i] = parent.GetChild(i).gameObject;
        }

        return children;
    }
}
