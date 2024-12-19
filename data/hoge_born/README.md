# Hoge Born
This dataset defines hypothetical working lanes, 
transport lanes, and so on, which will be helpful in developing and testing route planning software. Situated near farm De Hoge Born.

## working lanes
Lanes where the robot mut work (spraying, weeding, ..)

## transport lanes
Lanes where the robot may move without working.

## transport areas
Areas where the robot may travel freely.

## zones
Areas where specific properties are defined.

## properties
The geographic features used to define working lanes, transport lanes, transport area, and zones, may have additional properties defined.
Currently the following properties are defined for some of the features:
max_vehicle_weight: maximum weight of the vehicle of implement permitted on this lane or area (kg)
max_vehicle_width: maximum width of the vehicle and implement permitted on this lane or area (m)
max_speed: maximum speed of the vehicle permitted on this lane or area (m/s)