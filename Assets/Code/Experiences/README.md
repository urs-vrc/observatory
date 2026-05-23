# Experiences

Observatory includes multiple experiences in a single world, this folder is intended to host
the observatory's exclusive experiences.

## Authoring Experiences

To make an experience, simply make a folder here, and make your necessary system in isolation here.
All experiences should not rely on each other, and must be standalone.

Observatory experiences also have strict performance requirements to ensure it runs on a broad spectrum
of devices, so that means you will have to learn to optimize aggressively if you want your experience to be 
featured in the world.

To be guided a bit on some basic strategies on what you can do to keep your experiences optimized, here's a few tips:


### Prefer Stingray-Style Entity Management over Deep Inheritance

You are building for a world designed to host 40 to 80 players simultaneously. At this scale, standard object-oriented Unity patterns will quickly tank the frame rate.

To prevent this, we adopt a **Stingray-style architecture** (inspired by the data-oriented design of the Bitsquid/Autodesk Stingray engine). In this paradigm, we decouple data from logic entirely to bypass the heavy execution overhead of individual `UdonBehaviour` components.

#### The Core Philosophy
Instead of treating an interactive object as a "smart" GameObject with its own logic script, think of it the way Stingray did:
1. **Entities are just IDs:** An object is represented simply as an index (an integer) in an array.
2. **Components are flat data:** State (positions, health, active status) is stored in flat arrays managed by a controller.
3. **Systems handle the logic:** A single, monolithic Master Manager script loops through those data arrays and updates everything in a single, high-speed pass.

#### Example

Supposedly you're making a racing game that needs to track 32+ players with active and passive skills, stamina and speed values, you'd normally write it like this:

```cs
// RacerController.cs (Attached to 32+ individual racer objects)
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class RacerController : UdonSharpBehaviour
{
    public VRCPlayerApi owner;
    public float stamina = 100f;
    public float currentSpeed = 5f;
    public bool isSkillActive;

    // Now you have 32+ update loops going on, yikes...
    void Update()
    {
        if (owner == null) return;

        // Drain stamina if sprinting
        if (isSkillActive)
        {
            stamina -= 10f * Time.deltaTime;
            currentSpeed = 10f;
        }
        else
        {
            stamina = Mathf.Min(100f, stamina + 2f * Time.deltaTime);
            currentSpeed = 5f;
        }
    }
}
```

Instead, use a single manager class that tracks all of these players simultaneously using parallel arrays. This allows tracking players much more efficiently as you only need to handle one update loop for all 32 players.

```csharp
// RaceTrackManager.cs (Placed on ONE GameObject in the scene)
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class RaceTrackManager : UdonSharpBehaviour
{
    // The "Components" are just flat data arrays mapped by player ID (array index)
    private VRCPlayerApi[] racers;
    private float[] racerStaminas;
    private float[] racerSpeeds;
    private bool[] racerSkillStates;
    private int activeRacerCount = 0;

    void Start()
    {
        racers = new VRCPlayerApi[32];
        racerStaminas = new float[32];
        racerSpeeds = new float[32];
        racerSkillStates = new bool[32];
    }

    // One single Update pass handles all 32+ players sequentially in memory.
    void Update()
    {
        float deltaTime = Time.deltaTime;
        int totalRacers = activeRacerCount;

        for (int i = 0; i < totalRacers; i++)
        {
            VRCPlayerApi player = racers[i];
            if (player == null || !player.IsValid()) continue;
            
            if (racerSkillStates[i])
            {
                racerStaminas[i] -= 10f * deltaTime;
                racerSpeeds[i] = 10f;
            }
            else
            {
                racerStaminas[i] = Mathf.Min(100f, racerStaminas[i] + 2f * deltaTime);
                racerSpeeds[i] = 5f;
            }
            
        }
    }
}
```

### Minimize Update() and use Event-driven logic

Avoid using `Update()` in UdonSharp scripts whenever possible. With many players and many experiences active, constant polling quickly eats the frame budget. Use events, triggers, or `SendCustomEventDelayedSeconds` to create logic that only runs when necessary.

### Batching and Draw Calls

Keep your experience's visual impact low by using GPU Instancing or Texture Sheets (Atlasses). Reducing draw calls is critical for performance. Ensure your shaders (like the provided LiaSky) are optimized for single-pass stereo rendering.

### Object Pooling for Dynamic Elements

If your experience requires spawning objects (like projectiles or particles), use a pre-allocated object pool. Instantiating and destroying objects at runtime in VRChat causes significant frame hitches and is highly discouraged.

### Use the GPU!

I get it, this is very daunting to write shaders, but moving visual logic (and sometimes even animation or simple state) to the GPU is the most effective way to keep your experience performant. Shaders run in parallel and don't contribute to the Udon execution time limit. Use the provided `LiaSky.shader` as a reference for how to handle procedural visuals and time-based effects without touching the CPU.

Avatars are plenty heavy already in VRChat, so let's not contribute to the lag!
