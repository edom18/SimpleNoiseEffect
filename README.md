# Simple Noise effect for Unity

This project is simple noise effect for Unity. This project use a compute shader and use point mesh topology to render. So this project will work on OpenGL platforms well. Of cause DirectX platform is supported but the platform has no point rendering.

This project provide an effect like below.

![Demo](./demo.gif)



## Parameters

You can set some parameters on the inspector like below.

![Inspector](./inspector.png)

### Noise Scale

Noise scale means PerlinNoise scale. If you change the value, noise will change looking.

### Rotation

Rotation means that rotate each vertex before applying curl noise. So if you change this parameter, then the vertices are moved more aggressive.

### Intensity

Intensity means that how far applying to the value to each vertex. If you change the value, then all vertices are moved more far from origin.



## How to use

Put the prefab that named "ParticleEffect" into your scene. Then, you can set some parameters for displaying the particle like below.

![Howto demo](./howto.gif)

### Animation

You can also use animation with `Progress` method in `ParticleEffect` component like below.

```C#
ParticleEffect _particleEffect;
_particleEffect.Progress(0.5f);
```

You can see the demo in `SampleScene` via `ParticleDemo` component on the `ParticleEffect` prefab.