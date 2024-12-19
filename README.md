# farmsim

Dynamic simulation model of a vehicle + implement at work on a farm.

# License

The MIT license applies to the code in this repository, unless another license is indicated in a specific file of folder.

## Getting started

Download the code. Build with Visual Studio (see https://visualstudio.microsoft.com/vs/community/).
The VS build process creates an executable called farmsim.exe .
A set of example input files for farmsim is located in data/hoge_born.
Move to data/hoge_born/ and call make_cartesian.bat, this converts the .geojson files (WGS84) to a Cartesian coordinate system (in this case, the national Dutch system).
Then you can run the model by calling farmsim.exe, for example by moving to data/hoge_born/output/ and calling go.bat .

## Running the API

`dotnet run --project farmsim.Api/farmsim.Api.csproj`

### Example message

```json
{
  "TimeStep": 2,
  "TargetVelocity": 1.32,
  "PerformanceParameters": {
    "FuelPrice": 1.3,
    "LaborCost": 18,
    "MachineryRental": 15,
    "HumanTransportInvolvementPercent": 1,
    "HumanInterventionDuringOperationPercent": 0.2
  },
  "route": {
    "type": "FeatureCollection",
    "name": "vehicleroute",
    "crs": {
      "type": "name",
      "properties": {
        "name": "urn:ogc:def:crs:EPSG::28992"
      }
    },
    "features": [
      {
        "type": "Feature",
        "properties": {
          "id": 17,
          "lanetype": "working"
        },
        "geometry": {
          "type": "MultiLineString",
          "coordinates": [
            [
              [0, 0],
              [100, 0]
            ]
          ]
        }
      },
      {
        "type": "Feature",
        "properties": {
          "id": 18,
          "lanetype": "non-working"
        },
        "geometry": {
          "type": "MultiLineString",
          "coordinates": [
            [
              [100, 0],
              [100, 10]
            ]
          ]
        }
      },
      {
        "type": "Feature",
        "properties": {
          "id": 19,
          "lanetype": "working"
        },
        "geometry": {
          "type": "MultiLineString",
          "coordinates": [
            [
              [100, 10],
              [0, 10]
            ]
          ]
        }
      },
      {
        "type": "Feature",
        "properties": {
          "id": 19,
          "lanetype": "working"
        },
        "geometry": {
          "type": "MultiLineString",
          "coordinates": [
            [
              [0, 10],
              [0, 20]
            ]
          ]
        }
      },
      {
        "type": "Feature",
        "properties": {
          "id": 19,
          "lanetype": "working"
        },
        "geometry": {
          "type": "MultiLineString",
          "coordinates": [
            [
              [0, 20],
              [100, 20]
            ]
          ]
        }
      },
      {
        "type": "Feature",
        "properties": {
          "id": 19,
          "lanetype": "working"
        },
        "geometry": {
          "type": "MultiLineString",
          "coordinates": [
            [
              [100, 20],
              [100, 30]
            ]
          ]
        }
      },
      {
        "type": "Feature",
        "properties": {
          "id": 19,
          "lanetype": "working"
        },
        "geometry": {
          "type": "MultiLineString",
          "coordinates": [
            [
              [100, 30],
              [0, 30]
            ]
          ]
        }
      },
      {
        "type": "Feature",
        "properties": {
          "id": 19,
          "lanetype": "working"
        },
        "geometry": {
          "type": "MultiLineString",
          "coordinates": [
            [
              [0, 30],
              [0, 0]
            ]
          ]
        }
      }
    ]
  },
  "zones": {
    "type": "FeatureCollection",
    "name": "transportarea",
    "crs": {
      "type": "name",
      "properties": {
        "name": "urn:ogc:def:crs:EPSG::28992"
      }
    },
    "features": [
      {
        "type": "Feature",
        "properties": {
          "id": 1,
          "max_vehicle_weight": null,
          "max_vehicle_speed": 0.2,
          "max_vehicle_width": 2.4,
          "max_vehicle_height": 3.5
        },
        "geometry": {
          "type": "MultiPolygon",
          "coordinates": [
            [
              [
                [-5, -5],
                [-5, 35],
                [5, 35],
                [5, -5],
                [-5, -5]
              ]
            ]
          ]
        }
      },
      {
        "type": "Feature",
        "properties": {
          "id": 2,
          "max_vehicle_weight": 9000,
          "max_vehicle_speed": 3.5,
          "max_vehicle_width": 2.4,
          "max_vehicle_height": null
        },
        "geometry": {
          "type": "MultiPolygon",
          "coordinates": [
            [
              [
                [173318.943897876626579, 444382.562829734291881],
                [173538.159331627131905, 444366.647891922562849],
                [173582.481577795319026, 444346.454745028109755],
                [173456.017717106908094, 444312.913585776579566],
                [173318.943897876626579, 444382.562829734291881]
              ]
            ]
          ]
        }
      }
    ]
  }
}
```
