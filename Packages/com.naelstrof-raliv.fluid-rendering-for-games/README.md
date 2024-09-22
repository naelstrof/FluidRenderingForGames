Fluid Emitter Requirements:
1. UV2 unwrapped mesh. Can use unity's *Generate Lightmaps* import option.
2. Renderer needs to have material + shader that includes a _DecalColorMap texture input. Impossible to automate.
Needs are too varied.
3. Colliders on the same LayerMask as specified on the FluidEmitter
Can't be automated, user needs to set up according to their needs. Very least default to "Everything".
4. Colliders have a DecalableCollider component, with Renders associated with it in the list.
Possibly automatable, theoretically if a collider is "hit" that doesn't have a decalable collider, could activate a fallback.
5. FluidRendererFeature needs to be added on the current render pipeline