Fluid Emitter Requirements:
---
1. UV2 unwrapped mesh. Can use Unity's *Generate Lightmaps* import option.
2. Renderer needs to have material + shader that includes a _DecalColorMap AND _FluidHeight texture input. Use included Amplify Shader Function.
3. Colliders on the same LayerMask as specified on the FluidEmitter
4. Colliders have a DecalableCollider component, with Renders associated with it in the list.
5. FluidRendererFeature needs to be added on the current render pipeline