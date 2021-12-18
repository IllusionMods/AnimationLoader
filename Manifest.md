# Manifest.xml for animations

The current manifest.xml will continue to work. The extensions described here are optional and made
to have control on how AnimationLoader will load the animations.


```xml
<manifest schema-ver="1">
<guid></guid>
<name></name>
<version></version>
<author></author>
<description></description>
<website></website>

  <AnimationLoader>

    <Animation>
      <StudioId>0</StudioId>
      <PathFemale>anim_imports/kplug/female/02_40.unity3d</PathFemale>
      <PathMale>anim_imports/kplug/male/02_40.unity3d</PathMale>
      <ControllerFemale>khh_f_60</ControllerFemale>
      <ControllerMale>khh_m_60</ControllerMale>
      <AnimationName>Animation 0</AnimationName>
      <Mode>houshi</Mode>
      <kindHoushi>Hand</kindHoushi>
      <categories>
        <category>LieDown</category>
        <category>Stand</category>
      </categories>
      <DonorPoseId>0</DonorPoseId>
      <NeckDonorId>0</NeckDonorId>
      <IsFemaleInitiative>false</IsFemaleInitiative>
      <FileSiruPaste>TitsPussy</FileSiruPaste>
    </Animation>

    <Animation>
      <StudioId>1</StudioId>
      <PathFemale>anim_imports/kplug/female/02_41.unity3d</PathFemale>
      <PathMale>anim_imports/kplug/male/02_41.unity3d</PathMale>
      <ControllerFemale>khh_f_61</ControllerFemale>
      <ControllerMale>khh_m_61</ControllerMale>
      <AnimationName>Animation 1</AnimationName>
      <Mode>houshi</Mode>
      <kindHoushi>Hand</kindHoushi>
      <categories>
        <category>LieDown</category>
        <category>Stand</category>
      </categories>
      <DonorPoseId>0</DonorPoseId>
      <NeckDonorId>55</NeckDonorId>
      <IsFemaleInitiative>false</IsFemaleInitiative>
      <FileSiruPaste>TitsPussy</FileSiruPaste>
    </Animation>
  
  </AnimationLoader>

</manifest>
```

This is a example of the current format both Koikatu (**KK**) and Koikatsu Sunshine (**KKS**) will
try to load the two animations. There is a problem beecause KK does not have NeckDonorId 55. To
account for this there are two options.  From here on I will just use the minimum number of fields in
the examples.

1- The animation **Animation 1** only works for KKS. In this case inside the node AnimationLoader you
can add a section ```<KoikatsuSunshine></KoikatsuSunshine>``` and move **Animation 1** there.

```xml
  <AnimationLoader>

    <Animation>
      <StudioId>0</StudioId>
      <AnimationName>Animation 0</AnimationName>
    </Animation>

    <KoikatsuSunshine>
      <Animation>
        <StudioId>1</StudioId>
        <AnimationName>Animation 1</AnimationName>
        <NeckDonorId>55</NeckDonorId>
      </Animation>
    </KoikatsuSunshine>
  
  </AnimationLoader>
```
This way only KKS will try to load **Animation 1**. KK will ignore the **KoikatsuSunshine** node. 
~~All exclusive animations have to be in this section.~~ Exclusive animations can be all in one
section or marked individually.

2- **Animation 1** works for both games but for KKS works better with NeckDonorId 55.
```xml
  <AnimationLoader>

    <Animation>
      <StudioId>0</StudioId>
      <AnimationName>Animation 0</AnimationName>
    </Animation>

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
  
  </AnimationLoader>
```
Here both animations will load in KK and KKS.  **Animation 1** will load in KK with NeeckDonorId 0
and in KKS with NeckDonorId 55.

Resuming:

Animation exclusive or a game:
```xml
    <Koikatu> <!--Koikatu exclusive-->
      <Animation>
      </Animation>
    </Koikatu>
```

Animation that works in both games but with slight differences: 

```xml
    <Animation>
      <GameSpecificOverrides>
        <Koikatu> <!--Koikatu overrides-->
        </Koikatu>
      <GameSpecificOverrides>
    </Animation>
```

The position of the characters can be adjusted.

```xml
  <PositionHeroine>
    <x>0</x>
    <y>0</y>
    <z>1</z>
  </PositionHeroine>
  <PositionPlayer>
    <x>0</x>
    <y>0</y>
    <z>1</z>
  </PositionPlayer>
```
PositionHeroine and Position Player are vectors:
- x axis if left and right movement (red axis)
- y axis up and down (green axis)
- z axis forward and backwards (blue axis)

The values represent a factor or fraction of one unit movement. For example:
- to move one unit use 1 
- to move one and half 1.5.
- to move one fifth of a unit use 0.2

The characters are move individually only one position adjustment can be made no need to have a move
configuration for both characters.

```xml
  <PositionHeroine>
    <x>0</x>
    <y>0</y>
    <z>-0.04</z>
  </PositionHeroine>
```

Here it shows move the Heroine 0.04 fractions of a unit backwards.

