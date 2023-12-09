# LogModel Properties

## Modeling Overview

The **LogScannerEngine** receives a constant stream of profiles from the scan heads. After some preprocessing (e.g. unit conversion) this stream is fed into the **LogAssembler** (for most systems, this is the `SingleZoneLogAssembler`) The **LogAssembler** is responsible for monitoring the stream and detecting LogStart and LogEnd events. It also handles reverse travel, reduction of profile density and artifact removal.

Once the **LogAssembler** has detected that a log has been fully scanned, it hands over the set of profiles to the **LogModelBuilder**. This set of profiles is also called a **RawLog**. 

The **LogModelBuilder**'s job is to take a **RawLog** and construct the 3-D model, the **LogModel**. To do so, it first creates a set of [**LogSection**](logsection.md) objects. All **LogSections** become part of the [**LogModel**](logmodel.md). Additional properties of the LogModel are derived from the constituent sections. 



## LogModel Properties

### Length
:   The Length property is probably the most important measurement of LogScanner. In order to calculate it, LogScanner employs the following algorithm: 
    
    1.  take the first section (lowest z value) 
    1.  walk all profiles in ascending z-Order and check the Fit Error of the profile against the ellipse model of the section
    1. if the fit error is below the threshold `MaxFitError` set in `core.ini`, mark this profile as the `FirstGoodProfile`, otherwise keep walking
    1. take the last section (highest z value)
    1. walk all profiles of this section in descending Z-Order and check the Fit Error agains the ellipse model of this section
    1. if the fit error is below the threshold `MaxFitError` set in `core.ini`, mark this profile as the `LastGoodProfile`, otherwise keep walking.
    1. the **Length** is now calculated as the Z-Difference between `LastGoodProfile` and `FirstGoodProfile`

    !!!note 
        The algorithm is designed to exclude profiles that are distorted due to angled cuts, step cuts, hanging bark, tearouts and other imperfections of the log ends. 


### FirstGoodProfile and LastGoodProfile
 :  see [Length](#Length)

### SmallEndDiameter, SmallEndDiameterX and SmallEndDiameterY
:   (also abbreviated as SED throughout the user interface).
    During the building of the `LogModel`, the diameters of the two ends are compared, and the one with the smaller diameter is designated the *Small End*, the other the *Large End*, regardless of scanning direction. The Properties **SmallEndDiameterX** and **SmallEndDiameterY** then refer to the diameters of the section of this end. For details on how diameters of sections are defined, see [here](logsection.md/#ellipse-model)

    **SmallEndDiameter** is defined as (RawDiameterMin + RawDiameterMax)/2.0 for the small end section.

### LargeEndDiameter, LargeEndDiameterX and LargeEndDiameter
:   (also abbreviated as LED). See the previous section. 

    **LargeEndDiameter** is defined as (RawDiameterMin + RawDiameterMax)/2.0 for the large end section. 

### ButtEndFirst
:   The term ButtEnd in the industry in generally applied to the root end of the log, where the diameter is larger. A large change in diameter is also called a Butt Flare. The **ButtEndFirst** is a simple flag set to `true` if the larger end section was scanned first, e.g. has a lower Z value. 

### Sweep
:   The calculation of **Sweep** is based on the deviation of the section centers from a straight line between the first and last section. As detailed in the [LogSection](logsection.md) documentation, each section is described by a best fitting ellipse. To determine the **Sweep**, LogScanner first inscribes a line between the centroids of the first and last sections. This line intersects all other sections at their respective Z-Values (i.e. SectionCenter). For a curved log, this intersection is not identical with the section's centroid, the distance between the two is a measure of how "curved" a log is. 
    !!!note "Filtering"
        LogScanner employs some smoothing for the deviations from the center line, with a [Weighted Moving Average](https://en.wikipedia.org/wiki/Moving_average#Weighted_moving_average) algorithm. After applying the filter, the **Sweep** value is set as the largest deviation from the straight line. 

### SweepAngleRad
:   During calculation of the [Sweep](#sweep), the intersection point of the end-to-end line and the section's ellipse is used to determine the maximum deviation. The angle between horizontal and the line connection the sections centroid and the intersection point is the **Sweep Angle**, i.e. a measure of how the log is rotated. A sweep angle of 0 or π means that the log is on it's side, a sweep angle of π/2 means the log is *horns down*, and vice versa, a sweep angle of -π/2 means *horns up*. 
    !!!note "Angular units"
        In general, throughout LogScanner, angular values are stored as Radians. Variables reflect that by the "Rad" postfix. 
