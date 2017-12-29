# bigTiffResampler
A small C# program that can resample very large BigTiff files fast.

command line tool to resample large tiff files

Use:
```
TiffResampler.exe mylargeFile.tif sampling
```
The sampling is a number, usually a power of 2 to use 

## Output 
A new file in the same location called "mylargeFile Resampled [sampling].tif"
