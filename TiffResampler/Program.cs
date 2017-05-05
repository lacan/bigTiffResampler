using BitMiracle.LibTiff.Classic;
using System;

namespace TiffResampler
{
    class Program
    {
        static void Main(string[] args)
        { 
            if(args.Length > 1)
            resampleImage(args[0], Convert.ToInt32(args[1]));
           // Console.ReadLine();
        }


        private static void resampleImage(string filepath, int samplingRate)
        {
            // Read original image and create new one based on original metadata

            Tiff input = Tiff.Open(filepath, "r");
            Tiff output = Tiff.Open(filepath.Substring(0, filepath.Length - 4) + "-Resampled "+samplingRate+".tif", "w");


            int tileWidthOri  = input.GetField(TiffTag.TILEWIDTH)[0].ToInt();
            int tileHeightOri = input.GetField(TiffTag.TILELENGTH)[0].ToInt();

            int samplesPerPixel = input.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToInt();
            int bitsPerSample = input.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
            
            
            int widthOri = input.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int heightOri = input.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            int width  = (int) Math.Floor((double)widthOri / samplingRate);
            int height = (int) Math.Floor((double)heightOri / samplingRate);

            int tileWidth = (int)Math.Floor((double)tileWidthOri / samplingRate);
            int tileHeight = (int)Math.Floor((double)tileHeightOri / samplingRate);

            copyTags(input, output);

            output.SetField(TiffTag.IMAGEWIDTH, width);
            output.SetField(TiffTag.IMAGELENGTH, height);
            output.SetField(TiffTag.TILEWIDTH, tileWidth);
            output.SetField(TiffTag.TILELENGTH, tileHeight);



            int x = 0;// For new created image
            int y = 0;// For new created image

            byte[] buff = new byte[input.TileSize()];
            byte[] newBuff = new byte[output.TileSize()];

            Compression compression = (Compression)input.GetField(TiffTag.COMPRESSION)[0].ToInt();

            if (compression == Compression.NONE)
            {
                // Uncomressed images are much faster to crop so it is better to use uncompressed

                for (int j = 0; j < heightOri; j += tileHeightOri)
                {
                    for (int i = 0; i < widthOri; i+= tileWidthOri)
                    {
                        input.ReadTile(buff, 0, i, j, 0, 0);
                        for (int h = 0; h < tileHeight; h++)
                        {
                            for (int w = 0; w < tileWidth; w++)
                            {
                                newBuff[3 * (h * tileWidth + w) + 0] = buff[samplingRate * (h * tileWidthOri + w) * 3 + 0];
                                newBuff[3 * (h * tileWidth + w) + 1] = buff[samplingRate * (h * tileWidthOri + w) * 3 + 1];
                                newBuff[3 * (h * tileWidth + w) + 2] = buff[samplingRate * (h * tileWidthOri + w) * 3 + 2];
                            }
                        }
                        output.WriteTile(newBuff, i / tileWidthOri * tileWidth, j / tileHeightOri * tileHeight, 0, 0);

                    }
                }
            }//If               



            output.WriteDirectory();
            output.Close();

        }////

        private static void copyTags(Tiff input, Tiff output)
        {
            for (ushort t = ushort.MinValue; t < ushort.MaxValue; ++t)
            {
                TiffTag tag = (TiffTag)t;
                FieldValue[] tagValue = input.GetField(tag);
                if (tagValue != null)
                    output.GetTagMethods().SetField(output, tag, tagValue);
            }

            int height = input.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            output.SetField(TiffTag.ROWSPERSTRIP, height);
        }

    }
}

