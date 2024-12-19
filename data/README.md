# Route planning data
Example data to support development and testing of farm simulation and route planning software.

# File format
We use GeoJSON files for geographic information. GeoJSON by default uses the WGS84 coordinate reference system (CRS); 
this is unambiguous and can be used anywhere on Earth. 

However, for easy processing we need a Cartesian coordinate system. Therefore, before running simulations and so on, we will transform 
from WGS84 to a Cartesian system which is appropriate for the location at hand. This will often be UTM, but it doesn't have to be. Fortunately, the GeoJSON standard 
does allow using another CRS (https://datatracker.ietf.org/doc/html/rfc7946#section-4). 
