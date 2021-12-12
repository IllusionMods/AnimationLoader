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

This is a example of the current format both KK and KKS will try to load the two animations. There is 
a problem though KK does not have NeckDonorId 55. To account for this there are two options.  From 
here on I will just use the minimum number of fields in the examples.

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
All exclusive animations have to be in this section.

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

