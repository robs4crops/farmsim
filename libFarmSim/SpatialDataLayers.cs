using NetTopologySuite.Features;
using NetTopologySuite.IO;
using NModcom;
using BitMiracle.LibTiff.Classic;

namespace libFarmSim
{
    public class SpatialDataLayers : UpdateableSimObj
    {
        [Input("X")]
        IData X;

        [Input("Y")]
        IData Y;

        [Input("ZonesJson")]
        IData ZonesJson;

        [Output("MaxVehicleSpeed")]
        IData MaxVehicleSpeed = new ConstFloatSimData(-1);

        [Output("MaxVehicleWeight")]
        IData MaxVehicleWeight = new ConstFloatSimData(-1);

        [Output("MaxVehicleHeight")]
        IData MaxVehicleHeight = new ConstFloatSimData(-1);

        [Output("MaxVehicleWidth")]
        IData MaxVehicleWidth = new ConstFloatSimData(-1);

        [Output("InfestationLevel")]
        IData InfestationLevel = new ConstFloatSimData(1);

        private List<Feature> maxSpeedFeatures = new();
        private List<Feature> maxWidthFeatures = new();
        private List<Feature> maxHeightFeatures = new();
        private List<Feature> maxWeightFeatures = new();
        public override void StartRun()
        {
            base.StartRun();

            maxSpeedFeatures.Clear();
            maxWidthFeatures.Clear();
            maxHeightFeatures.Clear();
            maxWeightFeatures.Clear();

            //string filename = "myclippeddata_cartesian.tif";
            //LoadZoneGeotiff(filename);
            //LoadZoneGeotiffGDAL(filename);

            LoadZonesRestrictions();
        }

        private void LoadZonesRestrictions()
        {
            GeoJsonReader geoReader = new GeoJsonReader();
            FeatureCollection fc = geoReader.Read<FeatureCollection>(ZonesJson.ToString());
            foreach (Feature f in fc)
            {
                IAttributesTable at = f.Attributes;
                if (at != null)
                {
                    if (at.Exists("max_vehicle_speed")
                        && at["max_vehicle_speed"] != null)
                    {
                        maxSpeedFeatures.Add(f);
                    };
                    if (at.Exists("max_vehicle_weight")
                        && at["max_vehicle_weight"] != null)
                    {
                        maxWeightFeatures.Add(f);
                    };
                    if (at.Exists("max_vehicle_width")
                        && at["max_vehicle_width"] != null)
                    {
                        maxWidthFeatures.Add(f);
                    };
                    if (at.Exists("max_vehicle_height")
                        && at["max_vehicle_height"] != null)
                    {
                        maxHeightFeatures.Add(f);
                    };

                }
            }
        }

        public override void HandleEvent(ISimEvent simEvent)
        {
            double maxVehicleSpeed = GetMaxVehicleSpeed(X.AsFloat, Y.AsFloat);
            MaxVehicleSpeed.AsFloat = maxVehicleSpeed;

            double infestationLevel = GetInfestationLevel(X.AsFloat, Y.AsFloat);
            InfestationLevel.AsFloat = infestationLevel;
        }

        private double GetMaxVehicleSpeed(double x, double y)
        {
            double maxSpeed = double.MaxValue;
            foreach (Feature f in maxSpeedFeatures)
            {
                if (f.Geometry.Contains(new NetTopologySuite.Geometries.Point(x, y)))
                {
                    double m = Convert.ToDouble(f.Attributes["max_vehicle_speed"]);
                    maxSpeed = Math.Min(maxSpeed, m);
                }
            }
            return maxSpeed;
        }
        private double GetInfestationLevel(double x, double y)
        {
            // place holder until we read this data from a map
            double infestationLevel = 0.5;
            return infestationLevel;
        }

        private void LoadZoneGeotiff(string filename)
        {
            Console.WriteLine("reading geotiff " + filename);

            using (Tiff tiff = Tiff.Open(filename, "r"))
            {
                int nWidth = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int nHeight = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                float[,] heightMap = new float[nWidth, nHeight];
                FieldValue[] modelPixelScaleTag = tiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
                FieldValue[] modelTiePointTag = tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);

                byte[] modelPixelScale = modelPixelScaleTag[1].GetBytes();
                double dW = BitConverter.ToDouble(modelPixelScale, 0);
                double dH = BitConverter.ToDouble(modelPixelScale, 8) * -1;

                byte[] modelTransformation = modelTiePointTag[1].GetBytes();
                double originLon = BitConverter.ToDouble(modelTransformation, 24);
                double originLat = BitConverter.ToDouble(modelTransformation, 32);

                double startW = originLon + dW / 2.0;
                double startH = originLat + dH / 2.0;

                Console.WriteLine("ALL DONE!");

                FieldValue[] value;

                // basic values
                value = tiff.GetField(TiffTag.IMAGEWIDTH);
                int width = value[0].ToInt();
                Console.WriteLine(width);

                value = tiff.GetField(TiffTag.IMAGELENGTH);
                int height = value[0].ToInt();
                Console.WriteLine(height);

                value = tiff.GetField(TiffTag.RESOLUTIONUNIT);
                if (value != null)
                {
                    float resu = value[0].ToFloat();
                    Console.WriteLine(resu);
                }

                value = tiff.GetField(TiffTag.XRESOLUTION);
                if (value != null)
                {
                    float dpiX = value[0].ToFloat();
                    Console.WriteLine(dpiX);
                }

                value = tiff.GetField(TiffTag.YRESOLUTION);
                if (value != null)
                {
                    float dpiY = value[0].ToInt();
                    Console.WriteLine(dpiY);
                }

                value = tiff.GetField(TiffTag.XPOSITION);
                if (value != null)
                {
                    float xpos = value[0].ToFloat();
                    Console.WriteLine(xpos);
                }

                value = tiff.GetField(TiffTag.YPOSITION);
                if (value != null)
                {
                    float ypos = value[0].ToInt();
                    Console.WriteLine(ypos);
                }

                value = tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);
                if (value != null)
                {
                    Console.WriteLine(value.GetType().FullName);
                }
                //enumerating tags
                short numberOfDirectories = tiff.NumberOfDirectories();
                for (short d = 0; d < numberOfDirectories; ++d)
                {
                    if (d != 0)
                        Console.WriteLine("---------------------------------");

                    tiff.SetDirectory((short)d);

                    Console.WriteLine("Image {0}, page {1} has following tags set:\n", filename, d);
                    for (ushort t = ushort.MinValue; t < ushort.MaxValue; ++t)
                    {
                        TiffTag tag = (TiffTag)t;
                        value = tiff.GetField(tag);
                        if (value != null)
                        {
                            for (int j = 0; j < value.Length; j++)
                            {
                                Console.WriteLine("{0}", tag.ToString());
                                Console.WriteLine("{0} : {1}", value[j].Value.GetType().ToString(), value[j].ToString());
                            }

                            Console.WriteLine();
                        }
                    }
                }

            }

            Console.WriteLine("finished");
        }
    }
}
