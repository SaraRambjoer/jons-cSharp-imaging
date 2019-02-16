# jons-cSharp-imaging
Repository contains stuff I made for C# image and video manipulation. 
Dependency: dotImaging C# library

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

Currently these functions are supported: 
- Changing a pixel colour depening on how different it is to the pixel to the right of it (tested for PNG and JPEG)
- Making a video into images using the dotImaging library
- Making a set of images into an .avi video using the dotImaging library
- Changing each pixel of one colour (ignoring alpha) into a pixel of another colour
- Randomly changing a percentage of pixels of one colour into a pixel of another colour
- Going left to right and up to down over an image setting the next pixel equal to the current if their colours are sufficiently similar. Loops over several times to incrementally make the most similar equal first. 

The dotImaging library is an open source image IO library based on openCV. 
- https://github.com/dajuric/dot-imaging

YouTube playlist showing some features: 
https://www.youtube.com/watch?v=MH1lzd0Cx_4&list=PLePbZRY4O0syL1Bsk08uP1g2wdjhplxSN&index=1
