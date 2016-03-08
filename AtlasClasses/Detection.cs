//File:         Detection.cs
//Description:  This handles the server-side sensor and sends information through the communication class
//              based on detected objects
//
//              Interfaces with the xbox kinect sensor
//Programmers:  Jordan Poirier, Thom Taylor, Matthew Thiessen, Tylor McLaughlin
//Date:         5/1/2016

using System;
using System.Drawing;
using Microsoft.Kinect;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AtlasClasses
{
    public class Detection
    {
        //Please note: There are bonus points for adding portal turret
        //             sounds like "I see you," "Are you still there?" and more
        public KinectSensor _sensor;
        public DepthImageFrame startDepth;
        public Bitmap Difference;

        const float MaxDepthDistance = 4095; // max value returned
        const float MinDepthDistance = 850; // min value returned
        const float MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;

        public Detection()
        {

        }
        
        public void setUpSensors()
        {
            if(KinectSensor.KinectSensors.Count > 0)
            {
                _sensor = KinectSensor.KinectSensors[0];

                if(_sensor.Status == KinectStatus.Connected)
                {
                    _sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    _sensor.AllFramesReady += _sensor_AllFramesReady;
                    try
                    {
                        _sensor.Start();
                    }
                    catch (System.IO.IOException)
                    {
                        throw;
                    }
                }
            }
        }

        public void stopKinect(KinectSensor sensor)
        {
            if(sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
            }
        }
        
        /// <summary>
        /// Runs each frame of kinect
        /// </summary>
        /// <param name="sender">Event argument</param>
        /// <param name="e">event argument</param>
        private void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (startDepth == null)
            {
                startDepth = e.OpenDepthImageFrame();
            }
            else
            {
                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (depthFrame == null)
                    {
                        return;
                    }


                    byte[] pixels = GenerateColoredBytes(depthFrame);

                    //number of bytes per row width * 4 (B,G,R,Empty)
                    int stride = depthFrame.Width * 4;

                    ////create image
                    //Difference = (Bitmap)((new ImageConverter()).ConvertFrom(pixels));
                    
                    Difference = BitmapFromSource(
                        BitmapSource.Create(depthFrame.Width, depthFrame.Height,
                        96, 96, PixelFormats.Bgr32, null, pixels, stride));

                }
            }
        }

        /// <summary>
        /// Taken from John Gietzen on stackOverflow
        /// </summary>
        /// <param name="bitmapsource">bitmapsource to be converted to image</param>
        /// <returns></returns>
        private System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        /// <summary>
        /// Calculates depth and adds color representation
        /// </summary>
        /// <param name="depthFrame">Depth image frame</param>
        /// <returns>Returns byte array of pixels with colors representing depth</returns>
        private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
        {

            //get the raw data from kinect with the depth for every pixel
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            short[] originalDepthData = new short[startDepth.PixelDataLength];
            startDepth.CopyPixelDataTo(originalDepthData);

            //use depthFrame to create the image to display on-screen
            //depthFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];

            //Bgr32  - Blue, Green, Red, empty byte
            //Bgra32 - Blue, Green, Red, transparency 
            //You must set transparency for Bgra as .NET defaults a byte to 0 = fully transparent

            ////hardcoded locations to Blue, Green, Red (BGR) index positions       
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;


            //loop through all distances
            //pick a RGB color based on distance
            for (int depthIndex = 0, colorIndex = 0;
                depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                depthIndex++, colorIndex += 4)
            {
                //get the player (requires skeleton tracking enabled for values)
               // int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                //gets the depth value
                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                int back = originalDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;


                //.9M or 2.95'
                if (depth <= 450)
                {
                    //we are very close
                    pixels[colorIndex + BlueIndex] = 122;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 0;

                }
                else if (depth > 450 && depth < 900)
                {
                    //we are a bit further away
                    pixels[colorIndex + BlueIndex] = 255;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 0;
                }
                else if (depth > 900 && depth < 1000)
                {
                    //we are a bit further away
                    pixels[colorIndex + BlueIndex] = 122;
                    pixels[colorIndex + GreenIndex] = 122;
                    pixels[colorIndex + RedIndex] = 0;
                }
                // .9M - 2M or 2.95' - 6.56'
                else if (depth > 1000 && depth < 2000 )
                {
                    //we are a bit further away
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 255;
                    pixels[colorIndex + RedIndex] = 0;
                }
                // 2M+ or 6.56'+
                else if (depth > 2000 )
                {
                    //we are the farthest
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 255;
                }
                //else
                //{
                //    pixels[colorIndex + BlueIndex] = 0;
                //    pixels[colorIndex + GreenIndex] = 0;
                //    pixels[colorIndex + RedIndex] = 0;
                //}


                ////equal coloring for monochromatic histogram
                //byte intensity = CalculateIntensityFromDepth(depth);
                //pixels[colorIndex + BlueIndex] = intensity;
                //pixels[colorIndex + GreenIndex] = intensity;
                //pixels[colorIndex + RedIndex] = intensity;


                ////Color all players "gold"
                //if (player > 0)
                //{
                //    pixels[colorIndex + BlueIndex] = Colors.Gold.B;
                //    pixels[colorIndex + GreenIndex] = Colors.Gold.G;
                //    pixels[colorIndex + RedIndex] = Colors.Gold.R;
                //}

            }

            
            return pixels;
        }

        public static byte CalculateIntensityFromDepth(int distance)
        {
            //formula for calculating monochrome intensity for histogram
            return (byte)(255 - (255 * Math.Max(distance - MinDepthDistance, 0)
                / (MaxDepthDistanceOffset)));
        }

        /// <summary>
        /// Takes an angle and translates it between min and max tilt
        /// </summary>
        /// <param name="a">Angle from 0 - 100, percentage of elevation</param>
        public void sensorAngle(int a)
        {
            _sensor.ElevationAngle = (int)(((double)a / 100) * ((double)_sensor.MaxElevationAngle - _sensor.MinElevationAngle) + _sensor.MinElevationAngle);
        }

        public int scale(int num, int max, int small, int big)
        {
            return (int)(((double)num / max) * ((double)big - small) + small);
        }
    }
}