# AnimationLoader
A plugin for loading animations from Sideloader zipmods.  
Thank you Essu for the [main bulk of the code](https://github.com/IllusionMods/AnimationLoader/commit/402c02af3bbb5a6e1b3015bd0caa3f0a7db618fc)  
See the [template](template.xml) for how to configure animations in your own mod.

## How to install
1. Install the latest build of [BepInEx](https://github.com/BepInEx/BepInEx/releases)
2. Install [KKAPI](https://github.com/IllusionMods/IllusionModdingAPI/releases/tag/v1.31.2) 1.31.2
4. Download the latest release from [the releases page](../../releases)
4. Drop the dll to `bepinex/plugins`
5. Add animation packages to the mods folder. (These can usually be downloaded with KKManager)

##
The manifest.xml was extended from version **1.0.8**.

1. **PositionHeroine** and **PositionPlayer** vectors (x, y, z) that represent:
```xml
<PositionHeroine>
  <x>0</x>
  <y>0</y>
  <z>0</z>
</PositionHeroine>
<PositionPlayer>
  <x>0</x>
  <y>0</y>
  <z>0</z>
</PositionPlayer>
```
    x is left and right movement (red axis)
    y is up and down (green axis)
    z is forward and backwards (blue axis)
The values represent a factor by which a normalized vector in the direction of the axes is 
multiplied and the result added to the axis. The magnitude of normalized vector is one. The scale
a good guess could be a meter. If there is a need to move half a unit the factor for that axis 
would be 0.5 for example.

2. **GameSpecificOverrides** - the manifest.xml is one for both KK and KKS by using this section
it can be fine tuned for both games.
```xml
<Animation>
  <StudioId>1</StudioId>
  <AnimationName>Animation 1</AnimationName>
  <NeckDonorId>0</NeckDonorId>
  <GameSpecificOverrides>
    <KoikatsuSunshine>
      <NeckDonorId>55</NeckDonorId>
    </KoikatsuSunshine>
  <GameSpecificOverrides>
</Animation>
```
Here the NeckDonorId when KK reads the configuration will be 0 but when KKS reads the configuration
will be 55.

3. **Game specific animations**
```xml
<Koikatu>
  <Animation>
    <StudioId>1</StudioId>
    <AnimationName>Animation 1</AnimationName>
    <NeckDonorId>0</NeckDonorId>
  </Animation>
</Koikatu>
<Animation>
  <StudioId>2</StudioId>
  <AnimationName>Animation 1</AnimationName>
  <NeckDonorId>0</NeckDonorId>
</Animation>

```
When KKS reads this manifest the animation with StudioId 1 will be ignored. Koikatu will read both
StudioId 1 and 2.

4. **Game experience levels and animations**
```xml
<Animation>
  <StudioId>1</StudioId>
  <AnimationName>Animation 1</AnimationName>
  <NeckDonorId>0</NeckDonorId>
  <GameSpecificOverrides>
    <KoikatsuSunshine>
      <NeckDonorId>55</NeckDonorId>
      <ExpTaii>50</ExpTaii>
    </KoikatsuSunshine>
  <GameSpecificOverrides>
</Animation>
```
For KKS the animations are available gradually one of the factors is the heroine experience. For the
game there are 3 levels None, 50%, 100%.
- **None** the animation does not require experience
- **50%** the animation requires at least 50% experience
- **100%** the animation requires 100% experience

Setting this field will apply levels to the animations. In the example above the animation will be
available after the heroine reaches 50% experience.

More detailed information in the
[wiki](https://github.com/IllusionMods/AnimationLoader/wiki/manifest.xml).
