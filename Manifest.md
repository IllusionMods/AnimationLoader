# Manifest.xml for animations

The current manifest.xml will continue to work. Both Koikatu (**KK**) and Koikatsu Sunshine
(**KKS**) will try to load the two animations.  The extensions described here are optional and made
to have more control on how AnimationLoader will load the animations.

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
      <StudioId></StudioId>
      <PathFemale></PathFemale>
      <PathMale></PathMale>
      <ControllerFemale></ControllerFemale>
      <ControllerMale></ControllerMale>
      <AnimationName></AnimationName>
      <Mode></Mode>
      <kindHoushi></kindHoushi>
      <categories>
      </categories>
      <DonorPoseId></DonorPoseId>
      <NeckDonorId></NeckDonorId>
      <IsFemaleInitiative></IsFemaleInitiative>
      <FileSiruPaste></FileSiruPaste>
      <PositionHeroine>
        <x></x>
        <y></y>
        <z></z>
      </PositionHeroine>
      <PositionPlayer>
        <x></x>
        <y></y>
        <z></z>
      </PositionPlayer>
      <GameSpecificOverrides>
        <KoikatsuSunshine>
        </KoikatsuSunshine>
      </GameSpecificOverrides>
    </Animation>

    <Koikatu>
      <Animation>
      </Animation>
    </Koikatu>

    <KoikatsuSunshine>
      <Animation>
      </Animation>
    </KoikatsuSunshine>

  </AnimationLoader>

</manifest>

```

## Extensions:

1- **PositionHeroine** and **PositionPlayer** - vectors that represent:
- x axis if left and right movement (red axis)
- y axis up and down (green axis)
- z axis forward and backwards (blue axis)

The values represent a factor or fraction of one unit of movement. For example:
- to move one unit use 1 
- to move one and half units 1.5.
- to move one fifth of a unit use 0.2

The scale may be around a meter.

2- **GameSpecificOverrides** - Since the manifest.xml is the same for KK and KKS taking as a base that
the definitions are for KK to make any adjustment for KKS a node can be added for KKS 
<KoikatsuSunshine\> with value overrides for the animations that KKS will read.

3- **Nodes <Koikatu\> and <KoikatsuSunshine\>** - any animation inside these will be read by the
corresponding game. I there are more than one animation that is exclusive or a game all can be inside
the same node. The can also be mark individually if desired.

## Examples

### Example 1

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

This is a example of the current format. There is a problem beecause KK does not have NeckDonorId 55. To
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
Exclusive animations can be all in one section or marked individually.

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
GameSpecificOverrides only work for KKS do to Unity manage code stripping having an element with xml
will trigger errors. But since the current definitions are for KK anyway this should not be a
limitation. Animations targeting KK and KKS should always start with the definition for KK then do
any fine tune for KKS with the overrides.

### Example 2

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

Move the character forward 1 unit.

The characters are move individually only one position adjustment can be made no need to have a move
configuration for both characters.

```xml
  <PositionHeroine>
    <x>0</x>
    <y>0</y>
    <z>-0.04</z>
  </PositionHeroine>
```

Here it shows move the Heroine 0.04 fraction of a unit backwards.

