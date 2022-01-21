# Changelog

#### <u>(Unreleased)</u>

##### Added

- The majority of logs are enabled in config
- Log warning when moving characters
- [KKS] New labels for added animations work as expected

##### Changed

- Log of lists are done in one log call
- Reorganize the project renaming and major refactoring
- [KKS] LoadMotionList re-done complete code for buttons in order to ease extensions to plug-in

##### Fixed

- [KK] Fixed button duplication in Grid
- [@Kokaiinum] fix exception when setting NeckDonorId


### <u>v1.1.0.0 - 2021-12-25</u>

First official release with support for Koikatsu Sunshine.

##### Added

- Option to adjust character positions
- Support manifest extension in KK
- Specified processes permitted to load in KK
- Enable two more animations that needed position adjustment
- For developers if there no zipmod with a manifest for animations it will load any test 
manifests found in the directory config\AnimationLoader
- Animations can be mark individually as game specific no need to group them all in one section
- Mod can be disabled for Studio if not needed

##### Changed

- Modified the manifest.xml for katarsys animations using new extensions
- SiruPasteFiles can be specified by the name no need to be in dictionary. The dictionary is still
used it has precedence.
- Updates to manifest.xml for KKS (changes in some donors id's)
- Update to latest libraries in KK

##### Fixed

- kind of hoshi of the donor was used instead of the one defined in the manifest

### <u>v1.0.9b1 - 2021-12-10</u>

This is the first release with Koikatsu Sunshine support.  In KKS the animations are available 
depending on the Heroine experience.a

##### Added

- Koikatsu Sunshine Support
- Extensions to the manifest.xml format

##### Changed

- Solution use shared code now


###### Known Issues
