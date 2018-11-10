using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DotImaging;
using System.Diagnostics;
using System.Threading;

/*
MIT License

Copyright (c) [2018] [Jon Oddvar Rambjør]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace JonPicture
{
 
    class JImage
    {
        /// <summary>
        /// Sets the colour of each pixel in an copy of an image by comparing the absolute difference in RGB values from the pixel to the right and selecting from colList by values in tolList.
        /// </summary>
        /// <param name="imgPath">The path to the original image.</param>
        /// <param name="savePath">What the new image should be saved as.</param>
        /// <param name="tolList">A list of values dividing up all possible values in difference to right pixel neighbour into n+1 groups where n is length of tolList</param>
        /// <param name="colList">A list of list of integers where each list of integers is an RGB value. The list is n+1 long where n is length of tolList</param>
        /// <param name="blue">Should blue be considered in distance calculation</param>
        /// <param name="red">Should be red considered in distance calculation</param>
        /// <param name="green">Should be green considered in distance calculation</param>
        public void coloursByDistance(String imgPath, String savePath, List<int> tolList, List<List<int>> colList, bool blue=true, bool red=true, bool green=true)
        {
            if (tolList.Count != colList.Count- 1)
            {
                throw new ArgumentException("toList must be one larger than colList");
            }
            Bitmap bitmap = new Bitmap(imgPath);
            int width = bitmap.Width;
            int height = bitmap.Height;
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb); //Unlikely to work well with other pixel formats
            data.Stride = data.Width;
            int pixelWidth = Image.GetPixelFormatSize(bitmap.PixelFormat)/8;
            unsafe
            {//Goes through all pixels in a horizontal line for each horizontal line and finding out what group it should be placed in based on distance from right pixel. 
                byte* ptr = (byte*)data.Scan0;
                byte* ptr2;
                for (int y = 0; y < height-1; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        ptr2 = ptr;
                        int dist = 0;
                        int b1 = *(ptr2++);
                        int g1 = *(ptr2++);
                        int r1 = *(ptr2++);
                        ptr2 = ptr + pixelWidth;
                        int b2 = *(ptr2++);
                        int g2 = *(ptr2++);
                        int r2 = *(ptr2++);
                        dist += Math.Abs(b1 - b2) + Math.Abs(g1 - g2) + Math.Abs(r1 - r2);
                        int colIndex = 0;
                        for (int i1 = 0; i1 < tolList.Count; i1++)
                        {
                            if (dist > tolList[i1])
                            {
                                colIndex = i1 + 1;
                            }
                        }
                        List<int> selected = colList[colIndex];
                        ptr2 = ptr;
                        *(ptr2++) = (byte)selected[0];
                        *(ptr2++) = (byte)selected[1];
                        *(ptr2++) = (byte)selected[2];
                        ptr += pixelWidth;
                    }
                }
            }
            bitmap.UnlockBits(data);
            Bitmap bn = new Bitmap(bitmap);
            bitmap.Dispose();
            bn.Save(savePath, ImageFormat.Jpeg);
            bn.Dispose();
        }
        /// <summary>
        /// Calls coloursByDistance for each pair of strings in imgPaths and savePaths.
        /// </summary>
        /// <param name="imgPaths">A list of string paths to images. </param>
        /// <param name="savePaths">A list of strings signifiying where the modified images are to be saved. </param>
        /// <see cref="coloursByDistance(string, string, List{int}, List{List{int}}, bool, bool, bool)"></see>
        public void coloursByDistanceWrapper(List<String> imgPaths,List<String> savePaths, List<int> tolList, List<List<int>> colList, bool blue = true, bool red = true, bool green = true)
        {
            if (imgPaths.Count != savePaths.Count)
            {
                throw new ArgumentException("The length of imgPaths does not equal the length of savePaths");
            }
            for (int i0 = 0; i0 < imgPaths.Count; i0++)
            {
                coloursByDistance(imgPaths[i0], savePaths[i0], tolList, colList, blue, red, green);
            }
        }
        /// <summary>
        /// Uses the dotImaging library to extract each frame from a video and saving it. 
        /// </summary>
        /// <param name="savePath">Where the frames should be saved.</param>
        /// <param name="filePath">Path to videofile.</param>
        public void videoImageExtractor(String savePath, String filePath)
        {
            var reader = new FileCapture(filePath);
            reader.Open();
            reader.SaveFrames(savePath, "{0}.png");
            reader.Close();
        }
        /// <summary>
        /// Uses the dotImaging library to compile a video from frames. Video should be an .avi to guarantee it working. 
        /// </summary>
        /// <param name="imagePaths">Path to images to compile from.</param>
        /// <param name="videoPath">The path to the new video.</param>
        public void imagesToVideo(List<String> imagePaths, String videoPath)
        {
            Bitmap im = new Bitmap(imagePaths[0]);
            int width = im.Width;
            int height = im.Height;
            ImageStreamWriter videoWriter = new VideoWriter(videoPath, new DotImaging.Primitives2D.Size(width, height));
            foreach (String frame in imagePaths)
            {
               IImage image = ImageIO.LoadUnchanged(frame);
               videoWriter.Write(image);
               image.Dispose();
            }
            videoWriter.Close();
        }
        /// <summary>
        /// Used by threads to run coloursByDistance on files whose paths are in imgPaths from startIndex to endIndex.
        /// </summary>
        /// <param name="startIndex">The index of the first image to give to coloursByDistanceWrapper.</param>
        /// <param name="endIndex">The index of the last image to give to the coloursBYDistanceWrapper.</param>
        /// <param name="imgPaths">A list of paths to images.</param>
        /// <param name="savePaths">A list of paths to where new images should be saved.</param>
        /// <see cref="coloursByDistanceWrapper(List{string}, List{string}, List{int}, List{List{int}}, bool, bool, bool)"/>
        void coloursByDistanceWrapperThread(int startIndex, int endIndex, List<String> imgPaths, List<String> savePaths, List<int> tolList, List<List<int>> colList, bool blue = true, bool red = true, bool green = true)
        {
            List<String> modImgPaths = imgPaths.GetRange(startIndex, endIndex - startIndex);
            List<String> modSavePaths = savePaths.GetRange(startIndex, endIndex - startIndex);
            this.coloursByDistanceWrapper(modImgPaths, modSavePaths, tolList, colList, blue, red, green);
        }
        /// <summary>
        /// Runs coloursByDistance on a video. 
        /// </summary>
        /// <param name="sourceVideo">The path to the original video.</param>
        /// <param name="newVideoName">The path to the new video.</param>
        /// <param name="extractedFramePath">The path where extracted frames should be saved. Note: Should be an empty folder to avoid issues and unwanted file deletion.</param>
        /// <param name="modifiedFramePath">The path where modified frames should be saved. Note: Should be an empty folder to avoid issues and unwanted file deletion.</param>
        /// <param name="destroyExtractedFrames">bool deciding if extracted images should be deleted. </param>
        /// <param name="destroyModifiedFrames">bool deciding if modified images should be deleted. </param>
        static void coloursByDistanceOnVideo(String sourceVideo, String newVideoName, String extractedFramePath, String modifiedFramePath, List<int> tolList, List<List<int>> colList, bool blue = true, bool red = true, bool green = true, bool destroyExtractedFrames = true, bool destroyModifiedFrames = true)
        { 
            JImage im = new JImage();
            List<String> filePaths = new List<String>(Directory.GetFiles(extractedFramePath, "*.png", SearchOption.TopDirectoryOnly));
            //Ensures the folders for storing extracted frames and modified frames don't have other png files in them. 
            for (int i0 = 0; i0 < filePaths.Count; i0++)
            {
                File.Delete(filePaths[i0]);
            }
            List<String> savePaths = new List<String>(Directory.GetFiles(modifiedFramePath, "*.png", SearchOption.TopDirectoryOnly));
            for (int i0 = 0; i0 < savePaths.Count; i0++)
            {
                File.Delete(savePaths[i0]);
            }
            im.videoImageExtractor(extractedFramePath, sourceVideo);
            filePaths = new List<String>(Directory.GetFiles(extractedFramePath, "*.png", SearchOption.TopDirectoryOnly));
            int imgCount = filePaths.Count;
            savePaths = new List<String>();
            for (int i0 = 0; i0 < imgCount; i0++) //Gives each extracted frame a new name to make them sorted in lexiographic order instead of numeric. 
            {
                String filename = filePaths[i0].Substring(filePaths[i0].LastIndexOf("\\") + 1);
                File.Move(filePaths[i0], extractedFramePath + "00000000000".Substring(filename.Length) + filename);
            }
            filePaths = new List<String>(Directory.GetFiles(extractedFramePath, "*.png", SearchOption.TopDirectoryOnly));
            for (int i0 = 0; i0 < imgCount; i0++)
            {
                savePaths.Add(modifiedFramePath + filePaths[i0].Substring(filePaths[i0].LastIndexOf("\\") + 1));
            }
            //Divides frames in 8 groups to allow multithreading with 8 cores. 
            Thread thred1 = new Thread(() => im.coloursByDistanceWrapperThread(0 * (int)Math.Floor(imgCount / 8.0), Math.Min(imgCount, 1 * (int)Math.Floor(imgCount / 8.0)), filePaths, savePaths, tolList, colList));
            Thread thred2 = new Thread(() => im.coloursByDistanceWrapperThread(1 * (int)Math.Floor(imgCount / 8.0), Math.Min(imgCount, 2 * (int)Math.Floor(imgCount / 8.0)), filePaths, savePaths, tolList, colList));
            Thread thred3 = new Thread(() => im.coloursByDistanceWrapperThread(2 * (int)Math.Floor(imgCount / 8.0), Math.Min(imgCount, 3 * (int)Math.Floor(imgCount / 8.0)), filePaths, savePaths, tolList, colList));
            Thread thred4 = new Thread(() => im.coloursByDistanceWrapperThread(3 * (int)Math.Floor(imgCount / 8.0), Math.Min(imgCount, 4 * (int)Math.Floor(imgCount / 8.0)), filePaths, savePaths, tolList, colList));
            Thread thred5 = new Thread(() => im.coloursByDistanceWrapperThread(4 * (int)Math.Floor(imgCount / 8.0), Math.Min(imgCount, 5 * (int)Math.Floor(imgCount / 8.0)), filePaths, savePaths, tolList, colList));
            Thread thred6 = new Thread(() => im.coloursByDistanceWrapperThread(5 * (int)Math.Floor(imgCount / 8.0), Math.Min(imgCount, 6 * (int)Math.Floor(imgCount / 8.0)), filePaths, savePaths, tolList, colList));
            Thread thred7 = new Thread(() => im.coloursByDistanceWrapperThread(6 * (int)Math.Floor(imgCount / 8.0), Math.Min(imgCount, 7 * (int)Math.Floor(imgCount / 8.0)), filePaths, savePaths, tolList, colList));
            Thread thred8 = new Thread(() => im.coloursByDistanceWrapperThread(7 * (int)Math.Floor(imgCount / 8.0), imgCount, filePaths, savePaths, tolList, colList));

            thred1.Start();
            thred2.Start();
            thred3.Start();
            thred4.Start();
            thred5.Start();
            thred6.Start();
            thred7.Start();
            thred8.Start();

            thred1.Join();
            thred2.Join();
            thred3.Join();
            thred4.Join();
            thred5.Join();
            thred6.Join();
            thred7.Join();
            thred8.Join();

            im.imagesToVideo(savePaths, newVideoName);
        }
        /*
         * Below is some example code:
        public static void Main(string[] args)
        {
            List<int> list1 = new List<int>();
            list1.Add(15);
            List<int> list2 = new List<int>();
            List<List<int>> list3 = new List<List<int>>();
            list2.Add(0);
            list2.Add(0);
            list2.Add(0);
            list3.Add(list2);
            list2 = new List<int>();
            list2.Add(255);
            list2.Add(255);
            list2.Add(255);
            list3.Add(list2);
            list2 = new List<int>();
            //JImage.coloursByDistanceOnVideo("D:\\Pictures\\cyb\\walk.mp4", "D:\\Pictures\\cyb\\walk1.avi", "D:\\Pictures\\cyb\\extracted\\", "D:\\Pictures\\cyb\\processed\\", list1, list3);
            list2 = new List<int>() { 255, 238, 2 };
            List<int> list4 = new List<int>() { 255, 2, 40 };
            List<int> list5 = new List<int>() { 2, 69, 255 };
            List<int> list6 = new List<int>() { 0, 0, 0 };
            List<int> list7 = new List<int>() { 255, 255, 255 };
            list3 = new List<List<int>>() { list6, list2, list4, list5, list7 };
            list1 = new List<int>() { 3, 7, 15, 30 };
            JImage.coloursByDistanceOnVideo("D:\\Pictures\\cyb\\walk.mp4", "D:\\Pictures\\cyb\\walk3.avi", "D:\\Pictures\\cyb\\extracted\\", "D:\\Pictures\\cyb\\processed\\", list1, list3);
        }
        */
    }
}
